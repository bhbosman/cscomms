using System.Net;

namespace Comms
{
    class DialerBeginConnectContext
    {
        public IPAddress IpAddress { get; }
        public int Port { get; }

        public DialerBeginConnectContext(IPAddress ipAddress, int port)
        {
            IpAddress = ipAddress;
            Port = port;
        }
    }
}