using System;
using System.Threading;
using Comms.Stack;
using Unity;

namespace Comms
{

    public interface IConnectionCancelContext
    {
        void Cancel();
        bool IsCancellationRequested { get; }
        void Register(Action action);
        CancellationToken Token { get; }
        (CancellationTokenSource, CancellationToken) CreateChild(params Action[] actions);
    }
    
    
    public interface IStackComponent<TIn, TOut>
    {
        string Name { get; }
        object CreateStackData(
            ConnectionType connectionType,
            IConnectionCancelContext connectionCancelContext, 
            IUnityContainer unityContainer);
        IObservable<TOut> CreateInbound(ConnectionType connectionType, InOutboundParams<TIn> data);
        IObservable<TIn> CreateOutbound(ConnectionType connectionType, InOutboundParams<TOut> data);
    }
}