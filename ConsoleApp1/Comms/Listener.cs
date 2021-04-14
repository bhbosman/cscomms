using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Threading;
using Unity;

namespace Comms
{
    public class Listener<TOutFromStack> : ConnectionManager<TOutFromStack>
    {
        private readonly List<IpAddressAndPort<TOutFromStack>> _ipAddressAndPort = new List<IpAddressAndPort<TOutFromStack>>(); 
   
        public Listener(
            IUnityContainer parentContainer,
            IConnectionReactorFactory<TOutFromStack> connectionReactorFactory,
            params IConnectionManagerParamValue<TOutFromStack>[] parameter)
            : base(parentContainer, connectionReactorFactory, parameter)
        {

        }

        public IDisposable InitiateNewClient(
            IStackBuilder<MessageBlock.MessageBlock, TOutFromStack> stackBuilder,
            Action<IUnityContainer, CancellationTokenSource> registrationAction,
            IStreamableClient streamableWrapper,
            int sleepOnDispose)
        {
            return InitiateNewClient(
                stackBuilder,
                registrationAction,
                streamableWrapper,
                sleepOnDispose,
                ConnectionType.Acceptor);
        }

        private void StartListen(IPAddress address, int port, IStackBuilder<MessageBlock.MessageBlock, TOutFromStack> stackBuilder)
        {
            if (address == null) { throw new ArgumentNullException(nameof(address)); }
            if (port == 0) { throw new ArgumentNullException(nameof(port)); }
            var tcpListen = new TcpListener(address, port);
            tcpListen.Start();
            Container.AddToDisposableList(Disposable.Create(
                () =>
                {
                    tcpListen.Stop();
                }));
            AcceptConnection(tcpListen, stackBuilder);
        }

        public override void Start()
        {
            base.Start();
            foreach (IpAddressAndPort<TOutFromStack> ipAddressAndPort in _ipAddressAndPort)
            {
                StartListen(ipAddressAndPort.Address, ipAddressAndPort.Port, ipAddressAndPort.Builder);        
            }
        }

        private void AcceptConnection(TcpListener tcpListen, IStackBuilder<MessageBlock.MessageBlock, TOutFromStack> _stackBuilder)
        {
            tcpListen.BeginAcceptTcpClient(
                CreateNewConnection(
                    _stackBuilder,
                    tcpListen.EndAcceptTcpClient,
                    (container, source) => { },
                    () => AcceptConnection(tcpListen, _stackBuilder),
                    ConnectionType.Acceptor),
                null);
        }
        public void Assign(IPAddress address, int port, IStackBuilder<MessageBlock.MessageBlock, TOutFromStack> stackBuilder)
        {
            _ipAddressAndPort.Add(new IpAddressAndPort<TOutFromStack>(address, port, stackBuilder));
        }
    }
}