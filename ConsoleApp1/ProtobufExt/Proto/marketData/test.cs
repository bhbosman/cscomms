using System;
using System.IO;
using NUnit.Framework;

namespace ProtobufExt.Proto.marketData
{
    public class Test
    {
        
        [Test]
        public void Dddd()
        {
            var p = new Point
            {
                Price = 23
            };

            var messageBlock = new MessageBlock.MessageBlock(p.MessageBlockSize());
            p.WriteTo(messageBlock);
            Console.WriteLine(messageBlock.ToBitString());

            uint tc = messageBlock.PeekTypeCode();
            
            switch (tc)
            {
                case Point.TypeCodeValue:
                    p.MergeFrom(messageBlock);
                    break;
            }
        }
    }
}