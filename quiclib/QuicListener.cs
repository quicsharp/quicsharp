using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

using quicsharp.Frames;
using System.Threading.Tasks;
using System.Threading;

namespace quicsharp
{
    /// <summary>
    /// Main class of the API used like the TCPListener. Can start and manage a QUIC connections between here and different clients.
    /// </summary>
    public class QuicListener
    {
        private UdpClient server_;
        private bool started_;

        private static UInt32 idCounter_ = 0;
        private byte[] id_;

        public int Port { get; private set; }

        private ConnectionPool connectionPool_;

        private Task receiveTask_;
        private CancellationTokenSource receiveToken_;


        /// <summary>
        /// Create a server on a specific port
        /// </summary>
        /// <param name="port">The port to listen to</param>
        public QuicListener(int port)
        {
            started_ = false;
            Port = port;
            connectionPool_ = new ConnectionPool();
            Logger.LogToStdout = true;
        }

        ~QuicListener()
        {
            Close();
        }

        /// <summary>
        /// Start the listener
        /// </summary>
        public void Start()
        {
            server_ = new UdpClient(Port);
            started_ = true;
            id_ = BitConverter.GetBytes(idCounter_);
            idCounter_++;

            // Background task to receive packets from the remote server
            receiveToken_ = new CancellationTokenSource();
            receiveTask_ = Task.Run(() => Receive(), receiveToken_.Token);
        }

        /// <summary>
        /// Close the listener
        /// </summary>
        public void Close()
        {
            server_.Close();
            receiveToken_.Cancel();
        }

        /// <summary>
        /// Receive and manage the different packet reveice from all the client connections.
        /// </summary>
        public void Receive()
        {
            if (!started_)
                throw new InvalidOperationException("QuicListener is not started but is waiting for packets");

            IPEndPoint client = null;

            while (!receiveToken_.IsCancellationRequested)
            {
                // Listening
                Packet packet = Packet.Unpack(server_.Receive(ref client));
                Logger.Write($"Data received from server {client.Address}:{client.Port}");

                try
                {
                    // Listening
                    if (packet is InitialPacket)
                    {
                        HandleInitialPacket(packet as InitialPacket, client);
                    }
                    else
                    {
                        packet = packet as LongHeaderPacket;
                        byte[] DCID = (packet as LongHeaderPacket).DCID_;
                        Logger.Write($"Received packet with DCID {BitConverter.ToString(DCID)}");

                        QuicConnection connection = connectionPool_.Find(DCID);
                        if (connection == null)
                        {
                            Logger.Write($"No existing connection find for ID {BitConverter.ToString(DCID)}");
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
        }

        /// <summary>
        /// Handle an InitialPacket to create a new QuicConnection related to this packet.
        /// </summary>
        /// <param name="packet">The packet received</param>
        /// <param name="client">The client that sent the packet</param>
        private void HandleInitialPacket(InitialPacket packet, IPEndPoint client)
        {
            InitialPacket incomingPacket = packet as InitialPacket;
            incomingPacket.DecodeFrames();

            // Create random connection ID and use it as SCID for server -> client communications
            // Make sure it's not already in use
            byte[] connID = new byte[8];
            do
            {
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                rng.GetBytes(connID);
            } while (connectionPool_.Find(connID) != null);

            QuicConnection qc = new QuicConnection(server_, client, connID, incomingPacket.SCID_);
            connectionPool_.AddConnection(qc, connID);

            InitialPacket responsePacket = new InitialPacket(incomingPacket.SCID_, connID, 0);
            responsePacket.AddFrame(new PaddingFrame());
            byte[] b = responsePacket.Encode();
            server_.Send(b, b.Length, client);
            Logger.Write($"Connection established. This is server {BitConverter.ToString(connID)} connected to client {BitConverter.ToString(incomingPacket.SCID_)}");
        }

        /// <summary>
        /// Returns the ConnectionPool object
        /// </summary>
        /// <returns></returns>
        public ConnectionPool getConnectionPool()
        {
            return connectionPool_;
        }
    }
}
