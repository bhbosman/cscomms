using System;
using System.Collections.Generic;
using System.Threading;
using Unity;

namespace Comms
{
    public class StackBuilder<TIn, TOut> : IStackBuilder<TIn, TOut>
    {
        private readonly IStackFactory<TIn, TOut> _factory;

        public StackBuilder(IStackFactory<TIn, TOut> factory)
        {
            _factory = factory;
        }


        public IObservable<TOut> BuildIn(StackFactoryInOutboundParams<TIn> data)
        {
            return _factory.CreateInbound(data);
        }

        public IObservable<TIn> BuildOut(StackFactoryInOutboundParams<TOut> data)
        {
            return _factory.CreateOutbound(data);
        }
 
        public IList<IDisposable> CreateContext(
            ConnectionType connectionType,
            CancellationTokenSource cancellationTokenSource, IUnityContainer unityContainer)
        {
            List<IDisposable> disposables = new List<IDisposable>();
            _factory.CreateContext(connectionType, disposables, cancellationTokenSource, unityContainer);
            return disposables;
        }
    }
}