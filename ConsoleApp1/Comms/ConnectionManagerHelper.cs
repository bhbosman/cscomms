using System;
using System.Threading;

namespace Comms
{
    public static class ConnectionManagerHelper
    {
        private static void WriteData<T>(
            this IObserver<T> observable, 
            IConnectionCancelContext connectionCancelContext, 
            T block)
        {
            if (connectionCancelContext.IsCancellationRequested)
            {
                return;
            }

            if (block == null)
            {
                connectionCancelContext.Cancel();
                return;
            }

            observable.OnNext(block);
        }
        public static Action<T> WriteData<T>(
            this IObserver<T> observable, 
            IConnectionCancelContext connectionCancelContext)
        {
            return block =>
            {
                observable.WriteData(connectionCancelContext, block);
                
            };
        }
        
        
    }
}