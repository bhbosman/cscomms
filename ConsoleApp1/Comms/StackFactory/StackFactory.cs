using System;
using System.Collections.Generic;
using Comms.Interfaces;
using Unity;

namespace Comms.StackFactory
{
    public sealed class StackFactory<TIn, TOut>: IStackFactory<TIn, TOut>
    {
        private readonly IStackComponent<TIn, TOut> _stackComponent;

        public StackFactory(IStackComponent<TIn, TOut> stackComponent)
        {
            _stackComponent = stackComponent;
        }
        public  IStackFactory<TIn, T2> Next<T2>(IStackComponent<TOut, T2> stackComponent)
        {
            return new StackFactoryWithStackComponent<TIn, TOut, T2>(this, stackComponent);
        }

        public void CreateContext(
            ConnectionType connectionType,
            IDictionary<string, object> context, 
            IConnectionCancelContext connectionCancelContext,
            IUnityContainer unityContainer)
        {
            var stackData = _stackComponent.CreateStackData(connectionType, connectionCancelContext, unityContainer);
            if (stackData != null)
            {
                context.Add(_stackComponent.Name, stackData);
            }
        }

        public IObservable<TOut> CreateInbound(StackFactoryInOutboundParams<TIn> data)
        {
            data.StackContext.TryGetValue(_stackComponent.Name, out var stackData);
            return _stackComponent.CreateInbound(
                data.ConnectionType,
                new InOutboundParams<TIn>(
                    stackData, 
                    data.ConnectionCancelContext, 
                    data.NextObservable, 
                    data.Container));
        }
        
        public IObservable<TIn> CreateOutbound(StackFactoryInOutboundParams<TOut> data)
        {
            data.StackContext.TryGetValue(_stackComponent.Name, out var stackData);
            return _stackComponent.CreateOutbound(
                data.ConnectionType,
                new InOutboundParams<TOut>(
                    stackData, 
                    data.ConnectionCancelContext,
                    data.NextObservable,
                    data.Container));
        }
    }
}