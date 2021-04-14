using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Comms
{
    internal class StreamableTcpClientImpl : IStreamableClient
    {
        public StreamableTcpClientImpl(TcpClient tcpClient)
        {
            Socket = tcpClient.Client;
        }

        private Socket Socket { get; }

        public IAsyncResult BeginReceive(byte[] buffer, int offset, int size, AsyncCallback callback,
            object state)
        {
            return Socket.BeginReceive(buffer, offset, size, SocketFlags.None, callback, state);
        }

        public int EndReceive(IAsyncResult asyncResult)
        {
            return Socket.EndReceive(asyncResult);
        }

        public int Send(IList<ArraySegment<byte>> buffers)
        {
            return Socket.Send(buffers, SocketFlags.None);
        }

        public void Dispose()
        {
            Socket?.Dispose();
        }
    }
}