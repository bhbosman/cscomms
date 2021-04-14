using System;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Comms.Extensions;
using Unity;

namespace Comms
{
    public enum ConnectionType
    {
        Acceptor,
        Initiator
    }
    
    public abstract class ConnectionManager<TOutFromStack> :  IStartStop, IDisposable
    {
        private readonly IConnectionReactorFactory<TOutFromStack> _connectionReactorFactory;
        protected IUnityContainer Container { get; }

        protected ConnectionManager(
            IUnityContainer parentContainer,
            IConnectionReactorFactory<TOutFromStack> connectionReactorFactory,
            params IConnectionManagerParamValue<TOutFromStack>[] additionalData) 
        {
            _connectionReactorFactory = connectionReactorFactory;
            Container = parentContainer.CreateChildContainer();
            Container.RegisterInstance(connectionReactorFactory);
            foreach (var parameter in additionalData)
            {
                parameter.Resolve(this);
            }
        }

        public void Dispose()
        {
            Container?.Dispose();
        }

        private static void CreateConnectionLayer(
            IStackBuilder<MessageBlock.MessageBlock, TOutFromStack> stackBuilder,
            IConnectionReactorFactory<TOutFromStack> connectionReactorFactory,
            CancellationTokenSource cancellationTokenSource,
            IStreamableClient streamableClient,
            IUnityContainer container, 
            ConnectionType connectionType)
        {
            var readFromStreamBottomObservable = streamableClient.ReadDataObservable(cancellationTokenSource);
            
            var writeToStreamTopObservable = new Subject<TOutFromStack>();
            container.AddToDisposableList(() =>
            {
                writeToStreamTopObservable.OnCompleted();
                writeToStreamTopObservable.Dispose();
            });

            var (disposables, readFromStreamTopObservable, writeToStreamBottomObservable) = stackBuilder.Build(
                new StackBuilderExt.BuildParams<MessageBlock.MessageBlock, TOutFromStack>(
                    connectionType,
                    cancellationTokenSource,
                    container,
                    readFromStreamBottomObservable,
                    writeToStreamTopObservable));
            foreach (var disposable in disposables)
            {
                container.AddToDisposableList(disposable);
            }
            var connectionReactor = connectionReactorFactory.Create(container, cancellationTokenSource);
            container.AddToDisposableList(connectionReactor);
            
            var toConnectionReactorSubject = new Subject<TOutFromStack>();
            container.AddToDisposableList(() =>
            {
                toConnectionReactorSubject.OnCompleted();
                toConnectionReactorSubject.Dispose();
            });
            
            var nextAction = connectionReactor.Init(
                writeToStreamTopObservable,
                toConnectionReactorSubject,
                cancellationTokenSource,
                container);
            toConnectionReactorSubject
                .Subscribe(
                    nextAction.WrapWithException(toConnectionReactorSubject.OnError),
                    toConnectionReactorSubject.OnError,
                    toConnectionReactorSubject.OnCompleted);
            
            
            readFromStreamTopObservable
                .Subscribe(
                    toConnectionReactorSubject.WriteData(cancellationTokenSource)
                        .WrapWithException(
                            KillConnectionWithExceptionAction("Inbound stack error", cancellationTokenSource)),
                    KillConnectionWithExceptionAction("Inbound stack error", cancellationTokenSource),
                    KillConnectionForAction("Inbound stack complete", cancellationTokenSource)
                );

            writeToStreamBottomObservable
                .Subscribe(
                    streamableClient.WriteData(cancellationTokenSource)
                        .WrapWithException(KillConnectionWithExceptionAction("Outbound stack error", cancellationTokenSource)),
                    KillConnectionWithExceptionAction("Outbound stack error", cancellationTokenSource),
                    KillConnectionForAction("Outbound stack complete", cancellationTokenSource));
        }

        private static Action<Exception> KillConnectionWithExceptionAction(string s, CancellationTokenSource cancellationTokenSource)
        {
            return exception =>
            {
                KillConnection(s, cancellationTokenSource, exception);
            };
        }
        private static Action KillConnectionForAction(string s, CancellationTokenSource cancellationTokenSource)
        {
            return () =>
            {
                KillConnection(s, cancellationTokenSource, null);
            };
        }

        private static void KillConnection(string s, CancellationTokenSource cancellationTokenSource, Exception exception)
        {
            Console.WriteLine(s);
            if (cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }
            cancellationTokenSource.Cancel();
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
            Action<IUnityContainer, CancellationTokenSource> registrationAction,
            Action afterCreateAction, 
            ConnectionType connectionType)
        {
            return result =>
            {
                Task.Run(() =>
                {
                    TcpClient tcpClient;
                    try
                    {
                        tcpClient = createTcpFunc(result);
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    Console.WriteLine("CreateNewConnection");
                    IStreamableClient streamableWrapper = new StreamableTcpClientImpl(tcpClient);
                    InitiateNewClient(stackBuilder, registrationAction, streamableWrapper, 0, connectionType);
                    
                    afterCreateAction();
                });
                
            };
        }

        protected IDisposable InitiateNewClient(
            IStackBuilder<MessageBlock.MessageBlock, TOutFromStack> stackBuilder,
            Action<IUnityContainer, CancellationTokenSource> registrationAction,
            IStreamableClient streamableWrapper,
            int sleepOnDispose,
            ConnectionType connectionType)
        {
            var childContainer = Container.CreateChildContainer();
            RegisterTypesWithChildContainer(childContainer);
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Token.Register(
                () =>
                {
                    Task.Run(() =>
                    {
                        childContainer?.Dispose();
                    }, CancellationToken.None);
                });
            childContainer.RegisterInstance(streamableWrapper);
            try
            {
                CreateConnectionLayer(
                    stackBuilder, 
                    _connectionReactorFactory, 
                    cancellationTokenSource, 
                    streamableWrapper, 
                    childContainer, 
                    connectionType);
                registrationAction(childContainer, cancellationTokenSource);
            }
            catch (Exception e)
            {
                cancellationTokenSource.Cancel();
                throw;
            }
            return Disposable.Create(
                () =>
                {
                    if (sleepOnDispose != 0)
                    {
                        Thread.Sleep(sleepOnDispose); 
                    }
                    cancellationTokenSource.Cancel();
                });
        }

        protected virtual void RegisterTypesWithChildContainer(IUnityContainer childContainer)
        {
            childContainer.RegisterType<IDisposableList, DisposableList>(TypeLifetime.PerContainer);
        }
    }
}