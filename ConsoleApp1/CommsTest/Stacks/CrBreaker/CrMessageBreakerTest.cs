using System;
using System.Reactive.Subjects;
using System.Threading;
using Comms;
using Comms.Stack.CarriageReturnMessageBreaker;
using NUnit.Framework;
using Unity;

namespace CommsTest.Stacks.CrBreaker
{
    public class CrMessageBreakerTest
    {
        private static (Subject<MessageBlock.MessageBlock>, CancellationTokenSource) CreateSut(Action<MessageBlock.MessageBlock>act)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var unityContainer = new UnityContainer();
            unityContainer.RegisterType<IDisposableList, DisposableList>(TypeLifetime.PerContainer);


            var input = new Subject<MessageBlock.MessageBlock>();

            cancellationTokenSource.Token.Register(() => { unityContainer?.Dispose(); });
            unityContainer.RegisterInstance("input", input);

            var breaker = new CrMessageBreaker();
            var sut = breaker.CreateInbound(
                ConnectionType.Acceptor,
                new InOutboundParams<MessageBlock.MessageBlock>(
                    null,
                    cancellationTokenSource,
                    input,
                    unityContainer));
            unityContainer.AddToDisposableList(
                sut.Subscribe(
                    act,
                    exception => { },
                    () => {}));
            return (input, cancellationTokenSource);
        }
        
        
        [Test]
        public void TT0001()
        {
            int linesReceived = 0;
            var dd = CreateSut(block =>
            {
                ++linesReceived;
            });
            dd.Item1.OnNext(new MessageBlock.MessageBlock("Sentence\r\n"));
            dd.Item2.Cancel();
            Assert.AreEqual(1, linesReceived);
        }
        
        
        [Test]
        public void TT0002()
        {
            int linesReceived = 0;
            var dd = CreateSut(block =>
            {
                ++linesReceived;
            });
            dd.Item1.OnNext(new MessageBlock.MessageBlock("Sentence"));
            dd.Item2.Cancel();
            Assert.AreEqual(0, linesReceived);
        }
        
        
        
        
        [TestCase("\n", 1)]
        [TestCase("\n\n", 2)]
        // [TestCase("\n\n\n", 3)]
        // [TestCase("\n\n\n\n", 4)]
        // [TestCase("\n\n\n\n\n", 5)]
        public void TT0002ddd(string data, int count)
        {
            int linesReceived = 0;
            var dd = CreateSut(block =>
            {
                ++linesReceived;
            });
            dd.Item1.OnNext(new MessageBlock.MessageBlock(data)); ;
            Thread.Sleep(500);
            dd.Item2.Cancel();
            Assert.AreEqual(count, linesReceived);
        }
    }
}