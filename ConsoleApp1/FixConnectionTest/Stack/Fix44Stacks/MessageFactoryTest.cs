using System;
using System.Reactive.Subjects;
using System.Threading;
using Comms;
using FixConnection.FixDirectoryServices;
using FixConnection.Messages;
using FixConnection.Stack.Fix44.Fix44MessageFactory;
using NUnit.Framework;
using Unity;
using MessageBlock1 = MessageBlock.MessageBlock;

namespace FixConnectionTest.Stack.Fix44Stacks
{
    public class MessageFactoryTest
    {
        [TestCase("FIX.4.4", "D", 92, typeof(QuickFix.FIX44.NewOrderSingle),"8=FIX.4.49=14835=D34=108049=TESTBUY152=20180920-18:14:19.50856=TESTSELL111=63673064027889863415=USD21=238=700040=154=155=MSFT60=20180920-18:14:19.49210=092")]
        public void TestValid44(string beginString, string messageType, int chekSumValue, Type type, string completeFixMessage)
        {
            var stack = new MessageFactory();
            var cancellationTokenSource = new CancellationTokenSource();
            var nextObservable = new Subject<ParsedFixMessage>();
            IUnityContainer unityContainer = new UnityContainer();
            unityContainer.RegisterFixDictionaries();
            var stackContext = stack.CreateStackData(ConnectionType.Acceptor, cancellationTokenSource, unityContainer);
            var obs = stack.CreateInbound(
                ConnectionType.Acceptor, 
                new InOutboundParams<ParsedFixMessage>(
                    stackContext,
                    cancellationTokenSource,
                    nextObservable,
                    unityContainer));
            obs.Subscribe(message =>
            {
                Assert.IsInstanceOf(type, message);
            });

            var parsedFixMessage = new ParsedFixMessage(beginString, messageType, chekSumValue,
                new MessageBlock1(completeFixMessage));
            nextObservable.OnNext(parsedFixMessage);
        }
        
        [TestCase("FIX.4.3", "D", 92, "8=FIX.4.39=14835=D34=108049=TESTBUY152=20180920-18:14:19.50856=TESTSELL111=63673064027889863415=USD21=238=700040=154=155=MSFT60=20180920-18:14:19.49210=092")]
        public void TestInvalid44(string beginString, string messageType, int chekSumValue, string completeFixMessage)
        {
            Assert.Throws<MessageFactoryError>(() =>
            {
                var stack = new MessageFactory();
                var cancellationTokenSource = new CancellationTokenSource();
                var nextObservable = new Subject<ParsedFixMessage>();
                IUnityContainer unityContainer = new UnityContainer();
                unityContainer.RegisterType<IDisposableList, DisposableList>(TypeLifetime.PerContainer);
                unityContainer.RegisterFixDictionaries();
                var stackContext = stack.CreateStackData(ConnectionType.Acceptor, cancellationTokenSource, unityContainer);
                var obs = stack.CreateInbound(
                    ConnectionType.Acceptor, 
                    new InOutboundParams<ParsedFixMessage>(
                        stackContext,
                        cancellationTokenSource,
                        nextObservable,
                        unityContainer));
                obs.Subscribe(message =>
                {
                    // Assert.IsInstanceOf(type, message);
                });

                var parsedFixMessage = new ParsedFixMessage(beginString, messageType, chekSumValue,
                    new MessageBlock1(completeFixMessage));
                nextObservable.OnNext(parsedFixMessage);
            });
        }
    }
}