using Common.Network.Packets.ClientBound;
using System.Net;
using System.Net.Sockets;

namespace Common.Network.ServerNet
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public required Socket RemoteSocket { get; init; }
    }
    public class ClientListener
    {
        readonly TcpListener _listener;
        readonly Thread _listenThread;
        public event EventHandler<ClientConnectedEventArgs> ClientConnected;
        public ClientListener(IPAddress address, int port)
        {
            _listener = new TcpListener(address, port);

            _listener.Start();

            _listenThread = new Thread(ListenLoop);
            _listenThread.Name = "ClientListener";
            _listenThread.Start();
        }

        private async void ListenLoop()
        {
            while (true)
            {
                Socket? remoteSocket = null;
                try
                {
                    remoteSocket = await _listener.AcceptSocketAsync();

                    if(!remoteSocket.Connected)
                    {
                        remoteSocket.Close();
                        continue;
                    }


                    ClientConnected.Invoke(this, new ClientConnectedEventArgs() { RemoteSocket = remoteSocket});
                    continue;
                } 
                catch(IndexOutOfRangeException) { }
                catch(SocketException) { }

                remoteSocket?.Close();
            }
        }
    }
}
