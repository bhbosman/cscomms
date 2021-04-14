using Comms;
using Unity;

namespace FixConnection
{
    public class FixDialer<T> : Dialer<T>
    {
        public FixDialer(IUnityContainer container, IStackBuilder<MessageBlock.MessageBlock, T> stackBuilder, IConnectionReactorFactory<T> connectionReactorFactory) : base(container, stackBuilder, connectionReactorFactory)
        {
        }
    }
}