using System;
using System.Windows.Forms;
using Comms.Stack;
using Comms.Stack.BottomStack;
// using ConsoleApp1.Comms;

namespace Comms
{
    
    
    public static class CommonStacks
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly IStackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock> CompressedStackBuilder;
        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly IStackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock> UncompressedStackBuilder;
        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly IStackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock> CompressedTlsStackBuilder;
        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly IStackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock> UncompressedTlsStackBuilder;

        static CommonStacks()
        {
            CompressedStackBuilder = CreateCompressedBuilder();
            UncompressedStackBuilder = CreateUncompressedBuilder();
            CompressedTlsStackBuilder = CreateCompressedTlsBuilder();
            UncompressedTlsStackBuilder = CreateUncompressedTlsBuilder();
        }

        private static IStackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock> CreateUncompressedTlsBuilder()
        {
            var result = new StackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock>(
                BottomStackMessageBlock.Create().CreateFactory());
            return result;
        }

        private static IStackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock> CreateCompressedTlsBuilder()
        {
            var result = new StackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock>(
                BottomStackMessageBlock.Create().CreateFactory());
            return result;
        }

        private static IStackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock> CreateUncompressedBuilder()
        {
            var result = new StackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock>(
                BottomStackMessageBlock.Create().CreateFactory());
            return result;
        }

        private static IStackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock> CreateCompressedBuilder()
        {
            var result = new StackBuilder<MessageBlock.MessageBlock, MessageBlock.MessageBlock>(
                BottomStackMessageBlock.Create().CreateFactory());
            return result;
        }
    }
}