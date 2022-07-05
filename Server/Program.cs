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
        private static byte[] _bufferAsStorage = new byte[1024];

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

            // Put a socket to a 'listening' state. Pending connections count set to one:
            _serverSocket.Listen(1);

            // Create an async operation to accept incoming connection attempts from clients.
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);

            // Inform server that the server is started and ready for getting connections in console.
            Console.WriteLine("Server is listening...");
        }

        private static void AcceptCallBack(IAsyncResult ar)
        {
            // EndAccept: Accept the connection request and create a socket to communicate with this client.
            // EndAccep Method returns a buffer to get the transferred data.
            Socket socket = _serverSocket.EndAccept(ar);

            // ~Add socket created for client to List<Socket> _clientSockets
            _clienSockets.Add(socket);

            Console.WriteLine("Client connected");

            socket.BeginReceive(_bufferAsStorage, 0, _bufferAsStorage.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
        }

        private static void ReceiveCallBack(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;

            // ~ Get the data to 'receivedNumberOfBytes' variable and Stop reading:
            int receivedNumberOfBytes = socket.EndReceive(ar);

            // ~ databuf: the data in a byte-array
            byte[] receivedDataAsBufferAsByte = new byte[receivedNumberOfBytes];

            Array.Copy(_bufferAsStorage, receivedDataAsBufferAsByte, receivedNumberOfBytes);

            // decode byte (to string):
            string dataAsText = Encoding.ASCII.GetString(receivedDataAsBufferAsByte);

            // Show received text in Server:
            Console.WriteLine("Text received by me (Server): " + dataAsText);

            var response = dataAsText;

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
                    response += "\n \nWARNING! Don't send multiple messages in one second. You have been warned for the first and last time.\n";
                    _warnCount++;

                }
            }

            // Messages sent in one second will also be handled here. Client can not send messages in one second.
            // If so, a warning message will shown to user, if this happens again, connection will be terminated.
            // Send closeMessage as "I WARNED YOU. CONNECTION CLOSED!" to break the connection loop
            if (_warnCount > 1)
            {
                byte[] closeMessageAsByte = Encoding.ASCII.GetBytes("\nI WARNED YOU. CONNECTION CLOSED!\n");

                // send data to connected socket:
                socket.BeginSend(closeMessageAsByte, 0, closeMessageAsByte.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                socket.Close();
            }
            else
            {
                byte[] data = Encoding.ASCII.GetBytes(response);
                socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                socket.BeginReceive(_bufferAsStorage, 0, _bufferAsStorage.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);
            }

        }

        private static void SendCallback(IAsyncResult AR)
        {
            // Retrieve the socket from the state object.  
            Socket socket = (Socket)AR.AsyncState;

            // Complete sending data to the remote device.  
            socket.EndSend(AR);
        }

    }
}
