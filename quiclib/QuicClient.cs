using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using System.Threading;

using quicsharp.Frames;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace quicsharp
{
    /// <summary>
    /// Main class of the API used like the TCPClient. Can start and manage a QUIC connection.
    /// </summary>
    public class QuicClient
    {
        private UdpClient client_;
        private UInt32 packetNumber_;

        // Connection to the QUIC server
        private QuicConnectionWithServer serverConnection_;

        public bool Connected;

        public static Mutex mutex = new Mutex();

        public List<Frame> awaitingFrames = new List<Frame>();

        // Tasks to receive the quic packets in background
        private Task receiveTask_;
        private CancellationTokenSource receiveToken_;

        public QuicClient()
        {
            client_ = new UdpClient();
            packetNumber_ = 0;
            Connected = false;
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
            InitialPacket initialPacket = new InitialPacket(DCID, SCID, packetNumber_++);

            byte[] byteInitialPacket = initialPacket.Encode();
            client_.Send(byteInitialPacket, byteInitialPacket.Length, ip, port);

            IPEndPoint server = null;
            Packet packet = Packet.Unpack(client_.Receive(ref server));

            // Start the connection with an InitialPacket
            if (packet.GetType() == typeof(InitialPacket))
            {
                packet.DecodeFrames();
                Logger.Write($"Data received from server {server.Address}:{server.Port}");

                InitialPacket initPack = packet as InitialPacket;
                Logger.Write($"Connection established. This is client {BitConverter.ToString(initPack.DCID)} connected to server {BitConverter.ToString(initPack.SCID)}");
                serverConnection_ = new QuicConnectionWithServer(new UdpClient(), server, initPack.DCID, initPack.SCID, mutex);
                Connected = true;
            }

            // Background task to receive packets from the remote server
            receiveToken_ = new CancellationTokenSource();
            receiveTask_ = Task.Run(() => Receive(server), receiveToken_.Token);
        }

        /// <summary>
        /// Called in background to process the packets received through the QUIC connection
        /// </summary>
        /// <param name="endpoint">QUIC endpoint</param>
        private void Receive(IPEndPoint endpoint)
        {
            while (!receiveToken_.IsCancellationRequested)
            {
                Packet packet = Packet.Unpack(client_.Receive(ref endpoint));
                serverConnection_.ReadPacket(packet);
                mutex.WaitOne();
                awaitingFrames.AddRange(packet.Frames);
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Close a QUIC connection
        /// </summary>
        public void Close()
        {
            client_.Close();
            Connected = false;
            receiveToken_.Cancel();
        }

        /// <summary>
        /// Create a Stream
        /// TODO: Specifying if the stream is bidirectional or unidirectional
        /// </summary>
        /// <returns>The stream created</returns>
        public QuicStream CreateStream()
        {
            // TODO: choose a stream type
            return serverConnection_.CreateStream(0);
        }
    }
}
