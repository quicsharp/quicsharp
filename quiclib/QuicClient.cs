using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;

using System.Threading.Tasks;
using System.Collections.Generic;

namespace quicsharp
{
    /// <summary>
    /// Main class of the API used like the TCPClient. Can start and manage a QUIC connection.
    /// </summary>
    public class QuicClient
    {
        private List<Frame> _awaitingFrames = new List<Frame>();

        private static Mutex _mutex = new Mutex();
        private UdpClient _client;
        private UInt32 _packetNumber;

        // Connection to the QUIC server
        private QuicConnection _connection;

        // Tasks to receive the quic packets in background
        private Task _receiveTask;
        private CancellationTokenSource _receiveToken;

        public QuicClient()
        {
            _client = new UdpClient();
            _packetNumber = 0;
        }

        ~QuicClient()
        {
            Close();
        }

        /// <summary>
        /// Connect to a remote server.
        /// </summary>
        /// <param name="ip">Ip of the remote server</param>
        /// <param name="port">Port of the remote server</param>
        public void Connect(string ip, int port)
        {
            // Create random DCID and SCID
            byte[] DCID = new byte[8];
            byte[] SCID = new byte[8];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(DCID);
            rng.GetBytes(SCID);

            // Create and send an InitialPacket to open a connection with the remote server
            InitialPacket initialPacket = new InitialPacket(DCID, SCID, _packetNumber++);

            byte[] byteInitialPacket = initialPacket.Encode();
            _client.Send(byteInitialPacket, byteInitialPacket.Length, ip, port);

            IPEndPoint server = null;
            Packet packet = Packet.Unpack(_client.Receive(ref server));

            // Start the connection with an InitialPacket
            if (packet.GetType() == typeof(InitialPacket))
            {
                packet.DecodeFrames();
                Logger.Write($"Data received from server {server.Address}:{server.Port}");

                InitialPacket initPack = packet as InitialPacket;
                Logger.Write($"Connection established. This is client {BitConverter.ToString(initPack.DCID)} connected to server {BitConverter.ToString(initPack.SCID)}");
                _connection = new QuicConnection(_client, server, initPack.DCID, initPack.SCID);
            }

            // Background task to receive packets from the remote server
            _receiveToken = new CancellationTokenSource();
            _receiveTask = Task.Run(() => Receive(server), _receiveToken.Token);
        }

        /// <summary>
        /// Called in background to process the packets received through the QUIC connection
        /// </summary>
        /// <param name="endpoint">QUIC endpoint</param>
        private void Receive(IPEndPoint endpoint)
        {
            while (!_receiveToken.IsCancellationRequested)
            {
                Packet packet = Packet.Unpack(_client.Receive(ref endpoint));
                _connection.ReadPacket(packet);
                _mutex.WaitOne();
                _awaitingFrames.AddRange(packet.Frames);
                _mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Close a QUIC connection
        /// </summary>
        public void Close()
        {
            _client.Close();
            _receiveToken.Cancel();
        }

        /// <summary>
        /// Create a Stream
        /// TODO: Specifying if the stream is bidirectional or unidirectional
        /// </summary>
        /// <returns>The stream created</returns>
        public QuicStream CreateStream()
        {
            // TODO: choose a stream type
            return _connection.CreateStream(0);
        }
    }
}
