using System.Runtime.InteropServices;

namespace Common.Network
{
    public enum PacketType : int
    {
        Ping,
        CVarPayload
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        int _size;
        int _type;
        int _flags;
        int _responseId;
        public Header(int size, PacketType type, int flags, int responseId = 0)
        {
            _size = size;
            _type = (int)type;
            _flags = flags;
            // If responseID is not zero it should be known that 
            _responseId = responseId;
        }

        public PacketType Type => (PacketType)_type;
        public int Size => _size;
        public int Flags => _flags;
        public int ResponseId => _responseId;
    }
}
