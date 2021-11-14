using System;
using System.Text;
using FixConnection.Messages;
using MessageBlock;

namespace FixConnection.Stack.Breaker
{
    internal enum State
    {
        ReadTag,
        ReadValue
    }

    public interface dd
    {
        void OnNext(MessageBlock.MessageBlock block);
    }
    public class FixConnectionInboundBreaker : IDisposable, dd
    {
        private readonly Action<ParsedFixMessage> _next;
        private Action<MessageBlock.MessageBlock> _currentReader;

        public FixConnectionInboundBreaker(Action<ParsedFixMessage> onNext)
        {
            _next = onNext;
            _currentReader = ReadBeginString();
        }
        
        public void OnNext(MessageBlock.MessageBlock data)
        {
            while (data.AvailableRead > 0)
            {
                _currentReader(data);
            }
        }


        private static MessageBlock.MessageBlock CreateMessageBlock()
        {
            return new MessageBlock.MessageBlock(1024);
        }


        private static int ReadTagValue(MessageBlock.MessageBlock data, byte find)
        {
            return data.WalkBuffer((i, b) => b != find);
        }
        private static readonly byte[] BeginStringTag = {(byte) '8', (byte) '='};
        private static readonly byte[] LengthTag = {(byte) '9', (byte) '='};
        private static readonly byte[] CheckSumTag = {(byte) '1',(byte) '0', (byte) '='};
        private static readonly byte[] MsgTypeTag = {(byte) '3',(byte) '5', (byte) '='};
        private static readonly byte[] Soh = {1};
        private Action<MessageBlock.MessageBlock> ReadBeginString()
        {
            var state = State.ReadTag;
            var b = new byte[16];
            var completeFixMessage = CreateMessageBlock();
            var mb = CreateMessageBlock();
            return (incomingData) =>
            {
                if (state == State.ReadTag)
                {
                    var idx = incomingData.WalkBuffer((i, b1) =>
                    {
                        switch (b1)
                        {
                            case (byte) '=':
                                return false;
                            case (byte) '8':
                                return true;
                            default:
                                throw new Exception("Wrong char for begin string");
                        }
                    });
                    if (idx == -1)
                    {
                        incomingData.TransferBlockTo(mb);
                        if (mb.AvailableRead > 1)
                        {
                            throw new Exception("= expected");
                        }
                        return;
                    }

                    if (idx > 0)
                    {
                        incomingData.CopyPartial(mb, idx);
                    }
                    var n = mb.Read(b, 0, b.Length);
                    if (n != 1)
                    {
                        throw new Exception("BeginString tag length incorrect");
                    }

                    if (b[0] != (byte) '8')
                    {
                        throw new Exception("BeginString tag not equal to 8");
                    }

                    incomingData.ReadByte();
                    state = State.ReadValue;
                }

                // ReSharper disable once InvertIf
                if (state == State.ReadValue)
                {
                    var idx = ReadTagValue(incomingData, 1);
                    if (idx == -1)
                    {
                        incomingData.TransferBlockTo(mb);
                        return;
                    }

                    if (idx > 0)
                    {
                        incomingData.CopyPartial(mb, idx);
                    }

                    incomingData.ReadByte();
                    var n = mb.Read(b, 0, b.Length);
                    completeFixMessage.Write(BeginStringTag);
                    completeFixMessage.Write(b, 0, n);
                    completeFixMessage.Write(Soh);
                    _currentReader = ReadMessageSize(completeFixMessage, Encoding.UTF8.GetString(b, 0, n));
                }
            };
        }

        private Action<MessageBlock.MessageBlock> ReadMessageSize(MessageBlock.MessageBlock completeFixMessage, string beginstring)
        {
            var state = State.ReadTag;
            var b = new byte[16];
            var mb = CreateMessageBlock();
            return (incomingData) =>
            {
                if (state == State.ReadTag)
                {
                    var idx = ReadTagValue(incomingData, (byte)'=');
                    if (idx == -1)
                    {
                        incomingData.TransferBlockTo(mb);
                        return;
                    }

                    if (idx > 0)
                    {
                        incomingData.CopyPartial(mb, idx);
                    }

                    var n = mb.Read(b, 0, b.Length);

                    if (n != 1)
                    {
                        throw new Exception("Length tag length incorrect");
                    }

                    if (b[0] != (byte) '9')
                    {
                        throw new Exception("Length tag not equal to 9");
                    }

                    incomingData.ReadByte();
                    state = State.ReadValue;
                }

                // ReSharper disable once InvertIf
                if (state == State.ReadValue)
                {
                    var idx = ReadTagValue(incomingData, 1);
                    if (idx == -1)
                    {
                        incomingData.TransferBlockTo(mb);
                        return;
                    }

                    if (idx > 0)
                    {
                        incomingData.CopyPartial(mb, idx);
                    }

                    incomingData.ReadByte();

                    var n = mb.Read(b, 0, b.Length);
                    if (n == 0)
                    {
                        throw new Exception("Length can not be zero buffers");
                    }

                    int v = b[0] - '0';
                    for (var i = 1; i < n; i++)
                    {
                        v *= 10;
                        v += (b[i] - '0');
                    }

                    completeFixMessage.Write(LengthTag);
                    completeFixMessage.Write(b, 0, n);
                    completeFixMessage.Write(Soh);
                    _currentReader = ReadMessageType(completeFixMessage, beginstring, v);
                    
                }
            };
        }

