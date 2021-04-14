using System;
using System.Threading;
using Comms;
using Unity;

namespace FixConnection
{
    public abstract class BaseFixConnectionReactor<T> : IConnectionReactor<T>
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public IUnityContainer Container { get; }
        // ReSharper disable once MemberCanBePrivate.Global
        public CancellationTokenSource CancellationTokenSource { get; }
        // ReSharper disable once MemberCanBeProtected.Global
        public IObserver<T> ConnectionWriter => _connectionWriter;
        public IObserver<T> ToReactor => _toReactor;

        private IObserver<T> _connectionWriter;
        private IObserver<T> _toReactor;

        protected BaseFixConnectionReactor(IUnityContainer container, CancellationTokenSource cancellationTokenSource)
        {
            Container = container;
            CancellationTokenSource = cancellationTokenSource;
        }

        public virtual void Dispose()
        {
        }
        public Action<T> Init(IObserver<T> connectionWriter, IObserver<T> toReactor,
            CancellationTokenSource cancellationTokenSource,
            IUnityContainer container)
        {
            _connectionWriter = connectionWriter;
            _toReactor = toReactor;
            return Action;
        }
        protected abstract void Action(T data);
    }
}