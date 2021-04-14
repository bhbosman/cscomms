using System;
using System.Threading;
using Comms;
using Unity;

namespace ConsoleApp1
{
    internal class EchoClientConnectionReactorFactory : IConnectionReactorFactory<MessageBlock.MessageBlock>
    {
        public string Name { get; }

        public IConnectionReactor<MessageBlock.MessageBlock> Create(IUnityContainer container,
            CancellationTokenSource cancellationTokenSource)
        {
            return new EchoClientConnectionReactor(cancellationTokenSource);
        }
    }

    internal class EchoClientConnectionReactor : IConnectionReactor<MessageBlock.MessageBlock>
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private IObserver<MessageBlock.MessageBlock> _connectionWriter;
        private IObserver<MessageBlock.MessageBlock> _toReactor;

        public EchoClientConnectionReactor(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        public Action<MessageBlock.MessageBlock> Init(
            IObserver<MessageBlock.MessageBlock> connectionWriter,
            IObserver<MessageBlock.MessageBlock> toReactor,
            CancellationTokenSource cancellationTokenSource,
            IUnityContainer container)
        {
            _connectionWriter = connectionWriter;
            _toReactor = toReactor;
            return Action;
        }

        private void Action(MessageBlock.MessageBlock data)
        {
            Console.Write(data.Length);
        }

        public void Dispose()
        {

        }
    }

}