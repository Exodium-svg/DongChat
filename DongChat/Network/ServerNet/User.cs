using Common.Utils;
using System.Buffers;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Common.Network.ServerNet
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public Header Header { get; set; }
        public ReadOnlyMemory<byte> Data { get; set; }

        public MessageReceivedEventArgs(Header header, ReadOnlyMemory<byte> data)
        {
            Header = header;
            Data = data;
        }
    }
    public class User
    {
        protected static readonly ArrayPool<byte> _arrayPool = ArrayPool<byte>.Shared;

        readonly Socket _remoteSocket;
        readonly NetworkStream _remoteStream;
        readonly CVars _cVarsSession = new();
        public string Address { get => _remoteSocket.RemoteEndPoint!.ToString()!; }
        public Stream Stream { get => _remoteStream; }
        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
        public User(Socket remoteSocket)
        {
            _remoteSocket = remoteSocket;
            _remoteStream = new NetworkStream(remoteSocket, false);
        }

        public void Send(PacketType type, ReadOnlyMemory<byte> buffer)
        {
            Header header = new Header(buffer.Length, type, 0, 0);

            int totalSize = header.Size + Marshal.SizeOf(header);

            Span<byte> packetBuffer = totalSize > 1024 ? new byte[totalSize] : stackalloc byte[totalSize];
            MemoryMarshal.Write(packetBuffer, in header);

            buffer.Span.CopyTo(packetBuffer.Slice(Marshal.SizeOf(header), buffer.Length));

            _remoteSocket.Send(packetBuffer);
        }

        public void Close()
        {
            lock (_remoteSocket)
            {
                if (!_remoteSocket.Connected)
                    _remoteSocket.Close();
                else
                {
                    try { _remoteSocket.Shutdown(SocketShutdown.Send); }
                    catch (SocketException) { }
                    finally { _remoteSocket.Close(); }
                }
            }
        }
        public void Listen()
        {
            SocketAsyncEventArgs sockEvent = new();
            sockEvent.SetBuffer(new Memory<byte>(new byte[Marshal.SizeOf<Header>()]));
            sockEvent.UserToken = this;
            sockEvent.Completed += OnMessage;

            _remoteSocket.ReceiveAsync(sockEvent);
        }

        private void OnMessage(object? sender, SocketAsyncEventArgs e)
        {
            User user = (User)e.UserToken!; // we always fill it

            // Header should always be sent fully at once, or you will be disconnected.
            if (e.BytesTransferred < Marshal.SizeOf<Header>() && e.SocketError != SocketError.Success)
            {

                // something went wrong? --> check for errors
                Close();
                return;
            }

            Header header = Unsafe.ReadUnaligned<Header>(ref MemoryMarshal.GetReference(e.MemoryBuffer.Span));

            if (header.Size > ushort.MaxValue) // Not acceptable!
            {
                user.Close();
                return;
            }

            byte[] rentedBuffer = _arrayPool.Rent(header.Size);

            //Shouldn't use the stack Justification: A thread has a maximum memory bandwith of 1MB by default UP to 4MB if setup correctly.
            //Span<byte> buffer = header.Size > STACK_THRESH_HOLD ? new byte[header.Size] : stackalloc byte[header.Size];

            try
            {
                user.Stream.ReadExactly(rentedBuffer);
            }
            catch (EndOfStreamException)
            {
                user.Close();
                return;
            }

            // we make a defensive copy, sigh.... TODO: rewrite this function, as it is doodoo
            MessageReceived?.Invoke(user, new MessageReceivedEventArgs(header, rentedBuffer.ToArray()));

            _arrayPool.Return(rentedBuffer);
            OnMessage(sender, e);
        }

    }
}
