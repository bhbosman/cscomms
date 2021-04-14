using System;
using Google.Protobuf;

namespace ProtobufExt
{
    public static class TypeCodeMessageExt  
    {
        public static void WriteTo<T>(this ITypeCodeMessage<T> msg, MessageBlock.MessageBlock messageBlock)where T : ITypeCodeMessage<T>
        {
            messageBlock.WriteTypeCode(msg.TypeCode);
            messageBlock.WriteMessageLength((uint)msg.CalculateSize());
            using (var stream = new CodedOutputStream(messageBlock, true))
            {
                msg.WriteTo(stream);
            }
        }
        
        public static void MergeFrom<T>(this ITypeCodeMessage<T> msg, MessageBlock.MessageBlock messageBlock)where T : ITypeCodeMessage<T>
        {
            var tc = messageBlock.ReadTypeCode();
            if (tc != msg.TypeCode)
            {
                throw new Exception($"Wrong typecode in stream. Expected typecode: {msg.TypeCode}, received: {tc}");
            }
            var messageLength = messageBlock.ReadMessageLength();
            using (var stream = new CodedInputStream(messageBlock, true))
            {
                msg.MergeFrom(stream);
            }
        }
        
        public static int MessageBlockSize<T>(this ITypeCodeMessage<T> msg)where T : ITypeCodeMessage<T>
        {
            return msg.CalculateSize() + 8;
        }
    }
    
}