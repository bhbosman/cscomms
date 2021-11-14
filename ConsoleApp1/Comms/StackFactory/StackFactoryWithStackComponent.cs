using System;
using System.Collections.Generic;
using Comms.Interfaces;
using Unity;

namespace Comms.StackFactory
{
    internal sealed class StackFactoryWithStackComponent<TIn, TOut, T2> : IStackFactory<TIn, T2>
    {
        private readonly IStackFactory<TIn, TOut> _previous;
        private readonly IStackComponent<TOut, T2> _stackComponent;

        public StackFactoryWithStackComponent(IStackFactory<TIn, TOut> previous, IStackComponent<TOut, T2> stackComponent)
        {
            _previous = previous;
            _stackComponent = stackComponent;
        }

        public IStackFactory<TIn, T21> Next<T21>(IStackComponent<T2, T21> stackComponent)
        {
            return new StackFactoryWithStackComponent<TIn, T2, T21>(this, stackComponent);
        }

        public void CreateContext(
            ConnectionType connectionType,
            IDictionary<string, object> context, 
            IConnectionCancelContext connectionCancelContext,
            IUnityContainer unityContainer)
        {
            _previous.CreateContext(connectionType,context, connectionCancelContext, unityContainer);
            var stackData = _stackComponent.CreateStackData(connectionType, connectionCancelContext, unityContainer);
            if (stackData != null)
            {
                context.Add(_stackComponent.Name,stackData);
            }
        }

        public  IObservable<T2> CreateInbound(StackFactoryInOutboundParams<TIn> data)
        {
            var obs = _previous.CreateInbound(
                new StackFactoryInOutboundParams<TIn>(
                    data.ConnectionType,
                    data.StackContext,
                    data.ConnectionCancelContext,
                    data.NextObservable,
                    data.Container));
            return _stackComponent.CreateInbound(
                data.ConnectionType,
                new InOutboundParams<TOut>(
                    data.StackContext,
                    data.ConnectionCancelContext,
                    obs,
                    data.Container));
        }

        public IObservable<TIn> CreateOutbound(StackFactoryInOutboundParams<T2> data)
        {
            var obs = _stackComponent.CreateOutbound(
                data.ConnectionType,
                new InOutboundParams<T2>(
                    data.StackContext,
                    data.ConnectionCancelContext,
                    data.NextObservable,
                    data.Container));
            return _previous.CreateOutbound(
                new StackFactoryInOutboundParams<TOut>(
                    data.ConnectionType,
                    data.StackContext,
                    data.ConnectionCancelContext,
                    obs,
                    data.Container));
        }
    }
}