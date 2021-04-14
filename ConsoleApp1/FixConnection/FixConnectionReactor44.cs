using System;
using System.Collections.Generic;
using System.Threading;
using Comms;
using QuickFix.Fields;
using QuickFix.FIX44;
using Unity;

namespace FixConnection
{
    public enum FixConnectionReactor44State
    {
        Error,
        Connected,
        RequireLogin,
        RequestMissingMessages,
        WaitForResponseState,
        Success,
        WaitForInitiatorToSync
    }

    public class FixConnectionReactor44Error : Exception
    {
        public FixConnectionReactor44State State { get; }

        public FixConnectionReactor44Error(FixConnectionReactor44State state, string message) :base(message)
        {
            State = state;
        }
    }
    public sealed class FixConnectionReactor44 : IFixConnectionReactor<Message>
    {
        private readonly IUnityContainer _container;
        private readonly CancellationTokenSource _source;
        private FixConnectionReactor44State _state;
        private readonly IFixConnectionReactorFactory<Message> _factory;
        

        private Action<Message> _currentStateInbound;
        private Action<Message> _currentStateOutbound;

        public FixConnectionReactor44(IUnityContainer container, CancellationTokenSource source, IFixConnectionReactorFactory<Message> factory)
        {
            _container = container;
            _source = source;
            _factory = factory;
        }

        private (Action<Message>, Action<Message> ) CreateConnectedState(
            IUnityContainer container,
            IObserver<Message> connectionWriter,
            CancellationTokenSource cancellationTokenSource)
        {
            var list = new List<Message>();
            _state = FixConnectionReactor44State.Connected;
            return 
                (
                    message =>
                    {
                        switch (message)
                        {
                            case Logon logon:
                                var senderCompId = logon.Header.GetString(49);
                                var targetCompId = logon.Header.GetString(56);
                                var valid = _factory.IsValidSession(senderCompId, targetCompId);
                                if (valid)
                                {
                                    FixSessionState fixSessionState = _factory.Get(senderCompId, targetCompId);
                                    var disp = fixSessionState.OnConnect(connectionWriter, cancellationTokenSource);
                                    _container.AddToDisposableList(fixSessionState.OnDisconnectAction("from container"));
                                    _container.AddToDisposableList(disp);

                                    (_currentStateInbound, _currentStateOutbound) = CreateRequireLoginState(
                                        fixSessionState,
                                        list);
                                    _currentStateInbound(logon);
                                }

                                break;
                            default:
                                (_currentStateInbound, _currentStateOutbound) = CreateErrorState(
                                    FixConnectionReactor44State.Connected,
                                    "Login message expected");
                                _currentStateInbound(message);
                                break;
                        };
                    },
                    message => list.Add(message)
                );
        }

        private (Action<Message>, Action<Message>) CreateRequireLoginState(
            FixSessionState fixSessionState,
            List<Message> list)
        {
            _state = FixConnectionReactor44State.RequireLogin;
            return
            (
                message =>
                {
                    switch (message)
                    {
                        case Logon logon:
                            (_currentStateInbound, _currentStateOutbound) = CreateSuccessState(fixSessionState, list);
                            _currentStateInbound(message);
                            break;
                        default:
                            (_currentStateInbound, _currentStateOutbound) = CreateErrorState(
                                FixConnectionReactor44State.RequireLogin,
                                $"{nameof(Logon)} required, but {message.GetType().Name} received");
                            _currentStateInbound(message);
                            break;
                    }
                },
                list.Add
            );
        }

        private (Action<Message>, Action<Message>)  CreateSuccessState(
            FixSessionState fixSessionState,
            List<Message> list)
        {
            
            _state = FixConnectionReactor44State.Success;

            (Action<Message>, Action<Message>) ans = 
                (
                    fixSessionState.SendIncomingMessage,
                    fixSessionState.SendOutgoingMessage
                );
            foreach (var message in list)
            {
                ans.Item2(message);
            }

            fixSessionState.StartAux();
            return ans;
        }

        private (Action<Message>, Action<Message>) CreateErrorState(FixConnectionReactor44State state, string errorMessage)
        {
            _state = FixConnectionReactor44State.Error;
            var exception = new FixConnectionReactor44Error(state, errorMessage);
            return 
                (
                    message => throw exception,
                    message => throw exception
                );
        }
        
        public Action<Message> Init(
            IObserver<Message> connectionWriter, 
            IObserver<Message> toReactor,
            CancellationTokenSource cancellationTokenSource,
            IUnityContainer container)
        {
            (_currentStateInbound, _currentStateOutbound) = CreateConnectedState(
                container,
                connectionWriter,
                cancellationTokenSource);

            bool f = true;
            return message =>
            {
                if (f)
                {
                    f = false;
                    int e = 3;
                }
                _currentStateInbound(message);
                f = false;
            };
        }

        public void Dispose()
        {
            (_currentStateInbound, _currentStateOutbound) = CreateErrorState(
                _state,
                $"Dispose");
        }
    }
}