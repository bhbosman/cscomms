namespace Comms
{


    public interface IConnectionManagerParamValue<TOutFromStack>
    {
        void Resolve(ConnectionManager<TOutFromStack> connectionManager);
    }
}