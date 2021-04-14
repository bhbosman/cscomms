using System;
using System.Threading;
using Unity;

namespace Comms.Stack.BVISStackBreaker
{
    public sealed class CompressionStackComponent : IStackComponent<MessageBlock.MessageBlock, MessageBlock.MessageBlock>
    {
        public IDisposable CreateStackData(ConnectionType connectionType, CancellationTokenSource cancellationTokenSource,
            IUnityContainer unityContainer)
        {
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                    throw new NotImplementedException();
                case ConnectionType.Initiator:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }

        public IObservable<MessageBlock.MessageBlock> CreateInbound(ConnectionType connectionType, InOutboundParams<MessageBlock.MessageBlock> data)
        {
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                    throw new NotImplementedException();
                case ConnectionType.Initiator:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }

        public IObservable<MessageBlock.MessageBlock> CreateOutbound(ConnectionType connectionType, InOutboundParams<MessageBlock.MessageBlock> data)
        {
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                    throw new NotImplementedException();
                case ConnectionType.Initiator:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }
    }
}