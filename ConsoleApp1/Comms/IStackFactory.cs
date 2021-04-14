using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Unity;

namespace Comms
{
    public interface IStackFactory<TIn, TOut>
    {
        IStackFactory<TIn, T2> Next<T2>(IStackComponent<TOut, T2> stackComponent);
        void CreateContext( 
            ConnectionType connectionType,
            List<IDisposable> context, CancellationTokenSource cancellationTokenSource, IUnityContainer unityContainer);
        IObservable<TOut> CreateInbound(StackFactoryInOutboundParams<TIn> data);
        IObservable<TIn> CreateOutbound(StackFactoryInOutboundParams<TOut> data);
    }
}