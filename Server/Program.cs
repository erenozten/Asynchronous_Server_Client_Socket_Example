using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatService
{
    class Program
    {

        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static List<Socket> _clienSockets = new List<Socket>();

        // Receive buffer.  
        private static byte[] _buffer = new byte[1024];

        // will be used for getting time when connection made
        private static DateTime? _requestTime;

        // will be used for warn user if client sends more than one message in a second
        private static int _warnCount = 0;

        static void Main(string[] args)
        {
            Console.Title = "SERVER SIDE";
            Console.ReadKey();
        }


    }
}
