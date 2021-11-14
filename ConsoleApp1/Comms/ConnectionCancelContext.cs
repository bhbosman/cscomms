using System;
using System.Threading;

namespace Comms
{
    public class ConnectionCancelContext : IConnectionCancelContext
    {
        public string Name { get; private set; }
        protected CancellationTokenSource Source { get; }

        protected ConnectionCancelContext(CancellationTokenSource source, CancellationToken token, string name)
        {
            Source = source;
            Token = token;
            Name = name;
        }

        public void Cancel()
        {
            Source.Cancel();
        }

        public bool IsCancellationRequested => Source.IsCancellationRequested;
        public void Register(Action action)
        {
            Token.Register(action);
        }

        public CancellationToken Token { get; }
        public (CancellationTokenSource, CancellationToken) CreateChild(params Action[] actions)
        {
            var cancelTokenSource = new CancellationTokenSource();
            var cancelToken = CancellationTokenSource.CreateLinkedTokenSource(Token, cancelTokenSource.Token).Token;

            foreach (var action in actions)
            {
                cancelToken.Register(action);
            }
            return (cancelTokenSource, cancelToken);
        }
    } 
    public class ConnectionCancelContextOwner : ConnectionCancelContext, IDisposable
    {
        public void Dispose()
        {
            Source.Dispose();
        }

        public ConnectionCancelContextOwner(string name, CancellationTokenSource source) : this(name, source, source.Token)    
        {
        }

        public ConnectionCancelContextOwner(
            string name,
            CancellationTokenSource cancelTokenSource, 
            CancellationToken cancelToken) : base(cancelTokenSource, cancelToken, name)
        {
        }
    }
}