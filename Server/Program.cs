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

            // Accept connection from clients
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);

            // Indicates that the server is ready for connection
            Console.WriteLine("Server is ready...");
        }


        private static void AcceptCallBack(IAsyncResult ar)
        {
            var socket = _serverSocket.EndAccept(ar);
            _clienSockets.Add(socket);
            Console.WriteLine("Client connected");
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
        }

        private static void ReceiveCallBack(IAsyncResult ar)
        {
            var socket = (Socket)ar.AsyncState;
            var received = socket.EndReceive(ar);

            var dataBuf = new byte[received];
            Array.Copy(_buffer, dataBuf, received);

            var text = Encoding.ASCII.GetString(dataBuf);
            Console.WriteLine("Text received: " + text);

            var response = text;

            var data = Encoding.ASCII.GetBytes(response);

            // Send data to connected socket (a method must be implemented as an argument to send callback here)
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);
        }


    }
}
