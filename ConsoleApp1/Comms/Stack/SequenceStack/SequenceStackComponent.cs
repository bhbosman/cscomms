using System;
using System.Threading;
using Unity;

namespace Comms.Stack.BVISStackBreaker
{
    public sealed class SequenceStackComponent : IStackComponent<MessageBlock.MessageBlock, MessageBlock.MessageBlock>
    {
        public string Name => "Sequence";

        public object CreateStackData(
            ConnectionType connectionType, 
            IConnectionCancelContext connectionCancelContext,
            IUnityContainer unityContainer)
        {
            return null;
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