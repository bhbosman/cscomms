using System.Net;
using System.Threading;
using FixConnection;
using QuickFix.FIX44;
using Unity;

namespace FixService
{
    
    public class FixSessionStateSimulator: FixSessionState
    {
        public FixSessionStateSimulator(string initiatorCompId, string acceptorCompId, IPAddress address, int port, FixVersion version) 
            : base(initiatorCompId, acceptorCompId, address, port, version)
        {
            Router.Register<NewOrderSingle>(HandleNewOrderSingle);

        }
        
        private void HandleNewOrderSingle(QuickFix.FIX44.NewOrderSingle obj)
        {
            SendOutgoingMessage(new ExecutionReport());
            SendOutgoingMessage(new ExecutionReport());
            SendOutgoingMessage(new ExecutionReport());
            SendOutgoingMessage(new ExecutionReport());    
            SendOutgoingMessage(new ExecutionReport());    
            SendOutgoingMessage(new ExecutionReport());    
            SendOutgoingMessage(new ExecutionReport());    
            SendOutgoingMessage(new ExecutionReport());    
            SendOutgoingMessage(new ExecutionReport());    
            SendOutgoingMessage(new ExecutionReport());    
            SendOutgoingMessage(new ExecutionReport());    
            SendOutgoingMessage(new ExecutionReport());    
            SendOutgoingMessage(new ExecutionReport());    
        
        }
    }

    

    
}