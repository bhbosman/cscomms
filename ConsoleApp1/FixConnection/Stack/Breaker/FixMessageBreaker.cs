using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Comms;
using Comms.Extensions;
using FixConnection.Messages;
using QuickFix.FIX44;
using Unity;

namespace FixConnection.Stack.Breaker
{
    public class FixMessageBreaker: IStackComponent<MessageBlock.MessageBlock, ParsedFixMessage>
    {
        public IDisposable CreateStackData(
            ConnectionType connectionType,
            CancellationTokenSource cancellationTokenSource, IUnityContainer unityContainer)
        {
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                case ConnectionType.Initiator:
                    return Disposable.Empty;
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }

        public IObservable<ParsedFixMessage> CreateInbound(
            ConnectionType connectionType,
            InOutboundParams<MessageBlock.MessageBlock> data)
        {
            
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                case ConnectionType.Initiator:
                    return Observable.Create<ParsedFixMessage>(observer =>
                    {
                        var carriageReturnInboundBreaker = new FixConnectionInboundBreaker(
                            observer.OnNext);
                        data.NextObservable.Subscribe(
                            ((Action<MessageBlock.MessageBlock>)carriageReturnInboundBreaker.OnNext).WrapWithException(observer.OnError),
                            observer.OnError,
                            observer.OnCompleted);
                        
                        return Disposable.Create(carriageReturnInboundBreaker.Dispose);
                    }).ObserveOn(ThreadPoolScheduler.Instance);
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }

        public IObservable<MessageBlock.MessageBlock> CreateOutbound(
            ConnectionType connectionType,
            InOutboundParams<ParsedFixMessage> data)
        {
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                case ConnectionType.Initiator:
                    return data.NextObservable.Select(
                        block => data.TokenSource.IsCancellationRequested
                            ? null
                            : block.CompleteFixMessage);
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }
    }
}