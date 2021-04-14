using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Unity;

namespace Comms
{
    public interface IStackBuilder<TIn, TOut>
    {
        IList<IDisposable> CreateContext(
            ConnectionType connectionType,
            CancellationTokenSource cancellationTokenSource, IUnityContainer unityContainer);
        IObservable<TOut> BuildIn(StackFactoryInOutboundParams<TIn> data);
        IObservable<TIn> BuildOut(StackFactoryInOutboundParams<TOut> data);
    }
}