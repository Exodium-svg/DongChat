using System.Runtime.InteropServices;
using System.Text;

namespace Common.Utils
{
    public static class StreamExtensions
    {
        public static T Read<T>(this Stream stream) where T : struct
        {
            Span<byte> buffer = stackalloc byte[Marshal.SizeOf<T>()];
            stream.ReadExactly(buffer);

            return MemoryMarshal.Read<T>(buffer);
        }

        public static string ReadString(this Stream stream)
        {
            int strLen = stream.Read<int>();

            Span<byte> strBuff = strLen > 1024 ? new byte[strLen] : stackalloc byte[strLen];
            stream.ReadExactly(strBuff);

            return Encoding.UTF8.GetString(strBuff);
        }

        public static void Write<T>(this Stream stream, T value) where T : struct
        {
            Span<byte> buffer = stackalloc byte[Marshal.SizeOf<T>()];
            MemoryMarshal.Write(buffer, value);

            stream.Write(buffer);
        }

        public static void WriteString(this Stream stream, string value)
        {
            Span<byte> strBytes = Encoding.UTF8.GetBytes(value);
            stream.Write<int>(strBytes.Length);
            stream.Write(strBytes);
        }
    }
}
