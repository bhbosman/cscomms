using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Comms;
using FixConnection.Messages;
using QuickFix.Fields;
using QuickFix.Fields.Converters;
using QuickFix.FIX43;
using QuickFix.FIX44;
using Unity;

namespace FixConnection.Stack.Fix43MessageFactory
{
    public class MessageFactory : IStackComponent<ParsedFixMessage, QuickFix.FIX43.Message>
    {
        public IDisposable CreateStackData(
            ConnectionType connectionType,
            CancellationTokenSource cancellationTokenSource, IUnityContainer unityContainer)
        {
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                case ConnectionType.Initiator:
                    return Disposable.Empty;
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
        }

        public IObservable<QuickFix.FIX43.Message> CreateInbound(
            ConnectionType connectionType,
            InOutboundParams<ParsedFixMessage> data)
        {
            switch (connectionType)
            {
                case ConnectionType.Acceptor:
                    var factory = new QuickFix.FIX43.MessageFactory();
                    var dataDictionary = data.Container.Resolve<QuickFix.DataDictionary.DataDictionary>(QuickFix.Values.BeginString_FIX43);
                    return data.NextObservable.Select(source =>
                    {
                        if (source.BeginString != QuickFix.Values.BeginString_FIX43)
                        {
                            throw new MessageFactoryError($"A FIX.4.3 message was expected. Received header: {source.BeginString}"); 
                        }
                        var msg = factory.Create(source.BeginString, source.MessageType);
                        msg.FromString(source.CompleteFixMessage.ReadString(),false,
                            dataDictionary, dataDictionary);
                        return msg as QuickFix.FIX43.Message;
                    });    
                case ConnectionType.Initiator:
                default:
                    throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null);
            }
            
        }
        public IObservable<ParsedFixMessage> CreateOutbound(
            ConnectionType connectionType,
            InOutboundParams<QuickFix.FIX43.Message> data)
        {
            return data.NextObservable.Select(message =>
            {
                message.Header.SetField(new SendingTime(DateTime.Now, TimeStampPrecision.Microsecond));
                if (QuickFix.Values.BeginString_FIX43 != message.Header.GetString(8))
                {
                    throw new MessageFactoryError($"Begin tag not equal to {QuickFix.Values.BeginString_FIX43}");
                }
                return new ParsedFixMessage(QuickFix.Values.BeginString_FIX43, message.Header.GetString(35), -1, new MessageBlock.MessageBlock(message.ToString()));
            });
        }
    }
}