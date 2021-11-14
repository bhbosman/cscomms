using System.Net;
using Comms.Interfaces;

namespace Comms
{
    public class SetIpAddress<T> : IConnectionManagerParamValue<T>
    {
        private readonly IPAddress _address;
        private readonly int _port;
        private readonly IStackBuilder<MessageBlock.MessageBlock, T> _stackBuilder;
        public SetIpAddress(
            IPAddress address,
            int port,
            IStackBuilder<MessageBlock.MessageBlock, T> stackBuilder)
        {
            _address = address;
            _port = port;
            _stackBuilder = stackBuilder;
        }

        public void Resolve(ConnectionManager<T> connectionManager)
        {
            if (connectionManager is Listener<T>  listener)
            {
                listener.Assign(_address, _port, _stackBuilder);
            }
        }
    }
}