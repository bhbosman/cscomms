using System;

namespace FixConnection.Stack.Fix43MessageFactory
{
    public class MessageFactoryError : Exception
    {
        public MessageFactoryError(string s): base(s)
        {
        }
    }
}