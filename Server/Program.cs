using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatService
{
    class Program
    {

        // Socket for the server, in constructor, specified protocol type as TCP
        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Sockets are added to _clientSockets list for each socket since clients represent clients
        // For example,if you have two client console applications
        // you will have 2 sockets in the _clientSockets list
        private static List<Socket> _clientSockets = new List<Socket>();

        // Used for receiving buffer.  
        private static byte[] _bufferAsStorage = new byte[1024];

        // Used for getting time when connection made
        private static DateTime? _requestTime;

        // Used for warning user
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

            // Put a socket to a 'listening' state. Pending connections count set to one (?):
            _serverSocket.Listen(1);

            // Create an async operation to accept incoming connection attempts from clients.
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);

            // Inform server in console that the server is started and ready for getting connections.
            Console.WriteLine("Server is started. Listening...");
        }

        private static void AcceptCallBack(IAsyncResult ar)
        {
            // EndAccept: Accept the connection request and create a socket to communicate with this client.
            // EndAccep Method returns a socket to communicate with remote host.
            Socket socket = _serverSocket.EndAccept(ar);

            // ~Add socket created for client to List<Socket> _clientSockets
            // See that we are giving "socket" argument that returned from "EndAccept" to _clientSockets
            _clientSockets.Add(socket);

            Console.WriteLine("Client connected");

            // ~Since we finished EndAccept process and added socket to List<Socket>, we can begin to receive data from client 
            socket.BeginReceive(_bufferAsStorage, 0, _bufferAsStorage.Length, SocketFlags.None, new AsyncCallback(ReceiveCallBack), socket);

            // Begin to accept incoming connection attempt
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
            Console.WriteLine("Text received by me as Server: " + dataAsText);

            var response = dataAsText;

            if (_requestTime == null)
            {
                _requestTime = DateTime.Now;
            }
            else
            {
                var secondBetweenRequests = DateTime.Now.Subtract((DateTime)_requestTime).TotalSeconds;
                //Debug.WriteLine(secondBetweenRequests);
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

                // send closeMessageAsByte (encoded message) to connected socket as
                // "I WARNED YOU. CONNECTION CLOSED!" (decoded message) to inform user that connection is closed:
                socket.BeginSend(closeMessageAsByte, 0, closeMessageAsByte.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                
                // We dont have a BeginReceive method here after BeginSend method since we are to close connection.
                // Because _warnCount is bigger than 1, we are disconnecting socket.

                // Close socket's connection
                socket.Close();
            }
            else
            {
                byte[] data = Encoding.ASCII.GetBytes(response);
                socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                
                // Begin to receive data since connection is not closed due to _warnCount is not bigger than 1.
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
