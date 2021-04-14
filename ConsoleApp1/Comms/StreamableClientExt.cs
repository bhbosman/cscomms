using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Comms
{
    public static class StreamableClientExt
    {
        public static void WriteData(
            this IStreamableClient streamableClient,
            CancellationTokenSource cancellationTokenSource,
            MessageBlock.MessageBlock block)
        {
            try
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

                var list = block.GetBuffers();
                streamableClient.Send(list);
            }
            finally
            {
                block?.Dispose();
            }
        }

        public static Action<MessageBlock.MessageBlock> WriteData(
            this IStreamableClient streamableClient,
            CancellationTokenSource cancellationTokenSource)
        {
            return block =>
            {
                WriteData(streamableClient, cancellationTokenSource, block);
            };
        }

        private static Task<int> ReadAsync(this IStreamableClient client, byte[] buffer, int offset, int count)
        {
            var tcs = new TaskCompletionSource<int>();
            client.BeginReceive(buffer, offset, count,
                iar =>
                {
                    try
                    {
                        tcs.TrySetResult(client.EndReceive(iar));
                    }
                    catch (OperationCanceledException)
                    {
                        tcs.TrySetCanceled();
                    }
                    catch (Exception exc)
                    {
                        tcs.TrySetException(exc);
                    }
                }, null);
            return tcs.Task;
        }

        public static IObservable<MessageBlock.MessageBlock> ReadDataObservable(
            this IStreamableClient client,
            CancellationTokenSource cancellationTokenSource)
        {
            return Observable.Create<MessageBlock.MessageBlock>(
                observer =>
                {
                    Task.Run(
                        async () =>
                        {
                            try
                            {
                                (byte[], int, int) CreateNewArray()
                                {
                                    var byteArray = new byte[4096];
                                    return (byteArray, 0, byteArray.Length);
                                }

                                var buffer = CreateNewArray();
                                while (!cancellationTokenSource.IsCancellationRequested)
                                {
                                    if (buffer.Item3 < 256)
                                        buffer = CreateNewArray();
                                    {
                                    }
                                    var n = await client.ReadAsync(
                                        buffer.Item1,
                                        buffer.Item2,
                                        buffer.Item3);
                                    if (n == 0)
                                    {
                                        cancellationTokenSource.Cancel();
                                        break;
                                    }

                                    if (cancellationTokenSource.IsCancellationRequested)
                                    {
                                        break;
                                    }

                                    var segment = new ArraySegment<byte>(buffer.Item1, buffer.Item2, n);
                                    observer.OnNext(new MessageBlock.MessageBlock(segment));
                                    buffer.Item2 += n;
                                    buffer.Item3 -= n;
                                    if (buffer.Item3 < 256)
                                    {
                                        buffer = CreateNewArray();
                                    }
                                }
                            }
                            finally
                            {
                                observer.OnCompleted();
                            }
                        },
                        cancellationTokenSource.Token);
                    return Disposable.Create(cancellationTokenSource.Cancel);
                }).ObserveOn(ThreadPoolScheduler.Instance);
        }
    }
}