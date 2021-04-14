using System;
using System.Collections.Generic;

namespace Comms
{

    public interface IStackCommsQueue<in TIn, in TOut> : IDisposable
    {
        void SendInbound(params Func<TIn>[] data);
        void SendOutbound(params Func<TOut>[] data);
    }
    public class StackCommsHelper<TIn, TOut> : IStackCommsQueue<TIn, TOut>
    {
        private readonly IObserver<TIn> _inObservableTin;
        private readonly IObserver<TOut> _outObservableTin;
        private readonly IDisposable _inboundDispose;
        private readonly IDisposable _outboundDispose;

        public StackCommsHelper()
        {
        }

        public StackCommsHelper(
            IObserver<TIn> inObservableTin,
            IObservable<TOut> inObservableTOut,
            Action<TOut> actIn,
            IObserver<TOut> outObservableTin,
            IObservable<TIn> outObservableTOut,
            Action<TIn> actOut)
        {
            _inObservableTin = inObservableTin;
            _outObservableTin = outObservableTin;
            _inboundDispose = inObservableTOut.Subscribe(data => { actIn?.Invoke(data); },
                exception => throw exception,
                () =>
                {
                    
                });
            _outboundDispose = outObservableTOut.Subscribe(data => { actOut?.Invoke(data); },
                exception => throw exception,
                () => {});
        }

        public void Dispose()
        {
            _inboundDispose?.Dispose();
            _outboundDispose?.Dispose();
        }

        public void SendInbound(params Func<TIn>[] data)
        {
            foreach (var d in data)
            {
                _inObservableTin?.OnNext(d());
            }
        }

        public void SendOutbound(params Func<TOut>[] data)
        {
            foreach( var d in data)
            {
                _outObservableTin?.OnNext(d());
            }
        }
    }

    public class StackCommsQueue<TIn, TOut> : IStackCommsQueue<TIn, TOut>
    {
        private readonly StackCommsHelper<TIn, TOut> _stackCommsHelper;
        private int _count;
        private readonly Queue<Tuple<int, TOut>> _inBoundQueue = new Queue<Tuple<int, TOut>>();
        private readonly Queue<Tuple<int, TIn>> _outBoundQueue = new Queue<Tuple<int, TIn>>();
        public StackCommsQueue(
            IObserver<TIn> inObservableTin, IObservable<TOut> inObservableTOut,  
            IObserver<TOut> outObservableTin, IObservable<TIn> outObservableTOut) 
        {
            _stackCommsHelper = new StackCommsHelper<TIn, TOut>(
                inObservableTin, inObservableTOut, data =>
                {
                    _inBoundQueue.Enqueue(new Tuple<int, TOut>(++_count, data));
                },
                outObservableTin, outObservableTOut, data =>
                {
                    _outBoundQueue.Enqueue(new Tuple<int, TIn>(++_count, data));
                });
        }

        public void Dispose()
        {
            _stackCommsHelper?.Dispose();
        }

        public void SendInbound(params Func<TIn>[] data)
        { 
            _stackCommsHelper.SendInbound(data);
        }

        public void SendOutbound(params Func<TOut>[] data)
        {
            _stackCommsHelper.SendOutbound(data);
        }

        public Tuple<int, TOut> NextInbound()
        {
            if (_inBoundQueue.Count == 0)
            {
                return default;
            }

            return _inBoundQueue.Dequeue();
        }
        public Tuple<int, TIn> NextOutbound()
        {
            if (_outBoundQueue.Count == 0)
            {
                return default;
            }

            return _outBoundQueue.Dequeue();
        }
    }
}