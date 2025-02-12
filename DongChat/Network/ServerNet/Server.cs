using Common.Network.Packets;
using Common.Utils;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Common.Network.ServerNet
{
    public sealed class Server
    {
#pragma warning disable CS8618 // Justification: no way around it.
        public static Server Instance { get; private set; }
#pragma warning restore CS8618
        readonly ClientListener _listener;
        readonly CVars _cVars;
        readonly ConcurrentDictionary<PacketType, Action<User, ReadOnlyMemory<byte>>> _routes = new();
        readonly ConcurrentDictionary<PacketType, MiddleWare> _middlewares = new();
        public Server()
        {
            _cVars = new CVars("cVars");

            IPAddress address = IPAddress.Parse(_cVars.GetString("network.address", "0.0.0.0"));
            _listener = new ClientListener(address, _cVars.Get<int>("network.port", 22580));
            _listener.ClientConnected += ClientConnected;

            Instance = this;
        }
        ~Server() => _cVars.Save("cVars");
        public void RegisterRoute(PacketType packetType, Action<User, ReadOnlyMemory<byte>> route) => _routes[packetType] = route;
        private void ClientConnected(object? sender, ClientConnectedEventArgs e)
        {
            User user = new User(e.RemoteSocket);

            user.MessageReceived += MessageReceived;
        }

        private void MessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            User? user = sender as User;

            if (user == null)
                return;

            // middleware?
            if(_middlewares.TryGetValue(e.Header.Type, out MiddleWare? middleware))
            {
                // Send a error message if fails and do nothing.
            }

            // Send result to client
            Result result = Process(user, e.Header.Type, e.Data);
            
        }

        public Result Process(User user, PacketType type, ReadOnlyMemory<byte> buffer)
        {
            
            return Result.Success;
        }
    }
}
