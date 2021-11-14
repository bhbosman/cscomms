using Comms.Stack.BottomStack;
using Comms.Stack.TopStack;

namespace Comms.StackBuilders
{
    public static class BottomTopStackBuilder{
        static BottomTopStackBuilder()
        {
            Stack = CreateBottomTopStackBuilder();
        }
        public static readonly IStackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock> Stack;
        private static IStackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock> CreateBottomTopStackBuilder()
        {
            var result = new StackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock>(
                BottomStackMessageBlock.Create()
                    .CreateFactory()
                    .Next(new TopStack<MessageBlock.MessageBlock>()));
            return result;
        }
    }
}