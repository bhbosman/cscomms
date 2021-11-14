using Comms.Interfaces;
using Comms.StackFactory;

namespace Comms
{
    public static class StackComponentExtension
    {
        public static IStackFactory<TIn, TOut> CreateFactory<TIn, TOut>(this IStackComponent<TIn, TOut> instance)
        {
            return new StackFactory<TIn, TOut>(instance);
        }
    }
}