        private Action<MessageBlock.MessageBlock> ReadMessageType(MessageBlock.MessageBlock completeFixMessage, string beginString, int messageLength)
        {
            var mb = CreateMessageBlock();
            var state = State.ReadTag;
            var b = new byte[16];
            return incomingData =>
            {
                if (state == State.ReadTag)
                {
                    var idx = ReadTagValue(incomingData, (byte)'=');
                    if (idx == -1)
                    {
                        messageLength -= incomingData.TransferBlockTo(mb);
                        return;
                    }

                    if (idx > 0)
                    {
                        messageLength -= incomingData.CopyPartial(mb, idx);
                    }

                    var n = mb.Read(b, 0, b.Length);

                    if (n != 2)
                    {
                        throw new Exception("MessageType tag length incorrect");
                    }

                    if (b[0] != (byte) '3' || b[1] != (byte) '5')
                    {
                        throw new Exception("MsgType tag not equal to 35");
                    }

                    incomingData.ReadByte();
                    messageLength--;
                    state = State.ReadValue;
                }
                
                // ReSharper disable once InvertIf
                if (state == State.ReadValue)
                {
                    var idx = ReadTagValue(incomingData, 1);
                    if (idx == -1)
                    {
                        messageLength -= incomingData.TransferBlockTo(mb);
                        return;
                    }

                    if (idx > 0)
                    {
                        messageLength -= incomingData.CopyPartial(mb, idx);
                    }

                    incomingData.ReadByte();
                    messageLength--;

                    var n = mb.Read(b, 0, b.Length);
                    if (n == 0)
                    {
                        throw new Exception("MsgType can not be zero buffers");
                    }

                    completeFixMessage.Write(MsgTypeTag);
                    completeFixMessage.Write(b, 0, n);
                    completeFixMessage.Write(Soh);
                    _currentReader = ReadMessageData(completeFixMessage, beginString, messageLength, Encoding.UTF8.GetString(b, 0, n));
                }
            };
        }

        private Action<MessageBlock.MessageBlock> ReadMessageData(MessageBlock.MessageBlock completeFixMessage, string beginString, int requiredLength, string messageType)
        {
            var leftOver = requiredLength;
            var mb = CreateMessageBlock();
            return (incomingData) =>
            {
                var n = incomingData.CopyPartial(mb, leftOver);
                leftOver -= n;

                if (mb.AvailableRead >= requiredLength)
                {
                    mb.TransferBlockTo(completeFixMessage);
                    _currentReader = ReadHashTag(completeFixMessage, beginString, messageType);
                }
            };
        }

        private Action<MessageBlock.MessageBlock> ReadHashTag(MessageBlock.MessageBlock completeFixMessage, string beginString, string messageType)
        {
            var state = State.ReadTag;
            var b = new byte[16];
            var mb = CreateMessageBlock();
            return (incomingData) =>
            {
                if (state == State.ReadTag)
                {
                    var idx = ReadTagValue(incomingData, (byte)'=');
                    if (idx == -1)
                    {
                        incomingData.TransferBlockTo(mb);
                        return;
                    }

                    if (idx > 0)
                    {
                        incomingData.CopyPartial(mb, idx);
                    }

                    var n = mb.Read(b, 0, b.Length);

                    if (n != 2)
                    {
                        throw new Exception("Hash tag length incorrect");
                    }

                    if (!(b[0] == (byte) '1' && b[1] == (byte) '0'))
                    {
                        throw new Exception("Hash tag not equal to 10");
                    }

                    incomingData.ReadByte();
                    state = State.ReadValue;
                }

                // ReSharper disable once InvertIf
                if (state == State.ReadValue)
                {
                    var idx = ReadTagValue(incomingData, 1);
                    if (idx == -1)
                    {
                        incomingData.TransferBlockTo(mb);
                        return;
                    }

                    if (idx > 0)
                    {
                        incomingData.CopyPartial(mb, idx);
                    }

                    incomingData.ReadByte();

                    var n = mb.Read(b, 0, b.Length);
                    if (n == 0)
                    {
                        throw new Exception("CheckSum can not be zero buffers");
                    }

                    var chekSumValue = b[0] - '0';
                    for (var i = 1; i < n; i++)
                    {
                        chekSumValue *= 10;
                        chekSumValue += b[i] - '0';
                    }

                    var hash = 0;
                    completeFixMessage.WalkBuffer((i, b1) =>
                    {
                        hash += b1;
                        return true;
                    });
                    if (chekSumValue != hash % 256)
                    {
                        throw new Exception("invalid CheckSum");
                    }
                    completeFixMessage.Write(CheckSumTag);
                    completeFixMessage.Write(b, 0, n);
                    completeFixMessage.Write(Soh);

                    var msg = new ParsedFixMessage(beginString, messageType, chekSumValue, completeFixMessage);
                    DoNext(msg);
                    _currentReader = ReadBeginString();
                }
            };
        }

        

        public void OnNext(string data)
        {
            OnNext(new MessageBlock.MessageBlock(data));
        }
        public void OnNext(char data)
        {
            OnNext(new MessageBlock.MessageBlock(data));
        }

        private void DoNext(ParsedFixMessage messageBlock)
        {
            _next?.Invoke(messageBlock);
        }

        public void Dispose()
        {
        }
    }
}