using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace quicsharp
{
    public class QuicListener
    {
        private UdpClient server_;
        private bool started_;

        public int Port { get; private set; }

        public QuicListener(int port)
        {
            started_ = false;
            Port = port;
        }

        // Starts the listener
        public void Start()
        {
            server_ = new UdpClient(Port);
            started_ = true;
        }

        // Close the listener
        public void Close()
        {
            server_.Close();
        }

        public void Receive()
        {
            while (true)
            {
                IPEndPoint client = null;

                // Listening
                byte[] data = server_.Receive(ref client);
                Console.WriteLine("Data received {0}:{1}.", client.Address, client.Port);

                // Printing message
                string message = Encoding.Default.GetString(data);
                Console.WriteLine("MESSAGE : {0}\n", message);
            }
        }
    }
}
