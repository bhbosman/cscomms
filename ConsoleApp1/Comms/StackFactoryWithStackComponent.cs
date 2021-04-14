using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity;

namespace Comms
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
            List<IDisposable> context, CancellationTokenSource cancellationTokenSource,
            IUnityContainer unityContainer)
        {
            _previous.CreateContext(connectionType,context, cancellationTokenSource, unityContainer);
            context.Add(_stackComponent.CreateStackData(connectionType, cancellationTokenSource, unityContainer));
        }

        public  IObservable<T2> CreateInbound(StackFactoryInOutboundParams<TIn> data)
        {
            var stackForPrevious = new IDisposable[data.StackContext.Length-1];
            Array.Copy(
                data.StackContext,
                0,
                stackForPrevious,
                0,
                stackForPrevious.Length);
            var obs = _previous.CreateInbound(
                new StackFactoryInOutboundParams<TIn>(
                    data.ConnectionType,
                    stackForPrevious,
                    data.TokenSource,
                    data.NextObservable,
                    data.Container));
            return _stackComponent.CreateInbound(
                data.ConnectionType,
                new InOutboundParams<TOut>(
                    data.StackContext.Last(),
                    data.TokenSource,
                    obs,
                    data.Container));
        }

        public IObservable<TIn> CreateOutbound(StackFactoryInOutboundParams<T2> data)
        {
            var obs = _stackComponent.CreateOutbound(
                data.ConnectionType,
                new InOutboundParams<T2>(
                    data.StackContext.Last(),
                    data.TokenSource,
                    data.NextObservable,
                    data.Container));
            var stackForPrevious = new IDisposable[data.StackContext.Length - 1];
            Array.Copy(
                data.StackContext,
                0,
                stackForPrevious,
                0, stackForPrevious.Length);
            return _previous.CreateOutbound(
                new StackFactoryInOutboundParams<TOut>(
                    data.ConnectionType,
                    stackForPrevious,
                    data.TokenSource,
                    obs,
                    data.Container));
        }
    }
}