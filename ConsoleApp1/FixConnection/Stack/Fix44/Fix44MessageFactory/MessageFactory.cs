using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Comms;
using FixConnection.Messages;
using QuickFix.Fields;
using QuickFix.Fields.Converters;
using Unity;
using QuickFix.FIX44;
namespace FixConnection.Stack.Fix44.Fix44MessageFactory
{
    public class MessageFactory : IStackComponent<ParsedFixMessage, Message>
    {
        public IDisposable CreateStackData(
            ConnectionType connectionType,
            CancellationTokenSource cancellationTokenSource, IUnityContainer unityContainer)
        {
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                    return Disposable.Empty;
                case ConnectionType.Initiator:
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }

        public IObservable<QuickFix.FIX44.Message> CreateInbound(
            ConnectionType connectionType,
            InOutboundParams<ParsedFixMessage> data)
        {
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                    var factory = new QuickFix.FIX44.MessageFactory();
                    var dataDictionary = data.Container.Resolve<QuickFix.DataDictionary.DataDictionary>(QuickFix.Values.BeginString_FIX43);
                    return data.NextObservable.Select(source =>
                    {
                        if (source.BeginString != QuickFix.Values.BeginString_FIX44)
                        {
                            throw new MessageFactoryError($"A FIX.4.4 message was expected. Received header: {source.BeginString}"); 
                        }
                        var msg = factory.Create(source.BeginString, source.MessageType);
                        msg.FromString(source.CompleteFixMessage.ReadString(),false,
                            dataDictionary, dataDictionary);
                        return msg as QuickFix.FIX44.Message;
                    });
                case ConnectionType.Initiator:
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }
        public IObservable<ParsedFixMessage> CreateOutbound(
            ConnectionType connectionType,
            InOutboundParams<QuickFix.FIX44.Message> data)
        {
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                    return data.NextObservable.Select(message =>
                    {
                        message.Header.SetField(new SendingTime(DateTime.Now.ToUniversalTime(), TimeStampPrecision.Millisecond));
                        if (QuickFix.Values.BeginString_FIX44 != message.Header.GetString(8))
                        {
                            throw new MessageFactoryError($"Begin tag not equal to {QuickFix.Values.BeginString_FIX44}");
                        }
                        return new ParsedFixMessage(QuickFix.Values.BeginString_FIX44, message.Header.GetString(35), -1, new MessageBlock.MessageBlock(message.ToString()));
                    });
                case ConnectionType.Initiator:
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }
    }
}