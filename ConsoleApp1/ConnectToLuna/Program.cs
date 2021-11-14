using System;
using System.Threading;
using Comms;
using Comms.StackBuilders;
using Unity;
using Unity.Lifetime;

namespace ConnectToLuna
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var appContext = new ConnectionCancelContextOwner("ApplicationContext", new CancellationTokenSource());               
            try
            {
                IUnityContainer container = new UnityContainer();
                
                container.RegisterInstance<IConnectionCancelContext>("Application", appContext, InstanceLifetime.External);
                appContext.Register(() => container.Dispose());
                
                var dialer = new DefaultDialer(
                    "Main dialer",
                    container.CreateChildContainer(),
                    BottomTopStackBuilder.Stack,
                    new ConnectionReactorFactory()
                );
                dialer.Start();

                Console.ReadLine();
                
                dialer.Stop();
            }
            finally
            {
                appContext.Cancel();
            }
            Console.Write("ddddd");
        }
    }
}