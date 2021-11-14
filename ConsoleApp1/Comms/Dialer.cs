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

        protected Dialer(
            string name,
            IUnityContainer container, 
            IStackBuilder<MessageBlock.MessageBlock, TOutFromStack> stackBuilder, 
            IConnectionReactorFactory<TOutFromStack> connectionReactorFactory) 
            : base(name, container, connectionReactorFactory)
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
            DialConnection(null);
        }
        private void DialConnection(Exception prevException)
        {
            _semaphore.WaitOne();
            var tcpClient = new TcpClient();
            var context = new DialerBeginConnectContext(IPAddress.Parse("127.0.0.1"), 3000);
            tcpClient.Client.BeginConnect(
                context.IpAddress, context.Port,
                CreateNewConnection(
                    _stackBuilder,
                    result =>
                    {
                        if (!(result.AsyncState is DialerBeginConnectContext ctx))
                        {
                            var s = $"{nameof(result)} is not of type {typeof(DialerBeginConnectContext).FullName}";
                            throw new DialerException(s, new ArgumentException(s));
                        }
                        try
                        {
                            tcpClient.EndConnect(result);
                            return tcpClient;
                        }
                        catch (Exception e)
                        {
                            _semaphore.Release();
                            throw new DialerException($"Fail on establishing a connection to {ctx.IpAddress}", e);
                        }
                    },
                    source => source.Register(() => _semaphore.Release()),
                    DialConnection,
                    ConnectionType.Initiator),
                context);
        }
    }
}