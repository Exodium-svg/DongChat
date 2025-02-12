using Common.Utils;
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
        
        public Server()
        {
            _cVars = new CVars("cVars");

            IPAddress address = IPAddress.Parse(_cVars.GetString("network.address", "0.0.0.0"));
            _listener = new ClientListener(address, _cVars.Get<int>("network.port", 22580));
            _listener.ClientConnected += ClientConnected;

            Instance = this;
        }

        private void ClientConnected(object? sender, ClientConnectedEventArgs e)
        {
            TcpClient client = e.Client;

            SocketAsyncEventArgs sockEvent = new();
            sockEvent.SetBuffer(new Memory<byte>(new byte[Marshal.SizeOf<Header>()]));
            sockEvent.UserToken = client;
            sockEvent.Completed += OnMessage;

            client.Client.ReceiveAsync(sockEvent);
        }

        private void OnMessage(object? sender, SocketAsyncEventArgs e)
        {
            TcpClient client = (TcpClient)e.UserToken!; // we always fill it
            
            if(e.BytesTransferred < Marshal.SizeOf<Header>() && e.SocketError != SocketError.Success)
            {

                // something went wrong? --> check for errors
                client.Close();
                return;
            }

            // Might not function correctly on arm devices...
            //Header header;

            //unsafe
            //{
            //    fixed(byte* pBuff = e.MemoryBuffer.Span)
            //        header = *(Header*)pBuff;
            //}
            
            Header header = Unsafe.ReadUnaligned<Header>(ref MemoryMarshal.GetReference(e.MemoryBuffer.Span));


            if(header.Size > ushort.MaxValue) // Not acceptable!
            {
                client.Close();
                return;
            }

            Span<byte> buffer = header.Size > 1024 ? new byte[header.Size] : stackalloc byte[header.Size];

            try
            {
                client.GetStream().ReadExactly(buffer);
            } catch (EndOfStreamException)
            {
                client.Close();
                return;
            }


            Process(client, header.Type, buffer);

            OnMessage(sender, e);
        }
        public void Process(TcpClient client, PacketType type, scoped Span<byte> buffer)
        {

        }
        ~Server()
        {
            _cVars.Save("cVars");
        }
    }
}
