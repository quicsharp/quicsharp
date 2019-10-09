﻿using System;
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
        private static UInt32 idCounter_ = 0;
        private UInt32 id_;

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
            id_ = idCounter_;
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

                    if (packet.GetType() == typeof(InitialPacket))
                    {
                        InitialPacket initPack = packet as InitialPacket;
                        Console.WriteLine("New initial packet");
                        initPack.DecodeFrames();
                        Console.WriteLine("Data received {0}:{1}.", client.Address, client.Port);

                        QuicConnection qc = new QuicConnection(client);
                        UInt32 dcid = ConnectionPool.AddConnection(qc);

                        InitialPacket initialPacket = new InitialPacket(dcid, id_, 0);
                        byte[] b = initialPacket.Encode();
                        server_.Send(b, b.Length, client);
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
    }
}
