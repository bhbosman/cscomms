using System;
using System.Threading;
using Unity;

namespace FixConnection
{
    public sealed class FixConnectionReactorWithAction<T> : BaseFixConnectionReactor<T>
    {
        private readonly Action<FixConnectionReactorWithAction<T>, T> _onData;

        public FixConnectionReactorWithAction(IUnityContainer container, CancellationTokenSource cancellationTokenSource, Action<FixConnectionReactorWithAction<T>, T> onData) 
            : base(container, cancellationTokenSource)
        {
            _onData = onData;
        }

        protected override void Action(T data)
        {
            _onData?.Invoke(this, data);
        }
    }
}