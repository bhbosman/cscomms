using System.IO;

namespace MessageBlock
{
    public static class StreamExtensions
    {
        public static int CopyPartial(this Stream source, Stream destination, int count)
        {
            byte[] buffer = new byte[4096];
            var leftOver = count;
            var written = 0;
            while (leftOver > 0)
            {
                var n = source.Read(
                    buffer,
                    0,
                    buffer.Length < leftOver ? buffer.Length : leftOver);
                if (n == 0)
                {
                    break;
                }
                destination.Write(buffer, 0, n);
                leftOver -= n;
                written += n;
            }
            return written;
        }
    }
}