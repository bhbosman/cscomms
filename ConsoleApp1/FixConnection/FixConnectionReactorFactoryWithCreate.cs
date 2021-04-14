using System;
using System.Threading;
using QuickFix.FIX44;
using Unity;

namespace FixConnection
{
    public class FixConnectionReactorFactory44 : FixConnectionReactorFactory<Message>
    {
        public FixConnectionReactorFactory44(params IFixConnectionReactorFactoryParamValue<Message>[] values) : base(values)
        {
        }
    }
    public sealed class FixConnectionReactorFactory44WithCreate: FixConnectionReactorFactory44
    {
        private readonly Func<IUnityContainer, CancellationTokenSource, IFixConnectionReactorFactory<Message> , IFixConnectionReactor<Message>> _create;

        public FixConnectionReactorFactory44WithCreate(
            Func<IUnityContainer, CancellationTokenSource, IFixConnectionReactorFactory<Message>, IFixConnectionReactor<Message>> create, 
            params IFixConnectionReactorFactoryParamValue<Message>[] values) : base(values)
        {
            _create = create;
        }


        public override IFixConnectionReactor<Message> CreateFixConnection(IUnityContainer container, CancellationTokenSource cancellationTokenSource)
        {
            return _create(container, cancellationTokenSource, this);
        }
    }
}