using System;
using MessageBlock;
using NUnit.Framework;

namespace MessageBlockTest
{
    public class DataBlockTest
    {
        [Test]
        public void SimpleReadTest01Bytes()
        {
            var byteArray = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            var segment01 = new ArraySegment<byte>(byteArray, 0, 10);
            var db = new DataBlock(segment01);
            
            var targetArray = new byte[1024];
            var n = db.Read(targetArray, 0, 1);
            Assert.AreEqual(1, n);
            Assert.AreEqual(byteArray[0], targetArray[0]);
            Assert.AreEqual( 0, db.Offset);
            Assert.AreEqual( 10, db.Count);
            Assert.AreEqual( 1, db.ReadPosition);
            Assert.AreEqual( 10, db.WritePosition);
            
        }
        [Test]
        public void SimpleReadTest01BytesOffset5()
        {
            var byteArray = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            var segment01 = new ArraySegment<byte>(byteArray, 5, 10);
            var db = new DataBlock(segment01);
            
            var targetArray = new byte[1024];
            var n = db.Read(targetArray, 0, 1);
            Assert.AreEqual(1, n);
            Assert.AreEqual(byteArray[5], targetArray[0]);
            Assert.AreEqual( 5, db.Offset);
            Assert.AreEqual( 10, db.Count);
            Assert.AreEqual( 9, db.AvailableToRead);
            Assert.AreEqual( 0, db.AvailableToWrite);
            Assert.AreEqual( 1, db.ReadPosition);
            Assert.AreEqual( 10, db.WritePosition);
            
        }
        
        [Test]
        public void SimpleReadTest10Bytes()
        {
            var byteArray = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            var segment01 = new ArraySegment<byte>(byteArray, 0, 10);
            var db = new DataBlock(segment01);
            
            var targetArray = new byte[1024];
            var n = db.Read(targetArray, 0, 10);
            Assert.AreEqual(10, n);
            Assert.AreEqual( 
                new ArraySegment<byte>(byteArray, 0, 10), 
                new ArraySegment<byte>(targetArray, 0, 10));
            Assert.AreEqual( 0, db.Offset);
            Assert.AreEqual( 10, db.Count);
            Assert.AreEqual( 10, db.ReadPosition);
            Assert.AreEqual( 10, db.WritePosition);
            Assert.AreEqual( 0, db.AvailableToRead);
            Assert.AreEqual( 0, db.AvailableToWrite);

        }
        
        
        [Test]
        public void SimpleReadTestOverRead()
        {
            var byteArray = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            var segment01 = new ArraySegment<byte>(byteArray, 0, 10);
            var db = new DataBlock(segment01);
            
            var targetArray = new byte[1024];
            var n = db.Read(targetArray, 0, 20);
            Assert.AreEqual(10, n);
            Assert.AreEqual( 
                new ArraySegment<byte>(byteArray, 0, 10), 
                new ArraySegment<byte>(targetArray, 0, 10));
            Assert.AreEqual( 0, db.Offset);
            Assert.AreEqual( 10, db.Count);
            Assert.AreEqual( 10, db.ReadPosition);
            Assert.AreEqual( 10, db.WritePosition);
            Assert.AreEqual( 0, db.AvailableToRead);
            Assert.AreEqual( 0, db.AvailableToWrite);
        }
        
        [Test]
        public void SimpleWriteTest01Bytes()
        {
            var db = new DataBlock(10);
            var byteArray = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};

            var n  = db.Write(byteArray, 0, 1);
            Assert.AreEqual(1, n);

            var targetSegment = db.GetBuffer();
            
            Assert.AreEqual( 
                new ArraySegment<byte>(byteArray, 0, 1), 
                targetSegment);
            Assert.AreEqual( 0, db.Offset);
            Assert.AreEqual( 10, db.Count);
            Assert.AreEqual( 0, db.ReadPosition);
            Assert.AreEqual( 1, db.WritePosition);
            Assert.AreEqual( 1, db.AvailableToRead);
            Assert.AreEqual( 9, db.AvailableToWrite);
        }
        
        [Test]
        public void SimpleWriteTest10Bytes()
        {
            var db = new DataBlock(20);
            
            var byteArray = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};

            var n  = db.Write(byteArray, 0, 10);
            Assert.AreEqual(10, n);
            Assert.AreEqual( 
                new ArraySegment<byte>(byteArray, 0, 10), 
                 db.GetBuffer());
            Assert.AreEqual( 0, db.Offset);
            Assert.AreEqual( 20, db.Count);
            Assert.AreEqual( 0, db.ReadPosition);
            Assert.AreEqual( 10, db.WritePosition);
            Assert.AreEqual( 10, db.AvailableToRead);
            Assert.AreEqual( 10, db.AvailableToWrite);
            
        }
        
        
                
        [Test]
        public void SimpleWriteTest10BytesTwice()
        {
            var db = new DataBlock(20);
            
            var byteArray = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};

            var n  = db.Write(byteArray, 0, 10);
            Assert.AreEqual(10, n);
            n  = db.Write(byteArray, 10, 10);
            Assert.AreEqual(10, n);

            Assert.AreEqual( 
                new ArraySegment<byte>(byteArray, 0, 20), 
                db.GetBuffer());
            Assert.AreEqual( 0, db.Offset);
            Assert.AreEqual( 20, db.Count);
            Assert.AreEqual( 0, db.ReadPosition);
            Assert.AreEqual( 20, db.WritePosition);
            Assert.AreEqual( 20, db.AvailableToRead);
            Assert.AreEqual( 0, db.AvailableToWrite);
        }

        [Test]
        public void ArrayWithReadWrite()
        {
            var db = new DataBlock(new byte[]{1,2,3,4,5,6,7,8,9,0}, 0, 10, 4, 6);
            Assert.AreEqual( 0, db.Offset);
            Assert.AreEqual( 10, db.Count);
            Assert.AreEqual( 4, db.ReadPosition);
            Assert.AreEqual( 6, db.WritePosition);
            Assert.AreEqual( 2, db.AvailableToRead);
            Assert.AreEqual(4, db.AvailableToWrite);
            var b = new byte[20];
            var n = db.Read(b, 0, 20);
            Assert.AreEqual(2, n);
            Assert.AreEqual(5, b[0]);
            Assert.AreEqual(6, b[1]);
        }
    }
}