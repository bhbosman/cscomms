using System;
using System.Threading;
using Unity;

namespace Comms
{
    public class InOutboundParams<TIn>: IDisposable
    {
        public IDisposable StackContext { get; }
        public CancellationTokenSource TokenSource { get; }
        public IObservable<TIn> NextObservable { get; }
        public IUnityContainer Container { get; }

        public InOutboundParams(IDisposable stackContext, CancellationTokenSource cancellationTokenSource, IObservable<TIn> nextObservable, IUnityContainer unityContainer)
        {
            StackContext = stackContext;
            TokenSource = cancellationTokenSource;
            NextObservable = nextObservable;
            Container = unityContainer;
        }

        public void Dispose()
        {
            TokenSource?.Dispose();
            Container?.Dispose();
            StackContext?.Dispose();
        }
    }
}