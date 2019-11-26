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
        private int _port;
        private UdpClient _server;
        private bool _started;
        private ConnectionPool _connectionPool;
        private Task _receiveTask;
        private CancellationTokenSource _receiveToken;


        /// <summary>
        /// Create a server on a specific port
        /// </summary>
        /// <param name="port">The port to listen to</param>
        public QuicListener(int port)
        {
            _started = false;
            _port = port;
            _connectionPool = new ConnectionPool();
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
            _server = new UdpClient(_port);
            _started = true;

            // Background task to receive packets from the remote server
            _receiveToken = new CancellationTokenSource();
            _receiveTask = Task.Run(() => Receive(), _receiveToken.Token);
        }

        /// <summary>
        /// Close the listener
        /// </summary>
        public void Close()
        {
            _server.Close();
            _receiveToken.Cancel();
        }

        /// <summary>
        /// Receive and manage the different packet reveice from all the client connections.
        /// </summary>
        public void Receive()
        {
            if (!_started)
                throw new InvalidOperationException("QuicListener is not started but is waiting for packets");

            IPEndPoint client = null;

            while (!_receiveToken.IsCancellationRequested)
            {
                // Listening
                Packet packet = Packet.Unpack(_server.Receive(ref client));
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
                        byte[] DCID = (packet as LongHeaderPacket).DCID;
                        Logger.Write($"Received packet with DCID {BitConverter.ToString(DCID)}");

                        QuicConnection connection = _connectionPool.Find(DCID);
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
            } while (_connectionPool.Find(connID) != null);

            QuicConnection qc = new QuicConnection(_server, client, connID, incomingPacket.SCID);
            _connectionPool.AddConnection(qc, connID);

            InitialPacket responsePacket = new InitialPacket(incomingPacket.SCID, connID, 0);
            responsePacket.AddFrame(new PaddingFrame());
            byte[] b = responsePacket.Encode();
            _server.Send(b, b.Length, client);
            Logger.Write($"Connection established. This is server {BitConverter.ToString(connID)} connected to client {BitConverter.ToString(incomingPacket.SCID)}");
        }

        /// <summary>
        /// Returns the ConnectionPool object
        /// </summary>
        /// <returns></returns>
        public ConnectionPool getConnectionPool()
        {
            return _connectionPool;
        }
    }
}
