using System;
using System.Threading;
using Comms.Stack;
using Unity;

namespace Comms
{
     
    public interface IStackComponent<TIn, TOut>
    {
        IDisposable CreateStackData(ConnectionType connectionType, CancellationTokenSource cancellationTokenSource, IUnityContainer unityContainer);
        IObservable<TOut> CreateInbound(ConnectionType connectionType, InOutboundParams<TIn> data);
        IObservable<TIn> CreateOutbound(ConnectionType connectionType, InOutboundParams<TOut> data);
    }

    public static class StackComponentExtension
    {
        public static IStackFactory<TIn, TOut> CreateFactory<TIn, TOut>(this IStackComponent<TIn, TOut> instance)
        {
            return new StackFactory<TIn, TOut>(instance);
        }
    }
}