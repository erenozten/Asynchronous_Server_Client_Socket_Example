using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatService
{
    class Program
    {
        private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        static void Main(string[] args)
        {
            Console.Title = "CLIENT SIDE";
            LoopConnect();
            SendLoop();
        }

        private static void SendLoop()
        {
            while (true)
            {
                // wait a message from client
                Console.Write("Enter message: ");
                var message = Console.ReadLine();

                var buffer = Encoding.ASCII.GetBytes(message);
                _clientSocket.Send(buffer);

                var receivedBuf = new byte[1024];
                var rec = _clientSocket.Receive(receivedBuf);

                var data = new byte[rec];
                Array.Copy(receivedBuf, data, rec);
                var receivedText = Encoding.ASCII.GetString(data);
                Console.WriteLine("Received: " + receivedText);

                if (receivedText == "WARNING! Don't send multiple messages in one second. You have been warned for the first and last time.")
                {
                    break;
                }
            }
        }

        // Connection must be in a loop since we need to handle multiple messages sent by client(s) whether they sent messages in one second or not.
        // If so, a warning message will shown to user, if this happens again, connection will be terminated.
        private static void LoopConnect()
        {
            // if socket is not connected to remote host, try to connect:
            var attempts = 0;
            while (!_clientSocket.Connected)
            {
                try
                {
                    attempts++;
                    _clientSocket.Connect(IPAddress.Loopback, 100);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("Error! Connection attempts: " + attempts);
                }
            }

            Console.Clear();
            Console.WriteLine("Connected");
        }

    }
}
