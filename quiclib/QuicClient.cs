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
    public class QuicClient
    {
        private UdpClient client_;
        private UInt32 packetNumber_;

        // Connection to the QUIC server
        private QuicServerConnection serverConnection_;

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

        // Connect to a remote server.
        public void Connect(string ip, int port)
        {
            // Create random DCID and SCID
            byte[] DCID = new byte[8];
            byte[] SCID = new byte[8];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(DCID);
            rng.GetBytes(SCID);

            InitialPacket initialPacket = new InitialPacket(DCID, SCID, packetNumber_++);

            byte[] byteInitialPacket = initialPacket.Encode();
            client_.Send(byteInitialPacket, byteInitialPacket.Length, ip, port);

            IPEndPoint server = null;
            Packet packet = Packet.Unpack(client_.Receive(ref server));

            if (packet.GetType() == typeof(InitialPacket))
            {
                Logger.Write("New initial packet created (Client Side)");
                packet.DecodeFrames();
                Logger.Write($"Data received from server {server.Address}:{server.Port}");

                InitialPacket initPack = packet as InitialPacket;
                Logger.Write($"Connection started. I am client n {BitConverter.ToUInt32(initPack.DCID, 0)} connected to server n {BitConverter.ToUInt32(initPack.SCID, 0)}");
                serverConnection_ = new QuicServerConnection(new UdpClient(), server, initPack.DCID, initPack.SCID, mutex);
                Connected = true;
            }
            receiveToken_ = new CancellationTokenSource();
            receiveTask_ = Task.Run(() => Receive(server), receiveToken_.Token);
        }
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


        private int Send(byte[] payload)
        {
            if (!Connected)
                return -1;
            packetNumber_++;

            ShortHeaderPacket packet = new ShortHeaderPacket();
            packet.AddFrame(new DebugFrame { Message = payload.ToString() });

            return serverConnection_.SendPacket(packet);
        }

        private int Send(Packet packet)
        {
            return serverConnection_.SendPacket(packet);
        }

        public void Close()
        {
            client_.Close();
            Connected = false;
            receiveToken_.Cancel();
        }

        // Create a Stream
        // TODO: Specifying if the stream is bidirectional or unidirectional
        public QuicStream CreateStream()
        {
            // TODO: choose a stream type
            return serverConnection_.CreateStream(0);
        }
    }
}
