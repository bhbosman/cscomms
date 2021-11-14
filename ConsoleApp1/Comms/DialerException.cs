using System;
using System.Collections.Generic;

namespace Comms
{
    public class DialerException : ConnectionManagerException 
    {
        public DialerException(string message,  IDictionary<string, object> additionalInformation, Exception innerException)
            : base(message, additionalInformation, innerException)
        {
        }
        public DialerException(string message, Exception innerException)
            : this(message, null,innerException)
        {
        }
    }
}