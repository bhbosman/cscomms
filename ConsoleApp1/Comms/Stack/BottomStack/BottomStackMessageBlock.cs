namespace Comms.Stack.BottomStack
{
    public static class BottomStackMessageBlock 
    {
        public static BottomStack<MessageBlock.MessageBlock> Create()
        {
            return new BottomStack<MessageBlock.MessageBlock>(
                block => block,
                block => block);
        }
    }
}