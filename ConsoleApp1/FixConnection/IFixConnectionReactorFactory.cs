using System.Threading;
using Comms;
using Comms.Interfaces;
using Unity;

namespace FixConnection
{
    public interface IFixConnectionReactor<T> : IConnectionReactor<T>
    {
        
    }
    
    public interface IFixConnectionReactorFactory<T> : IConnectionReactorFactory<T>
    {
        new IFixConnectionReactor<T> CreateFixConnection(IUnityContainer container, CancellationTokenSource cancellationTokenSource);

        bool IsValidSession(string initiatorCompId, string acceptorCompId);
        FixSessionState Get(string initiatorCompId, string acceptorCompId);
    }
}