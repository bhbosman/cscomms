using Comms;
using Comms.Stack.BottomStack;
using Comms.Stack.TopStack;
using FixConnection.Messages;
using FixConnection.Stack.Breaker;
using QuickFix.FIX44;
using MessageFactory = FixConnection.Stack.Fix44.Fix44MessageFactory.MessageFactory;


namespace FixConnection
{
    public static class FixDefinedStack
    {
        public static readonly IStackBuilder<MessageBlock.MessageBlock, Message> StackBuilder44;

        static FixDefinedStack()
        {
            StackBuilder44 = CreateFixStack44();
        }

        private static IStackBuilder<MessageBlock.MessageBlock, Message> CreateFixStack44()
        {
            IStackFactory<MessageBlock.MessageBlock, Message> factory =
                BottomStackMessageBlock.Create().CreateFactory()
                    .Next(new FixMessageBreaker())
                    .Next(new MessageFactory())
                    .Next(new TopStack<Message>())
                    ;
            var result = new StackBuilder<MessageBlock.MessageBlock, QuickFix.FIX44.Message>(factory);
            return result;
        }
        
        private static IStackBuilder<MessageBlock.MessageBlock, QuickFix.FIX43.Message> CreateFixStack43()
        {
            var factory =
                BottomStackMessageBlock.Create().CreateFactory()
                    .Next(new FixMessageBreaker())
                    .Next(new Stack.Fix43MessageFactory.MessageFactory());
            var result = new StackBuilder<MessageBlock.MessageBlock, QuickFix.FIX43.Message>(factory);
            return result;
        }
        private static IStackBuilder<MessageBlock.MessageBlock, ParsedFixMessage> CreateFixStack()
        {
            var factory =
                BottomStackMessageBlock.Create().CreateFactory()
                    .Next(new FixMessageBreaker());
            var result = new StackBuilder<MessageBlock.MessageBlock, ParsedFixMessage>(factory);
            return result;
        }
    }
}