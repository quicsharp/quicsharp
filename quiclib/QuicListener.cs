using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using quicsharp.Frames;

namespace quicsharp
{
    public class QuicListener
    {
        private UdpClient server_;
        private bool started_;

        private static UInt32 idCounter_ = 0;
        private byte[] id_;

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
            id_ = BitConverter.GetBytes(idCounter_);
            idCounter_++;
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

                try
                {
                    // Listening
                    Packet packet = Packet.Unpack(server_.Receive(ref client));

                    if (packet is InitialPacket)
                    {
                        HandleInitialPacket(packet as InitialPacket, client);
                    }
                    else if (packet is ShortHeaderPacket)
                    {
                        // TEMP: Write every stream frame
                        packet.DecodeFrames();

                        foreach(Frame frame in packet.Frames)
                        {
                            if (frame is StreamFrame)
                            {
                                Console.WriteLine($"Received StreamFrame with message: {System.Text.Encoding.UTF8.GetString(frame.Content)}");
                            }
                        }
                    }
                }
                catch (CorruptedPacketException e)
                {
                    Console.WriteLine($"Received a corrupted QUIC packet {e.Message}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Source);
                    throw e;
                }
            }
        }

        private void HandleInitialPacket(InitialPacket packet, IPEndPoint client)
        {
            InitialPacket initPack = packet as InitialPacket;
            Console.WriteLine("New initial packet");
            initPack.DecodeFrames();
            Console.WriteLine("Data received {0}:{1}.", client.Address, client.Port);

            QuicClientConnection qc = new QuicClientConnection(new UdpClient(), client, new byte[0], id_);
            byte[] dcid = ConnectionPool.AddConnection(qc);
            qc.SetDCID(dcid);

            InitialPacket initialPacket = new InitialPacket(dcid, id_, 0);
            initialPacket.AddFrame(new PaddingFrame());
            byte[] b = initialPacket.Encode();
            server_.Send(b, b.Length, client);
        }
    }
}
