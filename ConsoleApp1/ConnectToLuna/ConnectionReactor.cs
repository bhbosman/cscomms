using System;
using Comms;
using Comms.Interfaces;
using Unity;

namespace ConnectToLuna
{
    public class ConnectionReactor : IConnectionReactor<MessageBlock.MessageBlock>
    {
        public Action<MessageBlock.MessageBlock> Init(
            IObserver<MessageBlock.MessageBlock> connectionWriter, 
            IObserver<MessageBlock.MessageBlock> toReactor,
            IConnectionCancelContext connectionCancelContext,
            IUnityContainer container)
        {
            return Action;
        }

        private void Action(MessageBlock.MessageBlock block)
        {
            
            Console.WriteLine(block.AvailableRead);
        }

        public void Dispose()
        {
        }
    }
}