using Comms.Stack;
using Comms.Stack.BottomStack;
using Comms.Stack.CarriageReturnMessageBreaker;

namespace Comms
{
    public static class TextCarriageReturnStackBuilder{
        static TextCarriageReturnStackBuilder()
        {
            Stack = CreateTextCarriageReturnStackBuilder();
        }
        public static readonly IStackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock> Stack;
        private static IStackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock> CreateTextCarriageReturnStackBuilder()
        {
            var result = new StackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock>(
                BottomStackMessageBlock.Create().CreateFactory()
                    .Next(new CrMessageBreaker()));
            return result;
        }
    }
}