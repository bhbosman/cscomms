using System;
using System.Collections.Generic;

namespace Comms
{
    public class ConnectionManagerException : Exception
    {
        public IDictionary<string, object> AdditionalInformation { get; }

        public ConnectionManagerException(
            string message, IDictionary<string, object> additionalInformation, Exception innerException): base(message, innerException)
        {
            AdditionalInformation = additionalInformation;
        }

        public ConnectionManagerException(string message, Exception innerException) :this(message, null, innerException)
        {
            
        }
    }
}