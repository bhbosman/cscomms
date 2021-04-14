using System.Net;

namespace Comms
{
    public class IpAddressAndPort<T>
    {
        public IStackBuilder<MessageBlock.MessageBlock, T> Builder { get; }
        public IPAddress Address { get; }
        public int Port { get; }

        public IpAddressAndPort(
            IPAddress address, 
            int port,
            IStackBuilder<MessageBlock.MessageBlock, T> stackBuilder)
        {
            Builder = stackBuilder;
            Address = address;
            Port = port;
        }
    }
}