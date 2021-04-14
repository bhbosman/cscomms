using System.Threading;
using Comms;
using FixConnection;
using FixConnection.FixDirectoryServices;
using FixConnection.Messages;
using FixConnection.Stack.Breaker;
using FixConnection.Stack.Fix44.Fix44MessageFactory;
using Moq;
using NUnit.Framework;
using Unity;

namespace FixConnectionTest
{
    public class RoundTripTest
    {
        [Test]
        public void TestRoundTipWithMessageFactory43()
        {
            IUnityContainer maiUnityContainer = new UnityContainer();
            maiUnityContainer.RegisterFixDictionaries();
            var fixConnectionReactorFactory = new Mock<IFixConnectionReactorFactory<QuickFix.FIX43.Message>>();

            fixConnectionReactorFactory
                .Setup(factory => factory.Create(
                    It.IsAny<IUnityContainer>(),
                    It.IsAny<CancellationTokenSource>()))
                .Returns(
                    (IUnityContainer a, CancellationTokenSource b) 
                        => new FixConnectionReactorWithAction<QuickFix.FIX43.Message>(a, b,
                            (sender, data) =>
                            {
                                sender.ConnectionWriter.OnNext(data);
                            }));
            var stackBuilder = new StackBuilder<MessageBlock.MessageBlock, QuickFix.FIX43.Message>(
                new FixMessageBreaker().CreateFactory()
                    .Next(new FixConnection.Stack.Fix43MessageFactory.MessageFactory()));
            var fixListener = new FixListener<QuickFix.FIX43.Message>(
                maiUnityContainer,
                fixConnectionReactorFactory.Object);
            var count = 0;
            using (var shortConnection = new ShortConnection(block =>
            {
                count++;
            }))
            {
                using (fixListener.InitiateNewClient(
                    stackBuilder,
                    (container, source) => {},
                    shortConnection,
                    100))
                {
                    var logon = new QuickFix.FIX43.Logon(new QuickFix.Fields.EncryptMethod(), new QuickFix.Fields.HeartBtInt(60));
                    shortConnection.SendData(logon.ToString());
                }
            }
            Assert.AreEqual(1, count);
        }
        
        
        [Test]
        public void TestRoundTipWithMessageFactory44()
        {
            IUnityContainer maiUnityContainer = new UnityContainer();
            maiUnityContainer.RegisterFixDictionaries();
            var fixConnectionReactorFactory = new Mock<IFixConnectionReactorFactory<QuickFix.FIX44.Message>>();

            fixConnectionReactorFactory
                .Setup(factory => factory.Create(
                    It.IsAny<IUnityContainer>(),
                    It.IsAny<CancellationTokenSource>()))
                .Returns(
                    (IUnityContainer a, CancellationTokenSource b) 
                        => new FixConnectionReactorWithAction<QuickFix.FIX44.Message>(a, b,
                            (sender, data) =>
                            {
                                sender.ConnectionWriter.OnNext(data);
                            }));
            var stackBuilder = new StackBuilder<MessageBlock.MessageBlock, QuickFix.FIX44.Message>(
                new FixMessageBreaker().CreateFactory()
                    .Next(new MessageFactory()));
            var fixListener = new FixListener<QuickFix.FIX44.Message>(
                maiUnityContainer,
                fixConnectionReactorFactory.Object);
            var count = 0;
            using (var shortConnection = new ShortConnection(block =>
            {
                count++;
            }))
            {
                using (fixListener.InitiateNewClient(
                    stackBuilder,
                    (container, source) => {},
                    shortConnection,
                    100))
                {
                    var logon = new QuickFix.FIX44.Logon(new QuickFix.Fields.EncryptMethod(), new QuickFix.Fields.HeartBtInt(60));
                    shortConnection.SendData(logon.ToString());
                }
            }
            Assert.AreEqual(1, count);
        }

        
        
    }
}