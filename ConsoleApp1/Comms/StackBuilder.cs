using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Comms.Interfaces;
using Comms.StackFactory;
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
 
        public IDictionary<string, object> CreateContext(
            ConnectionType connectionType,
            IConnectionCancelContext connectionCancelContext,
            IUnityContainer unityContainer)
        {
            IDictionary<string, object> dict = new Dictionary<string, object>();
            _factory.CreateContext(connectionType, dict, connectionCancelContext, unityContainer);
            return dict;
        }
    }
}