using System;

namespace FixConnection.Stack.Fix44.Fix44MessageFactory
{
    public class MessageFactoryError : Exception
    {
        public MessageFactoryError(string s): base(s)
        {
        }
    }
}