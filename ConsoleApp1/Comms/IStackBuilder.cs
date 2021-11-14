using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Unity;

namespace Comms
{
    public interface IStackBuilder<TIn, TOut>
    {
        IDictionary<string, object> CreateContext(
            ConnectionType connectionType,
            IConnectionCancelContext connectionCancelContext,
            IUnityContainer unityContainer);
        IObservable<TOut> BuildIn(StackFactoryInOutboundParams<TIn> data);
        IObservable<TIn> BuildOut(StackFactoryInOutboundParams<TOut> data);
    }
}