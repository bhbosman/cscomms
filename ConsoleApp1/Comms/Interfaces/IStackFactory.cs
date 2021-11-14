using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity;

namespace Comms.Interfaces
{
    public interface IStackFactory<TIn, TOut>
    {
        IStackFactory<TIn, T2> Next<T2>(IStackComponent<TOut, T2> stackComponent);
        void CreateContext( 
            ConnectionType connectionType,
            IDictionary<string, object> context, 
            IConnectionCancelContext connectionCancelContext,
            IUnityContainer unityContainer);
        IObservable<TOut> CreateInbound(StackFactoryInOutboundParams<TIn> data);
        IObservable<TIn> CreateOutbound(StackFactoryInOutboundParams<TOut> data);
    }
}