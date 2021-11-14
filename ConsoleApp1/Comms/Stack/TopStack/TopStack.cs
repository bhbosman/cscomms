using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Unity;

namespace Comms.Stack.TopStack
{
    public class TopStack<T>: IStackComponent<T, T>
    {
        public string Name => "Top";

        public object CreateStackData(
            ConnectionType connectionType, 
            IConnectionCancelContext connectionCancelContext,
            IUnityContainer unityContainer)
        {
            return null;
        }

        public IObservable<T> CreateInbound(ConnectionType connectionType, InOutboundParams<T> data)
        {
            return data.NextObservable.Select(msg => msg);
        }

        public IObservable<T> CreateOutbound(ConnectionType connectionType, InOutboundParams<T> data)
        {
            return data.NextObservable
                .Select(msg => msg)
                .ObserveOn(ThreadPoolScheduler.Instance);
        }
    }
}