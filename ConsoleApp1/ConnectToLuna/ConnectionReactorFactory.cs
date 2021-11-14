using Comms;
using Comms.Interfaces;
using Unity;

namespace ConnectToLuna
{
    public class ConnectionReactorFactory : IConnectionReactorFactory<MessageBlock.MessageBlock>
    {
        public string Name { get; }
        public IConnectionReactor<MessageBlock.MessageBlock> Create(IUnityContainer container, IConnectionCancelContext connectionCancelContext)
        {
            return new ConnectionReactor();
        }

    }
}