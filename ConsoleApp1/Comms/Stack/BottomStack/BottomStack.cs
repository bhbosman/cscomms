using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Unity;

namespace Comms.Stack.BottomStack
{
    public sealed class BottomStack<TInboundOutput> : IStackComponent<MessageBlock.MessageBlock, TInboundOutput>
    {
        private  long _inByteCount;
        private  long _outByteCount;
        private readonly Func<MessageBlock.MessageBlock, TInboundOutput> _inFunc;
        private readonly Func<TInboundOutput, MessageBlock.MessageBlock> _outFunc;
    
        public BottomStack(Func<MessageBlock.MessageBlock, TInboundOutput> inFunc, Func<TInboundOutput, MessageBlock.MessageBlock> outFunc)
        {
            _inFunc = inFunc;
            _outFunc = outFunc;
        }

        public IDisposable CreateStackData(
            ConnectionType connectionType,
            CancellationTokenSource cancellationTokenSource,
            IUnityContainer unityContainer)
        {
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                case ConnectionType.Initiator:
                    return Disposable.Create(() =>
                    {
                    });
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }
        public IObservable<TInboundOutput> CreateInbound(
            ConnectionType connectionType,
            InOutboundParams<MessageBlock.MessageBlock> data)
        {
            switch (connectionType)
            {
                case ConnectionType.Initiator:
                case ConnectionType.Acceptor:
                    return data.NextObservable.Select(
                            block =>
                            {
                                _inByteCount += block.AvailableRead;
                                return _inFunc(block);
                            })
                        .ObserveOn(ThreadPoolScheduler.Instance);
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }

        public IObservable<MessageBlock.MessageBlock> CreateOutbound(
            ConnectionType connectionType,
            InOutboundParams<TInboundOutput> data)
        {
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                case ConnectionType.Initiator:
                    return data.NextObservable.Select(output =>
                    {
                        var mb = _outFunc(output);
                        _outByteCount += mb.AvailableRead;
                        return mb;
                    });
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }
    }
}