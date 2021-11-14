using System;
using System.Collections.Generic;
using System.Threading;
using Unity;

namespace Comms
{
    public readonly struct StackFactoryInOutboundParams<T>: IDisposable
    {
        public ConnectionType ConnectionType { get; }
        public IDictionary<string, object> StackContext { get; }
        public IConnectionCancelContext ConnectionCancelContext { get; }
        public IObservable<T> NextObservable { get; }
        public IUnityContainer Container { get; }

        public StackFactoryInOutboundParams(
            ConnectionType connectionType, 
            IDictionary<string, object> stackContext,
            IConnectionCancelContext connectionCancelContext,
            IObservable<T> nextObservable,
            IUnityContainer unityContainer)
        {
            this.ConnectionType = connectionType;
            StackContext = stackContext;
            ConnectionCancelContext = connectionCancelContext;
            NextObservable = nextObservable;
            Container = unityContainer;
        }

        public void Dispose()
        {
            ConnectionCancelContext.Cancel();
            Container?.Dispose();
        }
    }
}