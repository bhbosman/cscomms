using System;
using System.Threading;
using Unity;

namespace Comms.Interfaces
{
    public interface IConnectionReactor<T>: IDisposable
    {
        Action<T> Init(
            IObserver<T> connectionWriter, 
            IObserver<T> toReactor, 
            IConnectionCancelContext connectionCancelContext, 
            IUnityContainer container);
    }
}