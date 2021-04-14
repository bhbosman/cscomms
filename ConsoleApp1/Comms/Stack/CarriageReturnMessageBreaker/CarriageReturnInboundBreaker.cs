using System;
using System.Runtime.InteropServices;
using MessageBlock;

namespace Comms.Stack.CarriageReturnMessageBreaker
{
    public class CarriageReturnInboundBreaker : IDisposable
    {
        private readonly Action<MessageBlock.MessageBlock> _next;
        private readonly Action<Exception> _onError;
        private readonly Action _onCompleted;
        private MessageBlock.MessageBlock mb;

        public CarriageReturnInboundBreaker(Action<MessageBlock.MessageBlock> onNext, Action<Exception> onError, Action onCompleted)
        {
            _next = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
            mb = CreateMessageBlock();
        }

        private static MessageBlock.MessageBlock CreateMessageBlock()
        {
            return new MessageBlock.MessageBlock(1024);
        }

        public void OnNext(MessageBlock.MessageBlock data)
        {
            using (data)
            {
               while (data.AvailableRead > 0)
               {
                   var n = data.WalkBuffer((i, b) =>
                   {
                       switch (b)
                       {
                           case (byte) '\n':
                               return false;
                           default:
                               return true;
                       }
                   });
                   if (n == -1)
                   {
                       data.TransferBlockTo(mb);
                       break;
                   }

                   if (n > 0)
                   {
                       data.CopyPartial(mb, n);
                   }

                   DoNext(mb);
                   data.ReadByte(); // must be \n
                   mb = CreateMessageBlock();
               }
            }
        }

        private void DoNext(MessageBlock.MessageBlock messageBlock)
        {
            _next?.Invoke(messageBlock);
        }

        public void Dispose()
        {
            mb?.Dispose();
        }
    }
}