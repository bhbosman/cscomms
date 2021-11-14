using System;
using System.Collections.Generic;
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
                IConnectionCancelContext connectionCancelContext,
                IUnityContainer unityContainer, 
                IObservable<TIn> inboundBottomObservable,
                IObservable<TOut> outboundBottomObservable)
            {
                ConnectionType = connectionType;
                ConnectionCancelContext = connectionCancelContext;
                UnityContainer = unityContainer;
                InboundBottomObservable = inboundBottomObservable;
                OutboundBottomObservable = outboundBottomObservable;
            }

            public ConnectionType ConnectionType { get; }
            public IConnectionCancelContext ConnectionCancelContext{ get; }
            public IUnityContainer UnityContainer { get; }
            public IObservable<TIn> InboundBottomObservable { get; }
            public IObservable<TOut> OutboundBottomObservable { get; }
        }
        public static Tuple<IDictionary<string, object>, IObservable<TOut>,IObservable<TIn>> Build<TIn, TOut>(
            this IStackBuilder<TIn, TOut> stackBuilder,
            BuildParams<TIn, TOut> data)
        {
            var dict = stackBuilder.CreateContext(
                data.ConnectionType,
                data.ConnectionCancelContext, 
                data.UnityContainer);
            var inboundTop = stackBuilder.BuildIn(
                new StackFactoryInOutboundParams<TIn>(
                    data.ConnectionType,
                    dict, 
                    data.ConnectionCancelContext, 
                    data.InboundBottomObservable,
                    data.UnityContainer));
            var outBoundBottom = stackBuilder.BuildOut(
                new StackFactoryInOutboundParams<TOut>(
                    data.ConnectionType,
                    dict,
                    data.ConnectionCancelContext, 
                    data.OutboundBottomObservable, 
                    data.UnityContainer));
            return new Tuple<IDictionary<string, object>, IObservable<TOut>, IObservable<TIn>>(
                dict, 
                inboundTop,
                outBoundBottom);
        }
    }
}