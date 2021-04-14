using System;
using System.Linq;
using System.Threading;
using Unity;

namespace Comms
{
    public static class StackBuilderExt
    {
        
        public readonly struct BuildParams<TIn, TOut>
        {
            public BuildParams(
                ConnectionType connectionType, 
                CancellationTokenSource cancellationTokenSource,
                IUnityContainer unityContainer, 
                IObservable<TIn> inboundBottomObservable,
                IObservable<TOut> outboundBottomObservable)
            {
                ConnectionType = connectionType;
                CancellationTokenSource = cancellationTokenSource;
                UnityContainer = unityContainer;
                InboundBottomObservable = inboundBottomObservable;
                OutboundBottomObservable = outboundBottomObservable;
            }

            public ConnectionType ConnectionType { get; }
            public CancellationTokenSource CancellationTokenSource { get; }
            public IUnityContainer UnityContainer { get; }
            public IObservable<TIn> InboundBottomObservable { get; }
            public IObservable<TOut> OutboundBottomObservable { get; }
        }
        public static Tuple<IDisposable[], IObservable<TOut>,IObservable<TIn>> Build<TIn, TOut>(
            this IStackBuilder<TIn, TOut> stackBuilder,
            BuildParams<TIn, TOut> data)
        {
            var disposables = stackBuilder.CreateContext(
                data.ConnectionType,
                data.CancellationTokenSource, 
                data.UnityContainer).ToArray();
            var inboundTop = stackBuilder.BuildIn(
                new StackFactoryInOutboundParams<TIn>(
                    data.ConnectionType,
                    disposables, 
                    data.CancellationTokenSource, 
                    data.InboundBottomObservable,
                    data.UnityContainer));
            var outBoundBottom = stackBuilder.BuildOut(
                new StackFactoryInOutboundParams<TOut>(
                    data.ConnectionType,
                    disposables,
                    data.CancellationTokenSource, 
                    data.OutboundBottomObservable, 
                    data.UnityContainer));
            return new Tuple<IDisposable[], IObservable<TOut>, IObservable<TIn>>(
                disposables.ToArray(), 
                inboundTop,
                outBoundBottom);
        }
    }
}