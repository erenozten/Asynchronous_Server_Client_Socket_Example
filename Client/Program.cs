using System.Net;
using System.Net.Sockets;

namespace ChatService
{
    class Program
    {
        private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        static void Main(string[] args)
        {
            Console.Title = "CLIENT SIDE";
            LoopConnect();
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
