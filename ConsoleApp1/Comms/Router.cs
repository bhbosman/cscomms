using System;
using System.Collections.Generic;
using System.Reflection;

namespace Comms
{
    public class Router : IDisposable
    {
        private abstract class SendMessageHandler
        {
            public abstract void Send(object message);
        }

        private class SendMessageHandler<T> : SendMessageHandler
        {
            private readonly Action<T> _act;

            public SendMessageHandler(Action<T> act)
            {
                _act = act;
            }

            public override void Send(object message)
            {
                T msgT = (T) message;
                if (msgT != null)
                {
                    _act?.Invoke(msgT);
                }
            }
        }
        private readonly Dictionary<Type, SendMessageHandler> _dictionary = new Dictionary<Type, SendMessageHandler>();
        public void Register<T>(Action<T> act)
        {
            _dictionary.Add(typeof(T), new SendMessageHandler<T>(act));
        }

        public void Send(object message)
        {
            if (_dictionary.TryGetValue(message.GetType(), out var value))
            {
                value.Send(message);
            }
        }

        public void Dispose()
        {
            _dictionary.Clear();
        }
    }
}