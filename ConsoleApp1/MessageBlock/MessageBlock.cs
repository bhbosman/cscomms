using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBlock
{
    public class MessageBlock : Stream
    {
        private readonly object _lockObject = new object();
        private const int MaxBlockSize = 64 * 1024;
        private readonly int _blockSize;
        private DataBlock _next;
        private DataBlock _last;
        private bool _canRead = true;
        private bool _canWrite = true;
        private Action<int> _writeNotification;

        public MessageBlock(int blockSize)
        {
            _blockSize = (blockSize > MaxBlockSize)
                ? MaxBlockSize
                : blockSize;
            _last = null;
            _next = null;
        }

        private MessageBlock() : this(1024)
        {
        }

        public MessageBlock(string data) : base()
        {
            WriteString(data);
        }

        public MessageBlock(char data) : this(Convert.ToString(data))
        {
        }


        class AsyncResult : IAsyncResult, IDisposable
        {
            private readonly AsyncCallback _callback;

            public AsyncResult(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                Buffer = buffer;
                Offset = offset;
                Count = count;
                _callback = callback;
                AsyncState = state;
                AsyncWaitHandle = new ManualResetEvent(false);
                CompletedSynchronously = true;
            }

            public bool IsCompleted { get; private set; }

            public WaitHandle AsyncWaitHandle { get; }
            public object AsyncState { get; }

            public bool CompletedSynchronously { get; }

            public byte[] Buffer { get; }

            public int Offset { get; }

            public int Count { get; }

            public void Dispose()
            {
                AsyncWaitHandle?.Dispose();
            }

            public void OnWrite(int e)
            {
                Run();
            }

            private void Run()
            {
                Task.Run(() =>
                {
                    ((ManualResetEvent) AsyncWaitHandle)?.Set();
                    IsCompleted = true;
                    _callback(this);
                });
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback,
            object state)
        {
            if (AvailableRead > 0)
            {
                return base.BeginRead(buffer, offset, count, callback, state);
            }

            var result = new AsyncResult(buffer, offset, count, callback, state);
            _writeNotification += result.OnWrite;
            return result;
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            if (!(asyncResult is AsyncResult ar)) return base.EndRead(asyncResult);
            using (ar)
            {
                return Read(ar.Buffer, ar.Offset, ar.Count);
            }
        }

        public override string ToString()
        {
            var byteArray = new byte[Length];
            WalkBuffer((i, b) =>
            {
                byteArray[i] = b;
                return true;
            });
            return Encoding.UTF8.GetString(byteArray);
        }

        public string ToBitString()
        {
            var byteArray = new byte[Length];
            WalkBuffer((i, b) =>
            {
                byteArray[i] = b;
                return true;
            });
            return BitConverter.ToString(byteArray, 0, byteArray.Length);
        }

        public MessageBlock(ArraySegment<byte> data) : base()
        {
            var db = new DataBlock(data);
            AddBlock(db);
        }


        public MessageBlock(byte[] data, int offset, int count, int read, int write) : base()
        {
            var db = new DataBlock(data, offset, count, read, write);
            AddBlock(db);
        }


        public static MessageBlock CreateMessageBlock(params byte[][] data)
        {
            var messageBlock = new MessageBlock();
            CreateMessageBlock(messageBlock, data);
            return messageBlock;
        }

        public static void CreateMessageBlock(MessageBlock messageBlock, params byte[][] data)
        {
            foreach (var byteArray in data)
            {
                messageBlock.Write(byteArray);
            }
        }

        public static MessageBlock CreateMessageBlock(params ArraySegment<byte>[] data)
        {
            var messageBlock = new MessageBlock();
            CreateMessageBlock(messageBlock, data);
            return messageBlock;
        }

        public static void CreateMessageBlock(MessageBlock messageBlock, params ArraySegment<byte>[] data)
        {
            foreach (var segment in data)
            {
                messageBlock.Write(segment);
            }
        }


        public int AddBlock(byte[] data, int offset, int count, int read, int write)
        {
            return AddBlock(new DataBlock(data, offset, count, read, write));
        }

        public int AddBlock(ArraySegment<byte> data, int read, int write)
        {
            return AddBlock(new DataBlock(data, read, write));
        }

        public int AddBlock(DataBlock db)
        {
            
            Monitor.Enter(_lockObject);
            try
            {
                if (_last != null)
                {
                    _last.Next = db;
                }

                _last = db;
                if (_next == null)
                {
                    _next = db;
                }
            }
            finally
            {
                Monitor.Exit(_lockObject);
            }

            int c = (int) AvailableRead;
            if (c > 0)
            {
                _writeNotification?.Invoke(c);
                _writeNotification = null;
            }

            return db.AvailableToRead;
        }

        public override void Flush()
        {
            Monitor.Enter(_lockObject);
            try
            {
                _canRead = false;
                _canWrite = false;
                _last?.Dispose();
                _last = null;
                _next?.Dispose();
                _next = null;
            }
            finally
            {
                Monitor.Exit(_lockObject);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotSupportedException("The stream does not support seeking");
        }

        public override void SetLength(long value)
        {
            throw new System.NotSupportedException("The stream does not support SetLength");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Monitor.Enter(_lockObject);
            try
            {
                return InternalRead(buffer, offset, count);
            }
            finally
            {
                Monitor.Exit(_lockObject);
            }
        }

        private int InternalRead(byte[] buffer, int offset, int count)
        {
            void MoveNext(DataBlock nextBlock)
            {
                _next = nextBlock.Next;
                nextBlock.Next = null;
                nextBlock.Dispose();
                if (_next == null)
                {
                    _last = null;
                }
            }

            var next = _next;
            if (_next == null) return 0;
            var n = _next.Read(buffer, offset, count);
            if (n == 0)
            {
                MoveNext(next);
                return InternalRead(buffer, offset, count);
            }

            if (n < count)
            {
                var nn = InternalRead(buffer, offset + n, count - n);
                return n + nn;
            }

            if (_next.AvailableToRead == 0)
            {
                MoveNext(_next);
            }
        
            return n;
        }

        private int InternalWriteToLast(byte[] buffer, int offset, int count)
        {
            return _last?.Write(buffer, offset, count) ?? 0;
        }

        public void Write(ArraySegment<byte> segment)
        {
            Write(segment.Array, segment.Offset, segment.Count);
        }

        public void Write(byte[] data)
        {
            Write(data, 0, data.Length);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            int cc = count;
            bool gotDataToWrite = count > 0;
            Monitor.Enter(_lockObject);
            try
            {
                while (count > 0)
                {
                    var n = InternalWriteToLast(buffer, offset, count);
                    count -= n;
                    offset += n;
                    if (count > 0)
                    {
                        AddBlock(new DataBlock(_blockSize));
                    }
                }
            }
            finally
            {
                Monitor.Exit(_lockObject);
            }

            if (gotDataToWrite)
            {
                _writeNotification?.Invoke((int) AvailableRead);
                _writeNotification = null;
            }
        }

        public override bool CanRead => _canRead;
        public override bool CanSeek => false;
        public override bool CanWrite => _canWrite;

        public override long Length => AvailableRead;

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public long AvailableWrite => _last?.AvailableToWrite ?? 0;

        public long AvailableRead
        {
            get
            {
                long availableRead = 0;
                for (var node = _next; node != null; node = node.Next)
                {
                    availableRead += node.AvailableToRead;
                }

                return availableRead;
            }
        }

        public int BlockCount
        {
            get
            {
                int count = 0;
                for (var node = _next; node != null; node = node.Next)
                {
                    count++;
                }

                return count;
            }
        }


        public long Wasted
        {
            get
            {
                long wasted = 0;
                for (var node = _next; node != null; node = node.Next)
                {
                    wasted += node.AvailableToWrite;
                }

                return wasted;
            }
        }

        public IList<ArraySegment<byte>> GetBuffers()
        {
            var list = new List<ArraySegment<byte>>();

            for (var node = _next; node != null; node = node.Next)
            {
                list.Add(node.GetBuffer());
            }

            return list;
        }

        private IList<DataBlock> GetDataBlocks()
        {
            var list = new List<DataBlock>();

            for (var node = _next; node != null; node = node.Next)
            {
                list.Add(node);
            }

            return list;
        }

        public int WalkBuffer(Func<int, byte, bool> action)
        {
            var count = -1;
            for (var node = _next; node != null; node = node.Next)
            {
                if (!node.WalkBuffer(() => ++count, action))
                {
                    return count;
                }
            }

            return -1;
        }

        protected override void Dispose(bool disposing)
        {
            Flush();
            base.Dispose(disposing);
        }

        public int TransferBlockTo(MessageBlock messageBlock)
        {
            var result = 0;
            Monitor.Enter(_lockObject);
            try
            {
                foreach (var db in GetDataBlocks())
                {
                    if (messageBlock.AvailableWrite >= db.AvailableToRead)
                    {
                        result += this.CopyPartial(messageBlock, db.AvailableToRead);
                    }
                    else
                    {
                        result += messageBlock.AddBlock(db);
                    }
                }

                _next = _last = null;
            }
            finally
            {
                Monitor.Exit(_lockObject);
            }

            return result;
        }

        public string ReadString()
        {
            var byteArray = new byte[Length];
            Read(byteArray, 0, byteArray.Length);
            return Encoding.UTF8.GetString(byteArray);
        }

        public void WriteString(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            var db = new DataBlock(bytes, 0, bytes.Length, 0, bytes.Length);
            AddBlock(db);
        }

        public uint PeekTypeCode()
        {
            if (AvailableRead >= 4)
            {
                Monitor.Enter(_lockObject);
                try
                {
                    var byteArray = new byte[4];
                    var n = InternalRead(byteArray, 0, byteArray.Length);
                    if (n != 4)
                    {
                        throw new InsufficientDataException(4);
                    }
                    AddBlockToFront(new DataBlock(byteArray));
                    return BitConverter.ToUInt32(byteArray, 0);
                }
                finally
                {
                    Monitor.Exit(_lockObject);
                }
            }
            throw new InsufficientDataException(4);
        }

        private void AddBlockToFront(DataBlock dataBlock)
        {
            if (_last == null)
            {
                _last = dataBlock;
            }
            dataBlock.Next = _next;
            _next = dataBlock;
        }

        public void WriteTypeCode(uint dataTypeCode)
        {
            Write(BitConverter.GetBytes(dataTypeCode));
        }

        public void WriteMessageLength(uint calculateSize)
        {
            Write(BitConverter.GetBytes(calculateSize));
        }

        public uint ReadMessageLength()
        {
            var byteArray = new byte[4];
            var n = Read(byteArray, 0, 4);
            if (n != 4)
            {
                throw new InsufficientDataException(4);
            }

            return BitConverter.ToUInt32(byteArray, 0);
        }

        public uint ReadTypeCode()
        {
            var byteArray = new byte[4];
            var n = Read(byteArray, 0, 4);
            if (n != 4)
            {
                throw new InsufficientDataException(4);
            }

            return BitConverter.ToUInt32(byteArray, 0);}
    }

    public class InsufficientDataException : Exception
    {
        
        public InsufficientDataException(int size): base($"Insufficient data. Requires 4 bytes but not enough available")
        {
        }
    }
}