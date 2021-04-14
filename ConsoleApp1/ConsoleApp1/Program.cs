using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Comms;
using Unity;

namespace ConsoleApp1
{
    class Program
    {
        class sss : ISubject<int, string>
        {
            public void OnCompleted()
            {
                throw new NotImplementedException();
            }

            public void OnError(Exception error)
            {
                throw new NotImplementedException();
            }

            public void OnNext(int value)
            {
                throw new NotImplementedException();
            }

            public IDisposable Subscribe(IObserver<string> observer)
            {
                throw new NotImplementedException();
            }
        }



        static void Main(string[] args)
        {
            var unityContainer = new UnityContainer();
            unityContainer.RegisterType<IDisposableList, DisposableList>(TypeLifetime.PerContainer);

            var listener = new Listener<MessageBlock.MessageBlock>(
                unityContainer, 
                new EchoServerConnectionReactorFactory(),
                new SetIpAddress<MessageBlock.MessageBlock>(IPAddress.Parse("0.0.0.0"), 12345, TextCarriageReturnStackBuilder.Stack),
                new SetIpAddress<MessageBlock.MessageBlock>(IPAddress.Parse("0.0.0.0"), 12346, TextCarriageReturnStackBuilder.Stack),
                new SetIpAddress<MessageBlock.MessageBlock>(IPAddress.Parse("0.0.0.0"), 12347, TextCarriageReturnStackBuilder.Stack));
            listener.Start();
            Console.ReadLine();
        }
    }
}
