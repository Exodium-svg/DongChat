
namespace Common.Network.Packets.ClientBound
{
    public class VersionPayload : NetMessage
    {
        public int Version { get; set; }
        public VersionPayload(int version) : base(PacketType.VersionPayload) => Version = version;

        protected override ArraySegment<byte> ToBuffer() => new ArraySegment<byte>(BitConverter.GetBytes(Version));
        
    }
}
