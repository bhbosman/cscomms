using System.Collections.Generic;
using FixConnection.Stack.Breaker;
using NUnit.Framework;

namespace FixConnectionTest.Stack.Breaker
{
    public class FixConnectionInboundBreakerTest01
    {
        [TestCase(1, "8=FIX.4.49=14835=D34=108049=TESTBUY152=20180920-18:14:19.50856=TESTSELL111=63673064027889863415=USD21=238=700040=154=155=MSFT60=20180920-18:14:19.49210=092")]
        public void CheckFullMessage(int count, string input)
        {
            var l = new List<string>();
            using (var sut = new FixConnectionInboundBreaker(
                block =>
                {    
                    l.Add(block.CompleteFixMessage.ToString());
                
                }))
            {
                sut.OnNext(input);
            }
            Assert.AreEqual(count, l.Count);
            Assert.AreEqual(input,  string.Concat(l.ToArray()));
        }
        
        
        [TestCase(1, "8=FIX.4.49=14835=D34=108049=TESTBUY152=20180920-18:14:19.50856=TESTSELL111=63673064027889863415=USD21=238=700040=154=155=MSFT60=20180920-18:14:19.49210=092")]
        public void CheckFullMessageWhenSendByteForByte(int count, string input)
        {
            var l = new List<string>();
            using (FixConnectionInboundBreaker sut = new FixConnectionInboundBreaker(
                block =>
                {    
                    l.Add(block.CompleteFixMessage.ToString());
                
                }))
            {
                foreach (var c in input)
                {
                    sut.OnNext(c);    
                }
            }
            Assert.AreEqual(count, l.Count);
            Assert.AreEqual(input,  string.Concat(l.ToArray()));
        }
    }
}