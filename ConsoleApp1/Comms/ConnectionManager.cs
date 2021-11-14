using System;
using System.Collections;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Comms.Extensions;
using Comms.Interfaces;
using Unity;

namespace Comms
{
    public abstract class ConnectionManager<TOutFromStack> :  IStartStop, IDisposable
    {
        private readonly IConnectionReactorFactory<TOutFromStack> _connectionReactorFactory;
        protected IUnityContainer Container { get; }
        private ConnectionCancelContextOwner ConnectionCancelContext { get; }

        protected ConnectionManager(
            string name,
            IUnityContainer container,
            IConnectionReactorFactory<TOutFromStack> connectionReactorFactory,
            params IConnectionManagerParamValue<TOutFromStack>[] additionalData) 
        {
            _connectionReactorFactory = connectionReactorFactory;
            Container = container;
            var appCancelContext = Container.Resolve<IConnectionCancelContext>("Application");

            var (cancelTokenSourceForDialer, cancelTokenForDialer) = appCancelContext.CreateChild(Container.Dispose);
            ConnectionCancelContext = new ConnectionCancelContextOwner(
                $"ConnectionCancelContext for: {name}",
                cancelTokenSourceForDialer,
                cancelTokenForDialer); 
            Container.RegisterInstance<IConnectionCancelContext>(ConnectionCancelContext, InstanceLifetime.External);
            Container.RegisterInstance(connectionReactorFactory);
            foreach (var parameter in additionalData)
            {
                parameter.Resolve(this);
            }
        }

        public void Dispose()
        {
            ConnectionCancelContext.Dispose();
        }

        private static void CreateConnectionLayer(
            IStackBuilder<MessageBlock.MessageBlock, TOutFromStack> stackBuilder,
            IConnectionReactorFactory<TOutFromStack> connectionReactorFactory,
            IConnectionCancelContext connectionCancelContext,
            IStreamableClient streamableClient,
            IUnityContainer container, 
            ConnectionType connectionType)
        {
            var readFromStreamBottomObservable = streamableClient.ReadDataObservable(connectionCancelContext);
            
            var writeToStreamTopObservable = new Subject<TOutFromStack>();
            connectionCancelContext.Register(
                () =>
                {
                    writeToStreamTopObservable.OnCompleted();
                    writeToStreamTopObservable.Dispose();
                });

            var (disposables, readFromStreamTopObservable, writeToStreamBottomObservable) = stackBuilder.Build(
                new StackBuilderExt.BuildParams<MessageBlock.MessageBlock, TOutFromStack>(
                    connectionType,
                    connectionCancelContext,
                    container,
                    readFromStreamBottomObservable,
                    writeToStreamTopObservable));

            IConnectionReactor<TOutFromStack> connectionReactor;

            try
            {
                connectionReactor = connectionReactorFactory.Create(container, connectionCancelContext);
                connectionCancelContext.Register(connectionReactor.Dispose);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new ConnectionManagerException("", e);
            }
            
            var toConnectionReactorSubject = new Subject<TOutFromStack>();
            connectionCancelContext.Register(
                () =>
                {
                    toConnectionReactorSubject.OnCompleted();
                    toConnectionReactorSubject.Dispose();
                });
            
            var nextAction = connectionReactor.Init(
                writeToStreamTopObservable,
                toConnectionReactorSubject,
                connectionCancelContext,
                container);
            toConnectionReactorSubject
                .Subscribe(
                    nextAction.WrapWithException(toConnectionReactorSubject.OnError),
                    toConnectionReactorSubject.OnError,
                    toConnectionReactorSubject.OnCompleted);
            
            
            readFromStreamTopObservable
                .Subscribe(
                    toConnectionReactorSubject.WriteData(connectionCancelContext)
                        .WrapWithException(
                            KillConnectionWithExceptionAction("Inbound stack error", connectionCancelContext)),
                    KillConnectionWithExceptionAction("Inbound stack error", connectionCancelContext),
                    KillConnectionForAction("Inbound stack complete", connectionCancelContext)
                );

            writeToStreamBottomObservable
                .Subscribe(
                    streamableClient.WriteData(connectionCancelContext)
                        .WrapWithException(KillConnectionWithExceptionAction("Outbound stack error", connectionCancelContext)),
                    KillConnectionWithExceptionAction("Outbound stack error", connectionCancelContext),
                    KillConnectionForAction("Outbound stack complete", connectionCancelContext));
        }

