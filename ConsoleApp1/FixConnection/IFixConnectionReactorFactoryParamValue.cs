namespace FixConnection
{
    public interface IFixConnectionReactorFactoryParamValue<T>
    {
        void Resolve(FixConnectionReactorFactory<T> connectionReactorFactory);
    }
}