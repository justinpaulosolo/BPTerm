using System.Net.Sockets;
using System.Text;

namespace BPTerm.DBus
{
    public class DBusConnection
    {
        Socket? _socket;
        
        public void Connect(string socketPath)
        {
            UnixDomainSocketEndPoint endpoint = new UnixDomainSocketEndPoint(socketPath);

            _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            _socket.Connect(endpoint);
            Console.WriteLine("[[Connected to DBus]]");

            // Initiate SASL Handshake
            SendRaw(new byte[]{0});

            int uid = GetUID();
            string uidHex = Convert.ToHexString(Encoding.ASCII.GetBytes(uid.ToString()));

            string authCommand = $"AUTH EXTERNAL {uidHex}\r\n";

            // Send auth
            SendRaw(Encoding.ASCII.GetBytes(authCommand));

            string authResponse = ReadLine();

            if (authResponse.StartsWith("OK"))
            {
                SendRaw(Encoding.ASCII.GetBytes("BEGIN\r\n"));
                Console.WriteLine("[[Authentication successful]]");
            }
            else
            {
                throw new InvalidOperationException($"Authentication failed: {authResponse}");
            }
        }

        public string ReadLine()
        {
            byte[] buffer = new byte[1];
            List<byte> bytes = new List<byte>();

            while (true)
            {
                int bytesRead = _socket!.Receive(buffer, 0, 1, SocketFlags.None);
                if (bytesRead == 0)
                    throw new EndOfStreamException("Unexpected end of stream");

                bytes.Add(buffer[0]);
                if (buffer[0] == '\n')
                    break;
            }
            return Encoding.ASCII.GetString(bytes.ToArray()).TrimEnd('\r','\n');
        }

        public byte[] ReceiveExactly(int count)
        {
            byte[] buffer = new byte[count];
            int received = 0;
            while (received < count)
            {
                int bytesRead = _socket!.Receive(buffer, received, count - received, SocketFlags.None);
                if (bytesRead == 0)
                    break;

                received += bytesRead;
            }
            if (received < count)
                throw new EndOfStreamException($"Expected {count} bytes, but received only {received}");

            return buffer;
        }

        public void SendRaw(byte[] data)
        {
            int sent = 0;
            while (sent < data.Length)
            {
                int bytesSent = _socket!.Send(data, sent, data.Length - sent, SocketFlags.None);
                sent += bytesSent;
            }
        }

        [System.Runtime.InteropServices.DllImport("libc", EntryPoint="getuid")]
        private static extern int GetUID();
    }
}