using System.Net;

namespace FixConnection
{
    public class AddSession<T> : IFixConnectionReactorFactoryParamValue<T>
    {
        private readonly FixSessionState _sessionState;

        public AddSession(FixSessionState sessionState)
        {
            _sessionState = sessionState;
        }

        public void Resolve(FixConnectionReactorFactory<T> connectionReactorFactory)
        {
            connectionReactorFactory.AddServerConnection(_sessionState);
        }
    }
    public class AddListenSession<T> : IFixConnectionReactorFactoryParamValue<T>
    {
        private readonly string _initiatorCompId;
        private readonly string _acceptorCompId;
        private readonly IPAddress _address;
        private readonly int _port;
        private readonly FixVersion _version;
        private readonly bool _initiate;

        public AddListenSession(string initiatorCompId, string acceptorCompId, IPAddress address, int port, FixVersion version)
        {
            _initiatorCompId = initiatorCompId;
            _acceptorCompId = acceptorCompId;
            _address = address;
            _port = port;
            _version = version;
        }

        public void Resolve(FixConnectionReactorFactory<T> connectionReactorFactory)
        {
            connectionReactorFactory.AddServerConnection(
                _initiatorCompId, 
                _acceptorCompId,
                _address, 
                _port, 
                _version);
        }
    }
}