namespace Comms.Interfaces
{
    public interface IConnectionManagerParamValue<TOutFromStack>
    {
        void Resolve(ConnectionManager<TOutFromStack> connectionManager);
    }
}