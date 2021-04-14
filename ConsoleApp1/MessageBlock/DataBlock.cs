using System;
using System.Runtime.InteropServices;

namespace MessageBlock
{
    public class DataBlock : IDisposable
    {
        private int _read;
        private int _write;
        private readonly int _offset;
        private readonly int _count;
        private readonly byte[] _data;
        public DataBlock Next { get; set; }


        private DataBlock(DataBlock next)
        {
            Next = next;
        }

        public DataBlock() : this(64 * 1024)
        {
        }

        public DataBlock(ArraySegment<byte> data, DataBlock next = null) : this(next)
        {
            _write = _count = data.Count;
            _offset = data.Offset;
            _data = data.Array;
            _read = 0;
        }


        public DataBlock(int n, DataBlock next = null) : this(new ArraySegment<byte>(new byte[n]), next)
        {
            _write = 0;
        }

        public DataBlock(byte[] buffer, DataBlock next = null) : this(new ArraySegment<byte>(buffer), next)
        {
        }

        public DataBlock(ArraySegment<byte> buffer, int read, int write, DataBlock next = null) : this(buffer, next)
        {
            _read = read;
            _write = write;
        }


        public DataBlock(byte[] buffer, int read, int write, DataBlock next = null) : this(
            new ArraySegment<byte>(buffer), read, write, next)
        {
        }

        public DataBlock(byte[] buffer, int offset, int count, int read, int write, DataBlock next = null) : this(
            new ArraySegment<byte>(buffer, offset, count), read, write, next)
        {
        }

        public int AvailableToRead => _write - _read;
        public int AvailableToWrite => _count - _write;
        public int Offset => _offset;

        public int Count => _count;

        public int ReadPosition => _read;
        public int WritePosition => _write;


        public int Read([In, Out] byte[] buffer, int offset, int count)
        {
            var toRead = AvailableToRead > count ? count : AvailableToRead;
            if (toRead > 0)
            {
                Array.Copy(_data, _offset + _read, buffer, offset, toRead);
                _read += toRead;
            }

            return toRead;
        }

        public int Write(byte[] buffer, int offset, int count)
        {
            var toWrite = AvailableToWrite > count ? count : AvailableToWrite;
            if (toWrite > 0)
            {
                Array.Copy(buffer, offset, _data, _offset + _write, toWrite);
                _write += toWrite;
            }

            return toWrite;
        }


        public ArraySegment<byte> GetBuffer()
        {
            return new ArraySegment<byte>(_data, _offset + _read, AvailableToRead);
        }

        public bool WalkBuffer(Func<int> index, Func<int, byte, bool> f)
        {
            int availableRead = AvailableToRead;
            for (int i = 0; i < availableRead; i++)
            {
                var idx = index();
                if (!f(idx, _data[_offset + _read + i]))
                {
                    return false;
                }
            }

            return true;
        }

        public void Dispose()
        {
            Next?.Dispose();
            Next = null;
        }
    }
}