using Common.Network.Packets;

namespace Common.Network.ServerNet
{
    public abstract class MiddleWare
    {
        public abstract Result Allowed(User user, Header header, ReadOnlyMemory<byte> data);
        public void SendMessage(User user, PacketType type, ReadOnlyMemory<byte> data)
        {
            user.Send(type, data);
        }
    }
}
