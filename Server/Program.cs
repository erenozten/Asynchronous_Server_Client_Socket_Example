using System.Diagnostics;
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

            if (_requestTime == null)
            {
                _requestTime = DateTime.Now;
            }
            else
            {
                var secondBetweenRequests = DateTime.Now.Subtract((DateTime)_requestTime).TotalSeconds;
                Debug.WriteLine(secondBetweenRequests);
                _requestTime = DateTime.Now;

                if (secondBetweenRequests <= 1)
                {
                    response += "WARNING! Don't send multiple messages in one second. You have been warned for the first and last time.";
                    _warnCount++;

                }
            }

            // if user is warned, close connection and send closeMessage as "CONNECTION CLOSED!" to break the connection loop
            if (_warnCount > 1)
            {
                var closeMessage = Encoding.ASCII.GetBytes("CONNECTION CLOSED!");
                socket.BeginSend(closeMessage, 0, closeMessage.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                socket.Close();
            }
            else
            {
                var data = Encoding.ASCII.GetBytes(response);
                socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);
            }

        }

        private static void SendCallback(IAsyncResult AR)
        {
            // Retrieve the socket from the state object.  
            var socket = (Socket)AR.AsyncState;

            // Complete sending data to the remote device.  
            socket.EndSend(AR);
        }

    }
}
