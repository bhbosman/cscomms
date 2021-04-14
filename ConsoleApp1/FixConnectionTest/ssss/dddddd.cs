using System;
using System.Globalization;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FixConnectionTest.ssss
{
    public class dddddd
    {
        [Test]
        public void De()
        {
            var sub = new Subject<int>();
            sub
                .ObserveOn(ThreadPoolScheduler.Instance);
            var obs01 = sub
                .Select(
                    i =>
                    {
                        Console.WriteLine($"ThreadId: {Thread.CurrentThread.ManagedThreadId} (0) {i}");
                        return i;
                    })
                .ObserveOn(ThreadPoolScheduler.Instance);
            var obs02 = obs01.Select(i =>
                    {
                        Console.WriteLine($"Enter Step2 ThreadId: {Thread.CurrentThread.ManagedThreadId} (10) {i}");
                        Thread.Sleep(400);
                        Console.WriteLine($"Leave  Step2 ThreadId: {Thread.CurrentThread.ManagedThreadId} (10) {i}");
                        return i;
                    })
                    .ObserveOn(ThreadPoolScheduler.Instance)
                ;
            obs02.Subscribe(i =>
            {
                // Console.WriteLine($"Enter {Thread.CurrentThread.ManagedThreadId} (20) {i}");

                // Console.WriteLine($"Leave {Thread.CurrentThread.ManagedThreadId} (20) {i}");
            });


            sub.OnNext(1);
            sub.OnNext(2);
            sub.OnNext(3);
            sub.OnNext(4);
            sub.OnNext(5);
            sub.OnNext(6);
            sub.OnNext(7);
            sub.OnNext(8);
            sub.OnNext(9);


            // Task.Run(() => sub.OnNext(1));
            // Task.Run(() => sub.OnNext(2));
            // Task.Run(() => sub.OnNext(3));
            // Task.Run(() => sub.OnNext(4));
            // Task.Run(() => sub.OnNext(5));
            // Task.Run(() => sub.OnNext(6));
            // Task.Run(() => sub.OnNext(7));
            // Task.Run(() => sub.OnNext(8));
            // Task.Run(() => sub.OnNext(9));


            Thread.Sleep(10000);
        }

        [Test]
        public void ddd()
        {
            var sub = new Subject<int>();
            var initialPipe = sub
                .Select<int, int>(i => i)
                .Select<int, int>(i => throw new Exception())
                .Select<int, int>(i => i);
                
            IDisposable disposable = null;

            void OnError(Exception exception)
            {
                Console.WriteLine("exception");
                disposable?.Dispose();
                disposable = initialPipe
                    .Subscribe(
                        OnNext,
                        OnError,
                        OnCompleted);

            }

            void OnNext(int i)
            {
                Console.WriteLine("action");
            }

            void OnCompleted()
            {
                Console.WriteLine("complete");
            }

            disposable = initialPipe
                .Subscribe(
                    OnNext,
                    OnError,
                    OnCompleted);
            Console.WriteLine("begin");
            sub.OnNext(12);
            sub.OnNext(12);
            sub.OnNext(12);
            Console.WriteLine("begin");




        }

        [Test]
        public void Publish()
        {
            var esub = new Subject<int>();
            esub.Publish().Connect();
            var sub = new Subject<int>();
            IConnectableObservable<int> pipe01 = sub
                .Select(i => { Console.WriteLine($"first {i}"); return i; })
                .Select(i => { Console.WriteLine($"second {i}"); return i; })
                .Select<int, int>(i =>
                {
                    if (i %2 ==0)
                    {
                        return i;
                    }
                    throw new Exception("dddd");
                })
                .Select(i => { Console.WriteLine($"third {i}"); return i; })
                .Retry()
                .Publish();
            var pipe02 = pipe01.Connect();

            // sub.OnError(new Exception());

            var disposable = pipe01
            .Subscribe(
            i =>
            {
            Console.WriteLine($"sub {i}");
            });

            sub.OnNext(1);
            sub.OnNext(2);
            sub.OnNext(3);
            sub.OnNext(4);
        }
    }
}