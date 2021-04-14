using System.Collections.Generic;
using System.Net;
using System.Threading;
using Comms;
using Unity;

namespace FixConnection
{
    public abstract class FixConnectionReactorFactory<T> : IFixConnectionReactorFactory<T>
    {
        private readonly IDictionary<string, FixSessionState> _fixSessions;
        public FixConnectionReactorFactory(params IFixConnectionReactorFactoryParamValue<T>[] values)
        {
            _fixSessions = new Dictionary<string, FixSessionState>();
            foreach (IFixConnectionReactorFactoryParamValue<T> value in values)
            {
                value.Resolve(this);
            }
        }

        public string Name { get; }
        public IConnectionReactor<T> Create(IUnityContainer container, CancellationTokenSource cancellationTokenSource)
        {
            return CreateFixConnection(container, cancellationTokenSource);
        }

        public void AddServerConnection(
            string initiatorCompId, 
            string acceptorCompId,  
            IPAddress address, 
            int port, 
            FixVersion version)
        {
            
            _fixSessions.Add(
                SessionHelper.SessionName(initiatorCompId, acceptorCompId ), 
                new FixSessionState( initiatorCompId, acceptorCompId, address, port, version));
        }
        public void AddServerConnection(FixSessionState sessionState)
        {
            
            _fixSessions.Add(
                SessionHelper.SessionName( sessionState.InitiatorCompId, sessionState.AcceptorCompId), 
                sessionState);
        }

        public virtual IFixConnectionReactor<T> CreateFixConnection(IUnityContainer container, CancellationTokenSource cancellationTokenSource)
        {
            return new FixConnectionReactor<T>(container, cancellationTokenSource, this);
        }

        public bool IsValidSession(string initiatorCompId, string acceptorCompId)
        {
            var sessionName = SessionHelper.SessionName(initiatorCompId, acceptorCompId);
            return _fixSessions.ContainsKey(sessionName);
        }

        public FixSessionState Get(string initiatorCompId, string acceptorCompId)
        {
            var sessionName = SessionHelper.SessionName(initiatorCompId, acceptorCompId);
            return _fixSessions[sessionName];
        }
    }


    public enum FixVersion
    {
        Fix44
    }
}