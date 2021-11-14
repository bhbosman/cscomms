using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Unity;

namespace Comms.Stack.BVISStackBreaker
{
    public sealed class BVISStackComponent : IStackComponent<MessageBlock.MessageBlock, MessageBlock.MessageBlock>
    {
        
        private readonly Func<MessageBlock.MessageBlock, MessageBlock.MessageBlock> _inFunc;
        private readonly Func<MessageBlock.MessageBlock, MessageBlock.MessageBlock> _outFunc;

        public BVISStackComponent(
            Func<MessageBlock.MessageBlock, MessageBlock.MessageBlock> inFunc, 
            Func<MessageBlock.MessageBlock, MessageBlock.MessageBlock> outFunc)
        {
            _inFunc = inFunc;
            _outFunc = outFunc;
        }

        public string Name => "BVIS";

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
                case ConnectionType.Initiator:
                case ConnectionType.Acceptor:
                    return data.NextObservable.Select(
                            block => _inFunc(block))
                        .ObserveOn(ThreadPoolScheduler.Instance);
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }

        public IObservable<MessageBlock.MessageBlock> CreateOutbound(ConnectionType connectionType, InOutboundParams<MessageBlock.MessageBlock> data)
        {
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                case ConnectionType.Initiator:
                    return data.NextObservable.Select(output => _outFunc(output));
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }
    }
}