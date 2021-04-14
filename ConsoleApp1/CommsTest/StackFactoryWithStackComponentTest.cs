using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using Comms.Stack;
using Comms.Stack.AnyStack;
using NUnit.Framework;
using Unity;

namespace Comms
{
    public class StackFactoryWithStackComponentTest
    {
        
        [Test]
        public void TestMultipleStacksInbound()
        {
            var unity = new UnityContainer();
            unity.RegisterType<IDisposableList, DisposableList>(TypeLifetime.PerContainer);
            var token = new CancellationTokenSource();

            var fac = new AnyStackComponent<int, int>(
                i => i*2, 
                i => throw  new NotImplementedException(), 
                () => Disposable.Empty).CreateFactory()
                .Next(
                    new AnyStackComponent<int, int>(
                        i => i*2, 
                        i => throw  new NotImplementedException(), 
                        () => Disposable.Empty))
                .Next(
                    new AnyStackComponent<int, int>(
                        i => i*2, 
                        i => throw  new NotImplementedException(), 
                        () => Disposable.Empty)); 
            using (var sub = new Subject<int>())
            {
                var l = new List<IDisposable>();
                fac.CreateContext(ConnectionType.Acceptor, l, token, unity);
                var inbound = fac.CreateInbound(
                    new StackFactoryInOutboundParams<int>(
                        ConnectionType.Acceptor,
                        l.ToArray(),
                        token,
                        sub,
                        unity));
                using (inbound.Subscribe(
                    s =>
                    {
                        Assert.AreEqual(8, s);
                    },
                    exception => { },
                    () => {}))
                {
                    sub.OnNext(1);
                }
            }
        }
        [Test]
        public void TestMultipleStacksOutbound()
        {
            var unity = new UnityContainer();
            unity.RegisterType<IDisposableList, DisposableList>(TypeLifetime.PerContainer);

            var token = new CancellationTokenSource();

            var fac = new AnyStackComponent<int, int>(
                    i => throw new NotImplementedException(), 
                    i => i/2, 
                    () => Disposable.Empty).CreateFactory()
                .Next(
                    new AnyStackComponent<int, int>(
                        i => throw new NotImplementedException(), 
                        i => i/2, 
                        () => Disposable.Empty))
                .Next(
                    new AnyStackComponent<int, int>(
                        i => throw new NotImplementedException(), 
                        i => i/2, 
                        () => Disposable.Empty)); 
            using (var sub = new Subject<int>())
            {
                var l = new List<IDisposable>();
                fac.CreateContext(ConnectionType.Acceptor, l, token, unity);
                var inbound = fac.CreateOutbound(
                    new StackFactoryInOutboundParams<int>(
                        ConnectionType.Acceptor,
                        l.ToArray(),
                        token,
                        sub,
                        unity));
                using (inbound.Subscribe(
                    s =>
                    {
                        Assert.AreEqual(1, s);
                    },
                    exception => { },
                    () => {}))
                {
                    sub.OnNext(8);
                }
            }
        }
    }
}