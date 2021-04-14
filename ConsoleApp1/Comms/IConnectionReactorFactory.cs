using System.Threading;
using Unity;

namespace Comms
{
    public interface IConnectionReactorFactory<T>
    {
        string Name { get; }
        IConnectionReactor<T> Create(IUnityContainer container, CancellationTokenSource cancellationTokenSource);
    }
}