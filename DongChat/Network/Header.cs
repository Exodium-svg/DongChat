using System.Runtime.InteropServices;

namespace Common.Network
{
    // Problem with this being, We need plugins to make their own ids, And how will the client know how to handle those ids
    public enum PacketType : int
    {
        Ping,
        CVarPayload,
        VersionPayload
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        int _size;
        int _type;
        int _flags;
        int _responseId;
        int _chunkId;
        public Header(int size, PacketType type, int flags)
        {
            _size = size;
            _type = (int)type;
            _flags = flags;
            // If responseID is not zero it should be known that 
            _responseId = 0;
            _chunkId = 0;
        }

        public PacketType Type => (PacketType)_type;
        public int Size => _size;
        public int Flags => _flags;
        public int ResponseId => _responseId;
        public int ChunkId => _chunkId;
    }
}
