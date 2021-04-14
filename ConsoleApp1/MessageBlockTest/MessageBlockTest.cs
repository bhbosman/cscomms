using System;
using System.Collections.Generic;
using MessageBlock;
using NUnit.Framework;

namespace MessageBlockTest
{
    public class MessageBlockTest
    {
        [Test]
        public void SimpleRead()
        {
            var byteArray = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0};
            var mb = new MessageBlock.MessageBlock(byteArray, 0, 10, 0, 10);
            var targetArray = new byte[30];
            mb.Read(targetArray, 0, 1);
            Assert.AreEqual(
                new ArraySegment<byte>(byteArray, 0, 1),
                new ArraySegment<byte>(targetArray, 0, 1));
        }

        [Test]
        public void SimpleReadOver()
        {
            var byteArray = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0};
            var mb = new MessageBlock.MessageBlock(byteArray, 0, 10, 0, 10);
            var targetArray = new byte[30];
            var n = mb.Read(targetArray, 0, 30);
            Assert.AreEqual(10, n);
            Assert.AreEqual(
                new ArraySegment<byte>(byteArray, 0, 10),
                new ArraySegment<byte>(targetArray, 0, 10));
        }

        [Test]
        public void SimpleReadTwoBuffers()
        {
            var byteArray = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0};
            var mb = MessageBlock.MessageBlock.CreateMessageBlock(byteArray, byteArray);
            var targetArray = new byte[30];
            var n = mb.Read(targetArray, 0, 30);
            Assert.AreEqual(20, n);
            Assert.AreEqual(
                new ArraySegment<byte>(byteArray, 0, 10),
                new ArraySegment<byte>(targetArray, 0, 10));
            Assert.AreEqual(
                new ArraySegment<byte>(byteArray, 0, 10),
                new ArraySegment<byte>(targetArray, 10, 10));
        }

        [Test]
        public void SimpleWrite()
        {
            var mb = new MessageBlock.MessageBlock(8);
            var byteArray = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0};
            mb.Write(byteArray, 0, 10);
            Assert.AreEqual(6, mb.AvailableWrite);
            Assert.AreEqual(10, mb.Length);
            Assert.AreEqual(6, mb.Wasted);
        }

        [Test]
        public void GetBufferFromMessageBlockCreatedWithArraySegments()
        {
            var byteArray = new byte[] {1, 2, 3, 4, 5, 6, 7, 8};
            var mb = new MessageBlock.MessageBlock(12);
            MessageBlock.MessageBlock.CreateMessageBlock(
                mb,
                new ArraySegment<byte>(byteArray, 0, 4),
                new ArraySegment<byte>(byteArray, 0, 4),
                new ArraySegment<byte>(byteArray, 0, 4));

            Assert.AreEqual(0, mb.AvailableWrite);
            Assert.AreEqual(12, mb.Length);
            Assert.AreEqual(0, mb.Wasted);
            Assert.AreEqual(
                new List<ArraySegment<byte>>
                {
                    new ArraySegment<byte>(new byte[] {1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4})
                },
                mb.GetBuffers());
        }

        [Test]
        public void GetBufferFromMessageBlockCreatedWithArrayByteArray()
        {
            var byteArray = new byte[] {1, 2, 3, 4};
            var mb = new MessageBlock.MessageBlock(12);
            MessageBlock.MessageBlock.CreateMessageBlock(
                mb,
                byteArray, byteArray, byteArray);

            Assert.AreEqual(0, mb.AvailableWrite);
            Assert.AreEqual(12, mb.Length);
            Assert.AreEqual(0, mb.Wasted);

            Assert.AreEqual(
                new List<ArraySegment<byte>>
                {
                    new ArraySegment<byte>(new byte[] {1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4})
                },
                mb.GetBuffers());
        }

        [Test]
        public void AddBlocks()
        {
            var byteArray = new byte[] {1, 2, 3, 4, 5, 6, 7, 8};
            var mb = new MessageBlock.MessageBlock(12);
            mb.AddBlock(new DataBlock(byteArray, 0, 8, 0, 4, null));
            mb.AddBlock(new DataBlock(byteArray, 0, 8, 0, 8, null));
            Assert.AreEqual(12, mb.Length);
            Assert.AreEqual(4, mb.Wasted);
            Assert.AreEqual(0, mb.AvailableWrite);
            Assert.AreEqual(
                new List<ArraySegment<byte>>
                {
                    new ArraySegment<byte>(new byte[]{1,2,3,4}),
                    new ArraySegment<byte>(new byte[]{1,2,3,4,5,6,7,8})
                }, 
                mb.GetBuffers());
        }

        [Test]
        public void CopyTest01()
        {
            var master = new MessageBlock.MessageBlock(16);
            Assert.AreEqual(0, master.AvailableWrite);
            Assert.AreEqual(0, master.AvailableRead);
            Assert.AreEqual(0, master.Length);
            
            var incomingData01 = new MessageBlock.MessageBlock(new byte[]{1,2,3,4,5,6}, 0, 6, 0, 6);
            Assert.AreEqual(0, incomingData01.AvailableWrite);
            Assert.AreEqual(6, incomingData01.AvailableRead);
            Assert.AreEqual(6, incomingData01.Length);
            
            incomingData01.CopyTo(master);
            Assert.AreEqual(10, master.AvailableWrite);
            Assert.AreEqual(6, master.AvailableRead);
            Assert.AreEqual(6, master.Length);
            Assert.AreEqual(0, incomingData01.Length);

            incomingData01.CopyTo(master);
            Assert.AreEqual(10, master.AvailableWrite);
            Assert.AreEqual(6, master.AvailableRead);
            Assert.AreEqual(6, master.Length);
            Assert.AreEqual(0, incomingData01.Length);
        }
        
        [Test]
        public void CopyTest02()
        {
            var master = new MessageBlock.MessageBlock(16);
            Assert.AreEqual(0, master.AvailableWrite);
            Assert.AreEqual(0, master.AvailableRead);
            Assert.AreEqual(0, master.Length);

            var incomingData01 = new MessageBlock.MessageBlock(12);
            incomingData01.AddBlock(new byte[]{1,2,3,4,5,6}, 0, 6, 0, 6);
            incomingData01.AddBlock(new byte[]{1,2,3,4,5,6}, 0, 6, 0, 6);
            Assert.AreEqual(0, incomingData01.AvailableWrite);
            Assert.AreEqual(12, incomingData01.AvailableRead);
            Assert.AreEqual(12, incomingData01.Length);
            Assert.AreEqual(2, incomingData01.BlockCount);
            
            incomingData01.CopyTo(master);
            Assert.AreEqual(4, master.AvailableWrite);
            Assert.AreEqual(12, master.AvailableRead);
            Assert.AreEqual(1, master.BlockCount);
            Assert.AreEqual(12, master.Length);
            Assert.AreEqual(0, incomingData01.Length);
            Assert.AreEqual(0, incomingData01.BlockCount);

            incomingData01.CopyTo(master);
            Assert.AreEqual(4, master.AvailableWrite);
            Assert.AreEqual(12, master.AvailableRead);
            Assert.AreEqual(12, master.Length);
            Assert.AreEqual(0, incomingData01.Length);
        }
        
        [Test]
        public void CopyTestPartial()
        {
            var master = new MessageBlock.MessageBlock(16);
            Assert.AreEqual(0, master.AvailableWrite);
            Assert.AreEqual(0, master.AvailableRead);
            Assert.AreEqual(0, master.Length);

            var incomingData01 = new MessageBlock.MessageBlock(12);
            incomingData01.AddBlock(new byte[]{1,2,3,4,5,6}, 0, 6, 0, 6);
            incomingData01.AddBlock(new byte[]{1,2,3,4,5,6}, 0, 6, 0, 6);
            Assert.AreEqual(0, incomingData01.AvailableWrite);
            Assert.AreEqual(12, incomingData01.AvailableRead);
            Assert.AreEqual(12, incomingData01.Length);
            Assert.AreEqual(2, incomingData01.BlockCount);
            
            incomingData01.CopyPartial(master, 6);
            Assert.AreEqual(10, master.AvailableWrite);
            Assert.AreEqual(6, master.AvailableRead);
            Assert.AreEqual(6, master.Length);
            Assert.AreEqual(0, incomingData01.AvailableWrite);
            Assert.AreEqual(6, incomingData01.AvailableRead);
            Assert.AreEqual(6, incomingData01.Length);
            Assert.AreEqual(1, incomingData01.BlockCount);
        }
        [Test]
        public void Transfer()
        {
            var master = new MessageBlock.MessageBlock(16);
            Assert.AreEqual(0, master.AvailableWrite);
            Assert.AreEqual(0, master.AvailableRead);
            Assert.AreEqual(0, master.Length);
            Assert.AreEqual(0, master.BlockCount);

            var incomingData01 = new MessageBlock.MessageBlock(12);
            incomingData01.AddBlock(new byte[]{1,2,3,4,5,6}, 0, 6, 0, 6);
            incomingData01.AddBlock(new byte[]{1,2,3,4,5,6}, 0, 6, 0, 6);
            Assert.AreEqual(0, incomingData01.AvailableWrite);
            Assert.AreEqual(12, incomingData01.AvailableRead);
            Assert.AreEqual(12, incomingData01.Length);
            Assert.AreEqual(2, incomingData01.BlockCount);
            
            incomingData01.TransferBlockTo(master);
            Assert.AreEqual(0, master.AvailableWrite);
            Assert.AreEqual(12, master.AvailableRead);
            Assert.AreEqual(12, master.Length);
            Assert.AreEqual(2, master.BlockCount);
            Assert.AreEqual(0, incomingData01.AvailableWrite);
            Assert.AreEqual(0, incomingData01.AvailableRead);
            Assert.AreEqual(0, incomingData01.Length);
            Assert.AreEqual(0, incomingData01.BlockCount);
        }



        [Test]
        public void Walker()
        {
            var incomingData01 = new MessageBlock.MessageBlock(12);
            incomingData01.AddBlock(new byte[]{1,2,3,4,5,6}, 0, 6, 0, 6);
            incomingData01.AddBlock(new byte[]{1,2,3,4,5,6}, 0, 6, 0, 6);
            var idx = incomingData01.WalkBuffer((i, b) => true);
            Assert.AreEqual(-1, idx);
        }
        
        [Test]
        public void Walker02()
        {
            var incomingData01 = new MessageBlock.MessageBlock(12);
            incomingData01.AddBlock(new byte[]{1,2,3,4,5,6}, 0, 6, 0, 6);
            incomingData01.AddBlock(new byte[]{1,2,3,4,5,6}, 0, 6, 0, 6);
            var idx = incomingData01.WalkBuffer((i, b) =>
            {
                switch (b)
                {
                    case 1:
                        return false;
                    default:
                        return true;
                }
            });
            Assert.AreEqual(0, idx);
        }

        [Test]
        public void Walker03()
        {
            var incomingData01 = new MessageBlock.MessageBlock(12);
            incomingData01.AddBlock(new byte[]{1,2,3,4,5,(byte)'\n'}, 0, 6, 0, 6);
            var idx = incomingData01.WalkBuffer((i, b) =>
            {
                switch (b)
                {
                    case (byte)'\n':
                        return false;
                    default:
                        return true;
                }
            });
            Assert.AreEqual(5, idx);
        }
        
        [Test]
        public void Walker04()
        {
            var incomingData01 = new MessageBlock.MessageBlock(12);
            incomingData01.AddBlock(new byte[]{1,2,3,4,5,6}, 0, 6, 0, 6);
            incomingData01.AddBlock(new byte[]{1,2,3,4,5,(byte)'\n'}, 0, 6, 0, 6);
            var idx = incomingData01.WalkBuffer((i, b) =>
            {
                switch (b)
                {
                    case (byte)'\n':
                        return false;
                    default:
                        return true;
                }
            });
            Assert.AreEqual(11, idx);
        }
        
        
        
        [Test]
        public void Walker05()
        {
            
            var incomingData01 = new MessageBlock.MessageBlock(12);
            incomingData01.AddBlock(new byte[]{1,2,3,4,5,6}, 0, 6, 0, 6);
            incomingData01.AddBlock(new byte[]{1,2,3,4,5,(byte)'\n'}, 0, 6, 0, 6);
            var idx = incomingData01.WalkBuffer((i, b) =>
            {
                switch (b)
                {
                    case (byte)'\n':
                        return false;
                    default:
                        return true;
                }
            });
            Assert.AreEqual(11, idx);
            var master = new MessageBlock.MessageBlock(16);
            incomingData01.CopyPartial(master, idx);
            Assert.AreEqual(5, master.AvailableWrite);
            Assert.AreEqual(11, master.AvailableRead);
            Assert.AreEqual(11, master.Length);
            Assert.AreEqual(1, master.BlockCount);
            Assert.AreEqual(0, incomingData01.AvailableWrite);
            Assert.AreEqual(1, incomingData01.AvailableRead);
            Assert.AreEqual(1, incomingData01.Length);
            Assert.AreEqual(1, incomingData01.BlockCount);
            incomingData01.ReadByte();
            Assert.AreEqual(0, incomingData01.AvailableRead);
            Assert.AreEqual(0, incomingData01.Length);
            Assert.AreEqual(0, incomingData01.BlockCount);

        }


        [TestCase("HelloWorld", 10)]
        [TestCase("HelloWorld\n", 11)]
        public void TestToString(string input, int byteLength)
        {
            var mb = new MessageBlock.MessageBlock(input);
            Assert.AreEqual(byteLength, mb.Length);
            var s = mb.ToString();
            Assert.AreEqual(input, s);
            
        }

        
        
    }
}