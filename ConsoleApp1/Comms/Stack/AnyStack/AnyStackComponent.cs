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
        private readonly Func<IConnectionCancelContext, object> _createContext;

        public AnyStackComponent(string name, Func<TIn, TOut> inFunc, Func<TOut, TIn> outFunc, Func<IConnectionCancelContext, object> createContext)
        {
            Name = name;
            _inFunc = inFunc;
            _outFunc = outFunc;
            _createContext = createContext;
        }

        public string Name { get; }

        public object CreateStackData(
            ConnectionType connectionType,
            IConnectionCancelContext connectionCancelContext, 
            IUnityContainer unityContainer)
        {
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                case ConnectionType.Initiator:
                    return _createContext?.Invoke(connectionCancelContext) ?? Disposable.Empty;
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