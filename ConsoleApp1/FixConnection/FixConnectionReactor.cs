using System.Threading;
using Comms;
using FixConnection.Messages;
using Unity;

namespace FixConnection
{
    public class FixConnectionReactor<T> : BaseFixConnectionReactor<T>, IFixConnectionReactor<T>
    {
        private readonly IFixConnectionReactorFactory<T> _factory;
        private readonly Router _router = new Router();
        public FixConnectionReactor(
            IUnityContainer container, 
            CancellationTokenSource cancellationTokenSource,
            IFixConnectionReactorFactory<T> factory) 
            : base(container, cancellationTokenSource)
        {
            _factory = factory;
        }
        
        protected override void Action(T data)
        {
            ConnectionWriter.OnNext(data);
        }

        public override void Dispose()
        {
            _router.Dispose();
            base.Dispose();
        }
    }
}