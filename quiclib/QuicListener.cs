using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Security.Cryptography;

using quicsharp.Frames;

namespace quicsharp
{
    /// <summary>
    /// Main class of the API used like the TCPListener. Can start and manage a QUIC connections between here and different clients.
    /// </summary>
    public class QuicListener
    {
        private UdpClient server_;
        private bool started_;

        public int Port { get; private set; }

        private ConnectionPool connectionPool_;

        /// <summary>
        /// Create a server on a specific port
        /// </summary>
        /// <param name="port">The port to listen to</param>
        public QuicListener(int port)
        {
            started_ = false;
            Port = port;
            connectionPool_ = new ConnectionPool();
        }

        /// <summary>
        /// Start the listener
        /// </summary>
        public void Start()
        {
            server_ = new UdpClient(Port);
            started_ = true;
        }

        /// <summary>
        /// Close the listener
        /// </summary>
        public void Close()
        {
            server_.Close();
        }

        /// <summary>
        /// Receive and manage the different packet reveice from all the client connections.
        /// </summary>
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
                    byte[] dcid = (packet as LongHeaderPacket).DCID;
                    Logger.Write($"Received packet with DCID {BitConverter.ToString(dcid)}");

                    QuicConnection connection = connectionPool_.Find(dcid);
                    if (connection == null)
                    {
                        Logger.Write($"No existing connection find for ID {BitConverter.ToString(dcid)}");
                    }
                    else
                    {
                        connection.ReadPacket(packet);
                    }
                }
            }
            catch (CorruptedPacketException e)
            {
                Logger.Write($"Received a corrupted QUIC packet {e.Message}");
            }
            catch (Exception e)
            {
                Logger.Write(e.Source);
                throw e;
            }
        }

        /// <summary>
        /// Handle an InitialPacket to create a new QuicClientConnection related to this packet.
        /// </summary>
        /// <param name="packet">The packet received</param>
        /// <param name="client">The client that sent the packet</param>
        private void HandleInitialPacket(InitialPacket packet, IPEndPoint client)
        {
            InitialPacket incomingPacket = packet as InitialPacket;
            incomingPacket.DecodeFrames();
            Logger.Write($"Data received from server {client.Address}:{client.Port}");

            // Create random connection ID and use it as SCID for server -> client communications
            // Make sure it's not already in use
            byte[] scid = new byte[8];
            do
            {
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                rng.GetBytes(scid);
            } while (connectionPool_.Find(scid) != null);

            QuicClientConnection qc = new QuicClientConnection(new UdpClient(), client, scid, incomingPacket.SCID);
            connectionPool_.AddConnection(qc, scid);

            InitialPacket responsePacket = new InitialPacket(incomingPacket.SCID, scid, 0);
            responsePacket.AddFrame(new PaddingFrame());
            byte[] b = responsePacket.Encode();
            server_.Send(b, b.Length, client);
        }
    }
}
