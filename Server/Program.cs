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
            StartServer();
            Console.ReadKey();
        }

        private static void StartServer()
        {
            // Associate socket with local endpoint using IPAddress and port
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));

            // Socket is in a listening state
            _serverSocket.Listen(1);

            // Accept connection from clients (a method must be implemented as an argument named like AcceptCallBack here)
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallBackHere), null);

            // Indicates that the server is ready for connection
            Console.WriteLine("Server is ready...");
        }



    }
}