        private static Action<Exception> KillConnectionWithExceptionAction(
            string s, 
            IConnectionCancelContext connectionCancelContext)
        {
            return exception =>
            {
                KillConnection(s, connectionCancelContext, exception);
            };
        }
        private static Action KillConnectionForAction(
            string s,
            IConnectionCancelContext connectionCancelContext)
        {
            return () =>
            {
                KillConnection(s, connectionCancelContext, null);
            };
        }

        private static void KillConnection(
            string s,
            IConnectionCancelContext connectionCancelContext,
            Exception exception)
        {
            Console.WriteLine(s);
            if (connectionCancelContext.IsCancellationRequested)
            {
                return;
            }
            connectionCancelContext.Cancel();
        }

        public virtual void Start()
        {
        }

        public void Stop()
        {
            Dispose();
        }

        protected AsyncCallback CreateNewConnection(
            IStackBuilder<MessageBlock.MessageBlock, TOutFromStack> stackBuilder,
            Func<IAsyncResult, TcpClient> createTcpFunc,
            Action<IConnectionCancelContext> registrationAction,
            Action<Exception> afterCreateAction, 
            ConnectionType connectionType)
        {
            return result =>
            {
                
                Task.Run(() =>
                {
                    Exception prevException = null;
                    TcpClient tcpClient = null;
                    try
                    {
                        tcpClient = createTcpFunc(result);
                    }
                    catch (DialerException e)
                    {
                        prevException = e;
                    }
                    catch (Exception e)
                    {
                        prevException = e;
                    }
                    if (tcpClient != null)
                    {
                        if (prevException == null)
                        {
                            Console.WriteLine("CreateNewConnection");
                            IStreamableClient streamableWrapper = new StreamableTcpClientImpl(tcpClient);
                            InitiateNewClient(
                                stackBuilder, 
                                registrationAction, 
                                streamableWrapper, 
                                connectionType);
                            afterCreateAction(prevException);
                        }
                    }
                });
                
            };
        }

        protected IDisposable InitiateNewClient(
            IStackBuilder<MessageBlock.MessageBlock, TOutFromStack> stackBuilder,
            Action<IConnectionCancelContext> registrationAction,
            IStreamableClient streamableWrapper,
            ConnectionType connectionType)
        {
            var childContainer = Container.CreateChildContainer();
            RegisterTypesWithChildContainer(childContainer);

            var childContext = ConnectionCancelContext.CreateChild(childContainer.Dispose);
            var connectionContextForNewInstance = new ConnectionCancelContextOwner("NewConnection", childContext.Item1, childContext.Item2);
            childContainer.RegisterInstance<IConnectionCancelContext>(connectionContextForNewInstance, InstanceLifetime.External);
            childContainer.RegisterInstance(streamableWrapper);
            try
            {
                CreateConnectionLayer(
                    stackBuilder, 
                    _connectionReactorFactory, 
                    connectionContextForNewInstance, 
                    streamableWrapper, 
                    childContainer, 
                    connectionType);
                registrationAction(connectionContextForNewInstance);
            }
            catch (Exception e)
            {
                connectionContextForNewInstance.Cancel();
                throw;
            }
            
            return Disposable.Create(connectionContextForNewInstance.Cancel);
        }

        protected virtual void RegisterTypesWithChildContainer(IUnityContainer childContainer)
        {
            childContainer.RegisterType<IDisposableList, DisposableList>(TypeLifetime.PerContainer);
        }
    }
}