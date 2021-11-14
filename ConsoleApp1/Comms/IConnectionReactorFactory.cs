using System.Threading;
using Comms.Interfaces;
using Unity;

namespace Comms
{
    public interface IConnectionReactorFactory<T>
    {
        string Name { get; }
        IConnectionReactor<T> Create(IUnityContainer container, IConnectionCancelContext connectionCancelContext);
    }
}