using System;
using System.Net;
using Comms;
using FixConnection;
using Unity;

namespace FixService
{
    internal static class Program
    {
        
        static void Main(string[] args)
        {
            IUnityContainer maiUnityContainer = new UnityContainer();
            maiUnityContainer.RegisterType<IDisposableList, DisposableList>(TypeLifetime.PerContainer);
            var sessionInFormation01 = new AddSession<QuickFix.FIX44.Message>(
                new FixSessionStateSimulator(
                "CLIENT1",
                "EXECUTOR",
                IPAddress.Parse("0.0.0.0"), 
                12345, 
                FixVersion.Fix44));
            var sessionInFormation02 = new AddSession<QuickFix.FIX44.Message>(
                new FixSessionStateSimulator(
                    "CLIENT2",
                    "EXECUTOR",
                    IPAddress.Parse("0.0.0.0"), 
                    12345, 
                    FixVersion.Fix44));

            var fixConnectionReactorFactory =
                new FixConnectionReactorFactory44WithCreate(
                    (container, source, fac) => new FixConnectionReactor44(container, source, fac)
                    , sessionInFormation01
                    //, sessionInFormation02
                    );
            var fixListener = new FixListener<QuickFix.FIX44.Message>(
                maiUnityContainer,
                fixConnectionReactorFactory,
                new SetIpAddress<QuickFix.FIX44.Message>(IPAddress.Parse("0.0.0.0"), 12345, FixDefinedStack.StackBuilder44));
            fixListener.Start();

            Console.ReadLine();
        }
    }
}