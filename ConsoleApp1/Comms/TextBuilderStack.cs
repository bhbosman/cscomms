using Comms.Stack;
using Comms.Stack.BottomStack;
// using ConsoleApp1.Comms;

namespace Comms
{
    public static class TextBuilderStack
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly IStackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock> TextStackBuilder;
        static TextBuilderStack()
        {
            TextStackBuilder = CreateTextBuilder();    
        }
        private static IStackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock> CreateTextBuilder()
        {
            var result = new StackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock>(
                BottomStackMessageBlock.Create().CreateFactory());
            return result;
        }
    }
}