using System.Buffers;

namespace Common.Network
{
    public abstract class NetMessage : IDisposable
    {
        protected static readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;
        public PacketType Type { get; init; }
        protected ArraySegment<byte> _payload;
        private bool _isReady = false;
        protected NetMessage(PacketType type) => Type = type;
        protected NetMessage(PacketType type, int size)
        {
            Type = type;
            _payload = new ArraySegment<byte>(_arrayPool.Rent(size));
        }
        protected virtual ArraySegment<byte> ToBuffer() => throw new NotImplementedException("");
        public void Dispose()
        {
            if (Bytes.Array != null)
                _arrayPool.Return(Bytes.Array, false);
        }
        public ArraySegment<byte> Bytes { 
            get  {
                if (_isReady == false)
                    _payload = ToBuffer();

                _isReady = true;
                return _payload; 
            } 
        }
    }
}
