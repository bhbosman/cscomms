using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Unity;

namespace Comms
{
    public class Dialer<TOutFromStack> : ConnectionManager<TOutFromStack>
    {
        private readonly IStackBuilder<MessageBlock.MessageBlock, TOutFromStack> _stackBuilder;

        private readonly Semaphore _semaphore;

        public Dialer(
            IUnityContainer container, 
            IStackBuilder<MessageBlock.MessageBlock, TOutFromStack> stackBuilder, 
            IConnectionReactorFactory<TOutFromStack> connectionReactorFactory) 
            : base(container, connectionReactorFactory)
        {
            _stackBuilder = stackBuilder;
            _semaphore = new Semaphore(1, 1);
        }

   
        public override void Start()
        {
            base.Start();
            DialConnection();
        }

        private void DialConnection()
        {
            _semaphore.WaitOne();
            var tcpClient = new TcpClient();
            tcpClient.BeginConnect(
                IPAddress.Parse("127.0.0.1"),
                3010,
                CreateNewConnection(
                    _stackBuilder,
                    result =>
                    {
                        try
                        {
                            tcpClient.EndConnect(result);
                            return tcpClient;
                        }
                        catch (Exception)
                        {
                            _semaphore.Release();
                            throw;
                        }
                    },
                    (container,
                        source) =>
                    {
                        source.Token.Register(() => { _semaphore.Release(); });
                    },
                    DialConnection,
                    ConnectionType.Initiator),
                null);
        }
    }
    public class Dialer : Dialer<MessageBlock.MessageBlock>
    {
        public Dialer(IUnityContainer container, IStackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock> stackBuilder, IConnectionReactorFactory<MessageBlock.MessageBlock> connectionReactorFactory) 
            : base(container, stackBuilder, connectionReactorFactory)
        {
        }
    }
    
}