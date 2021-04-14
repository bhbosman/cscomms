using System;
using System.Threading;
using Unity;

namespace Comms
{
    public interface IConnectionReactor<T>: IDisposable
    {
        Action<T> Init(IObserver<T> connectionWriter, IObserver<T> toReactor, CancellationTokenSource cancellationTokenSource, IUnityContainer container);
    }
}