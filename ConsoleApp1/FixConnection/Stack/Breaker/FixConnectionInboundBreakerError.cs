using System;

namespace FixConnection.Stack.Breaker
{
    public class FixConnectionInboundBreakerError : Exception
    {
        public FixConnectionInboundBreakerError(string error) : base(error)
        {
        }
    }
}