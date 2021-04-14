using System;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Comms;
using Comms.Extensions;
using QuickFix.Fields;
using QuickFix.FIX44;
using Timer = System.Timers.Timer;

namespace FixConnection
{
    public class FixSessionState : IDisposable
    {
        // on killing a connection
        private readonly Subject<Tuple<string, Exception>> _killObs;
        private readonly IConnectableObservable<Tuple<string, Exception>> _endOfKillObs;

        // outbound observable
        private readonly Subject<Message> _outboundObservable;
        private readonly IConnectableObservable<Message> _endOfOutboundObservable;

        // inbound observable
        private readonly Subject<Message> _inboundObservable;
        protected readonly Router Router = new Router();

        private readonly IPAddress _address;
        private readonly int _port;
        private readonly FixVersion _version;

        private int _nextInboundSeqNumber;
        private int _nextOutboundSeqNumber;
        public string InitiatorCompId { get; }
        public string AcceptorCompId { get; }

        public FixSessionState(string initiatorCompId, string acceptorCompId, IPAddress address, int port,
            FixVersion version)
        {
            InitiatorCompId = initiatorCompId;
            AcceptorCompId = acceptorCompId;
            _address = address;
            _port = port;
            _version = version;
            _nextInboundSeqNumber = 1;
            _nextOutboundSeqNumber = 100;

            _killObs = new Subject<Tuple<string, Exception>>();
            _endOfKillObs = _killObs.Publish();
            _endOfKillObs.Connect();

            _inboundObservable = new Subject<Message>();
            var endOfInboundObservable = _inboundObservable
                .Select(((Func<Message, Message>) CheckInboundCompIds).SelectWrapWithException(
                    KillActiveConnectionFromException("CheckInboundCompIds")))
                .Select(((Func<Message, Message>) PersistIn).SelectWrapWithException(
                    KillActiveConnectionFromException("PersistIn")))
                .Retry()
                .Publish();
            endOfInboundObservable.Connect();
            endOfInboundObservable.Subscribe(
                ((Action<object>) Router.Send).WrapWithException(KillActiveConnectionFromException("Router.Send")),
                KillActiveConnectionFromException("Router.Send"),
                KillActiveConnection("Router.Send"));
            _outboundObservable = new Subject<Message>();
            _outboundObservable.ObserveOn(ThreadPoolScheduler.Instance);
            _endOfOutboundObservable = _outboundObservable
                .Select<Message, Message>(((Func<Message, Message>) AddCompIds).SelectWrapWithException(
                    KillActiveConnectionFromException("AddCompIds")))
                .Select(((Func<Message, Message>) AddSequenceNumber).SelectWrapWithException(
                    KillActiveConnectionFromException("AddSequenceNumber")))
                .Select(((Func<Message, Message>) AddSendTime).SelectWrapWithException(
                    KillActiveConnectionFromException("AddSendTime")))
                .Select(((Func<Message, Message>) PersistOut).SelectWrapWithException(
                    KillActiveConnectionFromException("PersistOut")))
                .Retry()
                .Publish();
            _endOfOutboundObservable.Connect();

            Router.Register<Logon>(HandleLogon);
            Router.Register<SequenceReset>(HandleSequenceReset);
            Router.Register<ResendRequest>(HandleResendRequest);
            Router.Register<Reject>(HandleReject);
        }

        public void Dispose()
        {
            _inboundObservable.OnCompleted();
            _inboundObservable.Dispose();
            _outboundObservable.OnCompleted();
            _outboundObservable.Dispose();
            _killObs.OnCompleted();
            _killObs.Dispose();
            Router.Dispose();
        }

        private void HandleReject(Reject reject)
        {
        }

        private Message CheckInboundCompIds(Message message)
        {
            var senderCompId = message.Header.GetString(49);
            var targetCompId = message.Header.GetString(56);
            if (senderCompId == InitiatorCompId && targetCompId == AcceptorCompId)
            {
                return message;
            }

            throw new Exception("Wrong compId");
        }

        private void HandleResendRequest(ResendRequest resendRequest)
        {
            var beginSeq = resendRequest.BeginSeqNo.getValue();
            var endSeq = resendRequest.EndSeqNo.getValue();
            ReplayOutBoundMessages(beginSeq, endSeq, (seq, message) =>
            {
                if (message == null)
                {
                    message = new SequenceReset()
                    {
                        GapFillFlag = new GapFillFlag(true),
                        NewSeqNo = new NewSeqNo(seq + 1),
                    };
                    message.Header.SetField(new MsgSeqNum(seq), true);
                    message.Header.SetField(new PossDupFlag(true), true);
                    message.Header.SetField(new OrigSendingTime(DateTime.Now.ToUniversalTime()));
                }

                message.Header.SetField(new PossDupFlag(true));
                _outboundObservable.OnNext(message);
            });
        }

        private void HandleSequenceReset(SequenceReset obj)
        {
            MarkAsSeen(obj.Header.GetInt(34), obj.NewSeqNo.Obj);
        }

