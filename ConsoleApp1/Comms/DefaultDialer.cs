using Unity;

namespace Comms
{
    public sealed class DefaultDialer : Dialer<MessageBlock.MessageBlock>
    {
        public DefaultDialer(
            string name,
            IUnityContainer container,
            IStackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock> stackBuilder, 
            IConnectionReactorFactory<MessageBlock.MessageBlock> connectionReactorFactory) 
            : base(name, container, stackBuilder, connectionReactorFactory)
        {
        }
    }
}