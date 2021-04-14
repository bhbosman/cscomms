using System;

namespace Comms.Stack.CarriageReturnMessageBreaker
{
    public class CarriageReturnOutboundBreaker : IDisposable
    {
        private readonly Action<MessageBlock.MessageBlock> _next;
        private readonly Action<Exception> _onError;
        private readonly Action _onCompleted;

        public CarriageReturnOutboundBreaker(Action<MessageBlock.MessageBlock> onNext, Action<Exception> onError, Action onCompleted)
        {
            _next = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }

        private readonly byte[] crByteArray = new byte[]{(byte)'\n'};

        public void OnNext(MessageBlock.MessageBlock data)
        {
            data.Write(crByteArray);
            _next(data);
        }
        public void Dispose()
        {
            // do nothing;
        }
    }
}