using System;
using System.Collections.Generic;

namespace Comms
{
    public interface IStreamableClient: IDisposable
    {
        IAsyncResult BeginReceive(
            byte[] buffer,
            int offset,
            int size,
            AsyncCallback callback,
            object state);
        int EndReceive(IAsyncResult asyncResult);
        int Send(IList<ArraySegment<byte>> buffers);
    }
}