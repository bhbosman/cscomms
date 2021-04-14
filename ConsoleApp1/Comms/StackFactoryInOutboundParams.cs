using System;
using System.Threading;
using Unity;

namespace Comms
{
    public readonly struct StackFactoryInOutboundParams<T>: IDisposable
    {
        public ConnectionType ConnectionType { get; }
        public IDisposable[] StackContext { get; }
        public CancellationTokenSource TokenSource { get; }
        public IObservable<T> NextObservable { get; }
        public IUnityContainer Container { get; }

        public StackFactoryInOutboundParams(
            ConnectionType connectionType, 
            IDisposable[] stackContext,
            CancellationTokenSource cancellationTokenSource,
            IObservable<T> nextObservable,
            IUnityContainer unityContainer)
        {
            this.ConnectionType = connectionType;
            StackContext = stackContext;
            TokenSource = cancellationTokenSource;
            NextObservable = nextObservable;
            Container = unityContainer;
        }

        public void Dispose()
        {
            TokenSource?.Dispose();
            Container?.Dispose();
            foreach (var disposable in StackContext)
            {
                disposable.Dispose();
            } 
        }
    }
}