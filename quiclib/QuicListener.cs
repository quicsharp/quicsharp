using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using quicsharp.Frames;

namespace quicsharp
{
    /// <summary>
    /// Main class for a Quic Server.
    /// </summary>
    public class QuicListener
    {
        private UdpClient server_;
        private bool started_;

        private static UInt32 idCounter_ = 0;
        private byte[] id_;

        public int Port { get; private set; }

        private ConnectionPool connectionPool_;

        public QuicListener(int port)
        {
            started_ = false;
            Port = port;
            connectionPool_ = new ConnectionPool();
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

            IPEndPoint client = null;

            try
            {
                // Listening
                Packet packet = Packet.Unpack(server_.Receive(ref client));

                if (packet is InitialPacket)
                {
                    HandleInitialPacket(packet as InitialPacket, client);
                }
                else
                {
                    packet = packet as LongHeaderPacket;
                    // The available connection pool for new client connections currently ranges from 4096 to 2**24
                    if ((packet as LongHeaderPacket).SCIDLength > 24)
                        throw new IndexOutOfRangeException("SCID should only be encoded on 3 bytes so far");
                    uint scid = BitConverter.ToUInt32((packet as LongHeaderPacket).SCID, 0);

                    QuicConnection connection = connectionPool_.Find(scid);
                    Console.WriteLine($"Received Packet from connectionID {scid} with the following frames :");
                    connection.ReadPacket(packet);
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

        private void HandleInitialPacket(InitialPacket packet, IPEndPoint client)
        {
            InitialPacket initPack = packet as InitialPacket;
            Console.WriteLine("New initial packet");
            initPack.DecodeFrames();
            Console.WriteLine("Data received {0}:{1}.", client.Address, client.Port);

            QuicClientConnection qc = new QuicClientConnection(new UdpClient(), client, new byte[0], id_);
            byte[] dcid = connectionPool_.AddConnection(qc);
            qc.SetDCID(dcid);

            InitialPacket initialPacket = new InitialPacket(dcid, id_, 0);
            initialPacket.AddFrame(new PaddingFrame());
            byte[] b = initialPacket.Encode();
            server_.Send(b, b.Length, client);
        }
    }
}
