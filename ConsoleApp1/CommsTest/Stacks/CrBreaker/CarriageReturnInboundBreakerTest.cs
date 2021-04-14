using System.Collections.Generic;
using Comms.Stack.CarriageReturnMessageBreaker;
using NUnit.Framework;

namespace CommsTest.Stacks.CrBreaker
{
    public class CarriageReturnInboundBreakerTest
    {
        [TestCase("HelloWorld\n", 1, "HelloWorld")]
        [TestCase("msg01\nmsg01\nmsg01\nmsg01\nmsg01\nmsg01\n", 6, "msg01","msg01","msg01","msg01","msg01","msg01")]
        public void SimpleTest(string data, int count, params string[] ans)
        {
            var messagesReceived = 0;
            var list = new List<string>();
            var crBreaker = new CarriageReturnInboundBreaker(
                block =>
                {
                    messagesReceived++;
                    list.Add(block.ToString());
                }, 
                null, 
                null);
            crBreaker.OnNext(new MessageBlock.MessageBlock(data));
            Assert.AreEqual(count, messagesReceived);
            Assert.AreEqual(count, ans.Length);
            Assert.AreEqual(ans, list.ToArray());
        }
    }
}