        private void HandleLogon(Logon logon)
        {
            if (NextInboundSeqNumber > logon.Header.GetInt(Tags.MsgSeqNum))
            {
                var logout = new Logout
                {
                    Text = new Text("Logon seq number too low")
                };
                _outboundObservable.OnNext(logout);
                OnDisconnect("Logon seq number too low");
            }
            else
            {
                var ans = new Logon(new EncryptMethod(), new HeartBtInt(60));
                // ans.IsSetResetSeqNumFlag()
                if (logon.IsSetResetSeqNumFlag())
                {
                    ans.SetField(new ResetSeqNumFlag(true));
                }

                _outboundObservable.OnNext(ans);
                if (NextInboundSeqNumber < logon.Header.GetInt(Tags.MsgSeqNum))
                {
                    var resendRequest = new ResendRequest(new BeginSeqNo(NextInboundSeqNumber), new EndSeqNo(0));
                    _outboundObservable.OnNext(resendRequest);
                }
            }
        }

        private Message AddSendTime(Message message)
        {
            return message;
        }

        private Message AddCompIds(Message message)
        {
            message.Header.SetField(new SenderCompID(AcceptorCompId), true);
            message.Header.SetField(new TargetCompID(InitiatorCompId), true);
            return message;
        }

        private Message PersistIn(Message message)
        {
            Console.WriteLine($"Message in: {message.GetType().Name}, data: {message.ToString()}");

            return message;
        }

        private Message PersistOut(Message message)
        {
            Console.WriteLine($"Message out: {message.GetType().Name}, data: {message}");
            return message;
        }

        private Message AddSequenceNumber(Message message)
        {
            if (message.Header.IsSetField(43))
            {
                return message;
            }

            var seqNumber = _nextOutboundSeqNumber++;
            message.Header.SetField(new MsgSeqNum(seqNumber));
            return message;
        }


        public int NextInboundSeqNumber => _nextInboundSeqNumber;

        #region OnConnect/Disconnect

        protected virtual void OnConnect(IDisposableList list, IObserver<Message> connectionWriter,
            CancellationTokenSource cancellationTokenSource)
        {
            _killObs.OnNext(new Tuple<string, Exception>("From OnConnect",
                new Exception("Making sure previous connection is dead")));
            var connectionOutObservable = _endOfOutboundObservable.Subscribe(
                ((Action<Message>) connectionWriter.OnNext).WrapWithException(
                    KillActiveConnectionFromException("connectionWriter.OnNext")),
                KillActiveConnectionFromException("connectionWriter.OnNext"),
                connectionWriter.OnCompleted);
            var heartBeatTimer = new Timer(10000);
            heartBeatTimer.Elapsed += (sender, args) => { _outboundObservable.OnNext(new Heartbeat()); };

            heartBeatTimer?.Start();
            var killRegistration = _endOfKillObs
                .Subscribe(
                    data =>
                    {
                        if (!cancellationTokenSource.IsCancellationRequested)
                        {
                            cancellationTokenSource.Cancel();
                        }

                        ;
                        var s = data.Item2 == null ? "None" : data.Item2.Message;
                        Console.WriteLine($"kill subscribe. From: {data.Item1}, exception: {s}");
                    },
                    (e) => { },
                    () => { });
            list.Add(
                Disposable.Create(
                    () =>
                    {
                        _killObs.OnNext(new Tuple<string, Exception>("From OnConnect",
                            new Exception("Making sure previous connection is dead")));
                        killRegistration.Dispose();
                        connectionOutObservable.Dispose();
                        heartBeatTimer.Stop();
                        heartBeatTimer.Dispose();
                    }));
        }

        public IDisposable OnConnect(IObserver<Message> connectionWriter,
            CancellationTokenSource cancellationTokenSource)
        {
            IDisposableList list = new DisposableList();
            OnConnect(list, connectionWriter, cancellationTokenSource);
            return list;
        }

        public Action OnDisconnectAction(string reason)
        {
            return () => OnDisconnect(reason);
        }

        private void OnDisconnect(string reason)
        {
            Console.WriteLine("DisConnect");
            KillActiveConnection(reason, null);
        }

        #endregion

        #region Kill Active Connection

        private Action KillActiveConnection(string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                throw new ArgumentNullException(nameof(reason));
            }

            return () => { KillActiveConnection(reason, null); };
        }

        private Action<Exception> KillActiveConnectionFromException(string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                throw new ArgumentNullException(nameof(reason));
            }

            return exception => KillActiveConnection(reason, exception);
        }

        private void KillActiveConnection(string reason, Exception exception)
        {
            _killObs.OnNext(new Tuple<string, Exception>(reason, exception));
        }

        #endregion

        public void IncrementInboundSeqNumber()
        {
            _nextInboundSeqNumber++;
        }

        public void SetInboundSeqNumber(int getValue)
        {
            _nextInboundSeqNumber = getValue;
        }

        public void MarkAsSeen(int getInt, int getValue)
        {
        }

        public void SendIncomingMessage(Message message)
        {
            _inboundObservable.OnNext(message);
        }

        public void SendOutgoingMessage(Message message)
        {
            _outboundObservable.OnNext(message);
        }


        public void StartAux()
        {
        }

        private void ReplayOutBoundMessages(int beginSeq, int endSeq, Action<int, Message> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (endSeq == 0)
            {
                endSeq = _nextOutboundSeqNumber - 1;
            }

            for (var i = beginSeq; i <= endSeq; i++)
            {
                var message = GetOutboundMessage(i);
                action(i, message);
            }
        }

        private Message GetOutboundMessage(int i)
        {
            return null;
        }
    }
}