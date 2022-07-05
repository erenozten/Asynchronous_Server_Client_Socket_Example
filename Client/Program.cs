using System.Net;
using System.Net.Sockets;
using System.Text;


// ! See: Socket programming is a way of connecting two nodes on a network to communicate with each other.
// Basically, it is a one-way Client and Server setup
// where a Client connects, sends messages to the server and the server shows them using socket connection. 

namespace ChatService
{
    class Program
    {
        //
        private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        static void Main(string[] args)
        {
            Console.Title = "CLIENT SIDE";

            // Loop the connection request when project launches.
            // Loop will finish when connection is made successfully.
            LoopConnect();

            // When connection is made successfully, start to listen.
            SendLoop();
        }

        private static void SendLoop()
        {
            while (true)
            {
                Console.Write("Enter message: ");
                // wait a message from client
                string messageToSend = Console.ReadLine();

                byte[] bufferToSend = Encoding.ASCII.GetBytes(messageToSend);

                //!! Sends data to a connected socket
                _clientSocket.Send(bufferToSend);

                // receivedBuf: storage for received data (the "1024" is all about text length)
                byte[] receivedBufferAsStorage = new byte[1024];

                // see: "It is notable that just like in the C language, the ‘send’ and ‘receive’ methods still return the number of bytes sent or received."

                //receives data from a bound socket into a receive buffer
                int receivedNumberOfBytes = _clientSocket.Receive(receivedBufferAsStorage);

                byte[] receivedDataAsByte = new byte[receivedNumberOfBytes];

                // With Array.Copy, trimmed our array buffer (receivedBufferAsStorage)
                // That means, if we got a message that contains 24 byte,
                // our buffer has 1000 empty slots (1024 - 24 = 1000).
                Array.Copy(receivedBufferAsStorage, receivedDataAsByte, receivedNumberOfBytes);

                // So what we do is getting string value of trimmed byte variable. Not buffer's 1024 byte variable.
                // See GetString method below: 
                var receivedDataAsText = Encoding.ASCII.GetString(receivedDataAsByte);

                // What would happen if we just gave our 1024 byte array (receivedBufferAsStorage) to "GetString" method
                // instead of "receivedDataAsByte" like below: 
                //var receivedDataAsText = Encoding.ASCII.GetString(receivedBufferAsStorage);

                // That would create some string from these empty bits as following:
                //"www\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\00\0" ....   // It goes as far as it goes :D

                // Thats why we use Array.Copy method and get a byte[] variable
                // that has exactly the same slot count with the received message

                // !!! A message is sent client to server with Console.ReadLine. Server gets this message. And sends the message back to client.
                // So "receivedDataAsText" is the data that came from server to client right after client sends it.
                // So naming for variables like "receiving, received" is correct (instead of "sending, sent"...)
                Console.WriteLine("The text sent from me as a client to server and then returned back to me from server is: " + receivedDataAsText);

                // if receivedDataAsText is as the following, that means connection is already closed.
                // What this break does is just not to show "Enter message" text to client.
                // Since connection is terminated, "Enter message" text is not necessary anymore.
                if (receivedDataAsText == "\nI WARNED YOU. CONNECTION CLOSED!\n")
                {
                    break;
                }
            }
        }

        // Connection should be in a loop since we need to handle multiple messages sent by client(s).
        private static void LoopConnect()
        {
            // if socket is not connected to remote host, try to connect:
            var attempts = 0;
            while (!_clientSocket.Connected)
            {
                try
                {
                    attempts++;

                    // we can use some port number instead of 100 to see the error message
                    // and attempts to connect to db:
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
