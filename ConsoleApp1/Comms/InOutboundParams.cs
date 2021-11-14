using System;
using System.Threading;
using Unity;

namespace Comms
{
    public class InOutboundParams<TIn>: IDisposable
    {
        public object StackContext { get; }
        public IConnectionCancelContext ConnectionCancelContext { get; }
        public IObservable<TIn> NextObservable { get; }
        public IUnityContainer Container { get; }

        public InOutboundParams(
            object stackContext, 
            IConnectionCancelContext connectionCancelContext,
            IObservable<TIn> nextObservable,
            IUnityContainer unityContainer)
        {
            StackContext = stackContext;
            ConnectionCancelContext = connectionCancelContext;
            NextObservable = nextObservable;
            Container = unityContainer;
        }

        public void Dispose()
        {
            Container?.Dispose();
        }
    }
}