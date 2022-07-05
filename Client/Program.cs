using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatService
{
    class Program
    {
        //
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
                Console.Write("Enter message: ");
                // wait a message from client
                string messageToSend = Console.ReadLine();

                byte[] bufferToSend = Encoding.ASCII.GetBytes(messageToSend);
                _clientSocket.Send(bufferToSend);

                // receivedBuf: storage for received data
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

                Console.WriteLine("The text sent from me (client): " + receivedDataAsText);

                if (receivedDataAsText == "\n \nWARNING! Don't send multiple messages in one second. " +
                    "You have been warned for the first and last time.\n")
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
                    _clientSocket.Connect(IPAddress.Loopback, 104);
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
