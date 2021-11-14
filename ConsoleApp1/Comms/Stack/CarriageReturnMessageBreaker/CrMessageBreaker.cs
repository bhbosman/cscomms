using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Comms.Extensions;
using Unity;

namespace Comms.Stack.CarriageReturnMessageBreaker
{
    
    public sealed class CrMessageBreaker : IStackComponent<MessageBlock.MessageBlock, MessageBlock.MessageBlock>
    {
        public string Name => "CrMessageBreaker";

        public object CreateStackData(
            ConnectionType connectionType,
            IConnectionCancelContext connectionCancelContext,
            IUnityContainer unityContainer)
        {
            return null;
        }

        public IObservable<MessageBlock.MessageBlock> CreateInbound(
            ConnectionType connectionType, 
            InOutboundParams<MessageBlock.MessageBlock> data)
        {
            switch (connectionType)
            {
                case ConnectionType.Initiator:
                case ConnectionType.Acceptor:
                    return Observable.Create<MessageBlock.MessageBlock>(observer =>
                    {
                        var carriageReturnInboundBreaker = new CarriageReturnInboundBreaker(
                            observer.OnNext, 
                            observer.OnError, 
                            observer.OnCompleted);
                        ; 
                        data.NextObservable.Subscribe(
                            ((Action<MessageBlock.MessageBlock>)carriageReturnInboundBreaker.OnNext).WrapWithException(observer.OnError),
                            observer.OnError,
                            observer.OnCompleted);
                        return Disposable.Create(()=>
                        {
                            carriageReturnInboundBreaker?.Dispose();
                            data.Dispose();
                        });
                    }).ObserveOn(ThreadPoolScheduler.Instance);
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }

        public IObservable<MessageBlock.MessageBlock> CreateOutbound(
            ConnectionType connectionType,
            InOutboundParams<MessageBlock.MessageBlock> data)
        {
            switch (connectionType)
            {
                case ConnectionType.Initiator:
                case ConnectionType.Acceptor:
                    return Observable.Create<MessageBlock.MessageBlock>(observer =>
                    {
                        var carriageReturnOutboundBreaker = new CarriageReturnOutboundBreaker(
                            observer.OnNext, 
                            observer.OnError, 
                            observer.OnCompleted);
                        ; 
                        data.NextObservable.Subscribe(
                            ((Action<MessageBlock.MessageBlock>)carriageReturnOutboundBreaker.OnNext).WrapWithException(observer.OnError),
                            observer.OnError,
                            observer.OnCompleted);
                        return Disposable.Create(()=>
                        {
                            carriageReturnOutboundBreaker.Dispose();
                            data.Dispose();
                        });
                    }).ObserveOn(ThreadPoolScheduler.Instance);
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }
    }
}