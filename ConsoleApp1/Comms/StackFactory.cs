using System;
using System.Collections.Generic;
using System.Threading;
using Unity;

namespace Comms
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
            List<IDisposable> context, CancellationTokenSource cancellationTokenSource,
            IUnityContainer unityContainer)
        {
            var disposable = _stackComponent.CreateStackData(connectionType, cancellationTokenSource, unityContainer);
            unityContainer.AddToDisposableList(disposable);
            
            context.Add(disposable);
        }

        public IObservable<TOut> CreateInbound(StackFactoryInOutboundParams<TIn> data)
        {
            return _stackComponent.CreateInbound(
                data.ConnectionType,
                new InOutboundParams<TIn>(
                    data.StackContext[0], 
                    data.TokenSource, 
                    data.NextObservable, 
                    data.Container));
        }
        
        public IObservable<TIn> CreateOutbound(StackFactoryInOutboundParams<TOut> data)
        {
            return _stackComponent.CreateOutbound(
                data.ConnectionType,
                new InOutboundParams<TOut>(
                    data.StackContext[0], 
                    data.TokenSource,
                    data.NextObservable,
                    data.Container));
        }
    }
}