using System;
using System.Threading;

namespace Comms
{
    public static class ConnectionManagerHelper
    {
        private static void WriteData<T>(this IObserver<T> observable, CancellationTokenSource cancellationTokenSource, T block)
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            if (block == null)
            {
                cancellationTokenSource.Cancel();
                return;
            }

            observable.OnNext(block);
        }
        public static Action<T> WriteData<T>(this IObserver<T> observable, CancellationTokenSource cancellationTokenSource)
        {
            return block =>
            {
                observable.WriteData(cancellationTokenSource, block);
                
            };
        }
        
        
    }
}