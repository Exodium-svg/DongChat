using Common.Utils;
using System.Runtime.InteropServices;

namespace Common.Network.Packets.ClientBound
{
    public class CvarPayload : NetMessage
    {
        ReadOnlyMemory<byte> CvarBytes { get; set; }
        public CvarPayload(in ReadOnlyMemory<byte> cVarBytes) : base(PacketType.CVarPayload)
        {
            CvarBytes = cVarBytes;
        }

        protected override ArraySegment<byte> ToBuffer()
        {
            int headerSize = Marshal.SizeOf<Header>();
            int totalSize = headerSize + CvarBytes.Length + sizeof(int);

            ArraySegment<byte> buffer = NetMessage._arrayPool.Rent(totalSize);

            Array.Clear(buffer.Array!, buffer.Offset, totalSize);

            ArraySegment<byte> bodyBuffer = buffer.Slice(headerSize, totalSize - headerSize);

            using MemoryStream ms = new MemoryStream(buffer.Array!, buffer.Offset + headerSize, bodyBuffer.Count, writable: true);

            ms.WritePrefixBytes(CvarBytes.Span);

            Header header = new Header(bodyBuffer.Count, Type, 0);

            MemoryMarshal.Write(buffer.AsSpan(0, headerSize), in header);

            return buffer;
        }
    }
}
