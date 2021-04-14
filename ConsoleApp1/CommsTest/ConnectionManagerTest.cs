using System;
using System.Threading;
using Comms;
using NUnit.Framework;

namespace CommsTest
{
    public class ConnectionManagerTest
    {
        [Test]
        public void ddd()
        {
            var token = new CancellationTokenSource();
            using (var d = new ShortConnection(block => { }))
            {
                IObservable<MessageBlock.MessageBlock> dd = d.ReadDataObservable(token);
                using (dd.Subscribe(block =>
                {
                    string ss = block.ToString();
                    Console.WriteLine(ss);
                },
                    exception => { },
                    () => {}))
                {
                    d.SendData("ddddasdasdasdasdasdasdasdasd");
                    Thread.Sleep(100);        
                }
            }
        }
    }
}