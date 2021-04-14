using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Unity;

namespace Comms.Stack.AnyStack
{
    public class AnyStackComponent<TIn, TOut> : IStackComponent<TIn, TOut>
    {
        private readonly Func<TIn, TOut> _inFunc;
        private readonly Func<TOut, TIn> _outFunc;
        private readonly Func<IDisposable> _createContext;

        public AnyStackComponent(Func<TIn, TOut> inFunc, Func<TOut, TIn> outFunc, Func<IDisposable> createContext)
        {
            _inFunc = inFunc;
            _outFunc = outFunc;
            _createContext = createContext;
        }

        public IDisposable CreateStackData(
            ConnectionType connectionType,
            CancellationTokenSource cancellationTokenSource, 
            IUnityContainer unityContainer)
        {
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                case ConnectionType.Initiator:
                    return _createContext?.Invoke() ?? Disposable.Empty;
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }

        public IObservable<TOut> CreateInbound(
            ConnectionType connectionType,
            InOutboundParams<TIn> data)
        {
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                case ConnectionType.Initiator:
                    return data.NextObservable.Select(_inFunc);
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }

        public IObservable<TIn> CreateOutbound(
            ConnectionType connectionType,
            InOutboundParams<TOut> data)
        {
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                case ConnectionType.Initiator:
                    return data.NextObservable.Select(block => _outFunc(block));
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }
    }
}