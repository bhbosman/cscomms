using Comms;
using FixConnection.FixDirectoryServices;
using FixConnection.Messages;
using Unity;

namespace FixConnection
{
    public class FixListener<TOutFromStack> : Listener<TOutFromStack>
    {
        public FixListener(
            IUnityContainer parentContainer,
            IFixConnectionReactorFactory<TOutFromStack> connectionReactorFactory,
            params IConnectionManagerParamValue<TOutFromStack>[] parameter) 
            : base(parentContainer, connectionReactorFactory, parameter)
        {
            Container.RegisterInstance(connectionReactorFactory);
            Container.RegisterFixDictionaries();
        }
    }
}