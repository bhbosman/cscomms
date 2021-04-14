using MessageBlock1 = MessageBlock.MessageBlock;

namespace FixConnection.Messages
{
    public class ParsedFixMessage
    {
        public ParsedFixMessage(string beginString, string messageType, int chekSumValue, MessageBlock1 completeFixMessage)
        {
            BeginString = beginString;
            MessageType = messageType;
            ChekSumValue = chekSumValue;
            CompleteFixMessage = completeFixMessage;
        }

        public ParsedFixMessage(MessageBlock1 mb)
        {
            CompleteFixMessage = mb;
        }

        public string BeginString { get; }

        public string MessageType { get; }

        public int ChekSumValue { get; }

        public MessageBlock1 CompleteFixMessage { get; }
    }
}