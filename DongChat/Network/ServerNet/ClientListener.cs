using Common.Network.Packets.ClientBound;
using System.Net;
using System.Net.Sockets;

namespace Common.Network.ServerNet
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public TcpClient Client { get; set; }
    }
    public class ClientListener
    {
        readonly TcpListener _listener;
        readonly Thread _listenThread;
        public event EventHandler<ClientConnectedEventArgs> ClientConnected;
        CvarPayload cVarPayload;
        public ClientListener(IPAddress address, int port)
        {
            if (File.Exists("clientCvars"))
                cVarPayload = new CvarPayload(new ReadOnlyMemory<byte>(File.ReadAllBytes("clientCvars")));
            else
                cVarPayload = new CvarPayload(new ReadOnlyMemory<byte>(Array.Empty<byte>()));
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
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();

                    if (client.Connected)
                        client.GetStream().Write(cVarPayload.Bytes);
                    else
                    {
                        client.Close();
                        continue;
                    }

                    ClientConnected.Invoke(null, new ClientConnectedEventArgs() { Client = client});
                } 
                catch(IndexOutOfRangeException) { continue; }
                catch(SocketException) { continue; }
            }
            cVarPayload.Dispose();
        }
    }
}
