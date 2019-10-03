using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using quicsharp.Frames;

namespace quicsharp
{
    public class QuicListener
    {
        private UdpClient server_;
        private bool started_;

        private ServerConnection serverConnection_;

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
            //serverConnection_ = new ServerConnection(server_, endpoint);
            started_ = true;
        }

        // Close the listener
        public void Close()
        {
            server_.Close();
        }

        public void Receive()
        {
            if (!started_)
                throw new InvalidOperationException("QuicListener is not started but is waiting for packets");

            while (true)
            {
                IPEndPoint client = null;

                // Listening
                Packet packet = Packet.Unpack(server_.Receive(ref client));
                packet.DecodeFrames();
                Console.WriteLine("Data received {0}:{1}.", client.Address, client.Port);

                QuicConnection qc = ConnectionPool.Find(packet.ClientId);

                if (qc == null)
                {
                    Console.WriteLine("New connection");
                    ConnectionPool.AddConnection(qc);
                }

                // Printing message
                foreach (Frame f in packet.Frames)
                {
                    if (f.Type == 0x1e)
                    {
                        DebugFrame fd = f as DebugFrame;
                        Console.WriteLine("Debug message : {0}\n", fd.Message);
                    }
                }
            }
        }
    }
}
