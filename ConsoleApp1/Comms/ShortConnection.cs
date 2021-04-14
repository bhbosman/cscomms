using System;
using System.Collections.Generic;
using System.Net.Sockets;
using MessageBlock;


namespace Comms
{
    public class ShortConnection : IStreamableClient
    {
        private readonly Action<MessageBlock.MessageBlock> _onReceived;
        private readonly MessageBlock.MessageBlock _outgoing;
        private readonly MessageBlock.MessageBlock _incoming;

        public ShortConnection(Action<MessageBlock.MessageBlock> onReceived)
        {
            _onReceived = onReceived;
            _outgoing = new MessageBlock.MessageBlock(4096);
            _incoming = new MessageBlock.MessageBlock(4096);
        }

        public void SendData(string s)
        {
            _outgoing.WriteString(s);
        }


        IAsyncResult IStreamableClient.BeginReceive(byte[] buffer, int offset, int size, AsyncCallback callback,
            object state)
        {
            return _outgoing.BeginRead(buffer, offset, size, callback, state);
        }

        int IStreamableClient.EndReceive(IAsyncResult asyncResult)
        {
            return _outgoing.EndRead(asyncResult);
        }

        int IStreamableClient.Send(IList<ArraySegment<byte>> buffers)
        {
            int count = 0;
            foreach (ArraySegment<byte> arraySegment in buffers)
            {
                count += arraySegment.Count;
                _incoming.AddBlock(new DataBlock(
                    arraySegment,
                    0,
                    arraySegment.Count));
            }

            _onReceived?.Invoke(_incoming);
            return count;
        }

        public void Dispose()
        {
            _outgoing?.Dispose();
            _incoming?.Dispose();
        }
    }
}