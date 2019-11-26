using quicsharp.Frames;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace quicsharp
{
    /// <summary>
    /// Used to handle information about a connection endpoint.
    /// Handle the packets sent and received by a specific endpoint.
    /// </summary>
    public class QuicConnection
    {
        // Debug variable. Set it from 0 to 100
        // Simulate packet loss by not sending the packet.
        static public int PacketLossPercentage = 0;

        public IPEndPoint Endpoint { get; private set; }
        private PacketManager _packetManager;
        private Dictionary<UInt64, QuicStream> _streams;
        private Packet _currentPacket;
        private UdpClient _socket;
        private UInt64 _lastStreamId;
        private List<UInt32> _received = new List<UInt32>();

        // Background task to send packets again
        private Task _resendTask;
        private CancellationTokenSource _resendToken;

        // Delay, in milliseconds, at the end of which a packet is considered lost and to be sent again
        static private int _ackDelay = 1000;

        /// <summary>
        /// Create the QUIC connection information
        /// </summary>
        /// <param name="socket">The related UDP socket</param>
        /// <param name="endPoint">The endpoint to save</param>
        /// <param name="connID">DCID of incoming packets, SCID of outgoing packets</param>
        /// <param name="peerID">DCID of outgoing packets, SCID of incoming packets</param>
        public QuicConnection(UdpClient socket, IPEndPoint endPoint, byte[] connID, byte[] peerID)
        {
            _socket = socket;
            Endpoint = endPoint;
            _streams = new Dictionary<UInt64, QuicStream>();
            _packetManager = new PacketManager(connID, peerID);
            _lastStreamId = 0;
            _resendToken = new CancellationTokenSource();
            _resendTask = Task.Run(() => ResendNonAckPackets(), _resendToken.Token);
        }

        ~QuicConnection()
        {
            _resendToken.Cancel();
        }

        /// <summary>
        /// Creata a new stream (TODO: streams are always bidirectionnal for now)
        /// </summary>
        /// <param name="type">Type of the stream (unidirectional, bidirectional)</param>
        /// <returns>The new stream</returns>
        public QuicStream CreateStream(byte type)
        {
            QuicStream stream = new QuicStream(this, new VariableLengthInteger(_lastStreamId), type);
            _streams.Add(_lastStreamId, stream);
            _lastStreamId++;

            return stream;
        }

        /// <summary>
        /// Read a received packet and process its frames.
        /// </summary>
        /// <param name="packet">The received packet</param>
        public void ReadPacket(Packet packet)
        {
            // Process every new packet
            if (!_packetManager.IsPacketOld(packet))
            {
                packet.DecodeFrames();


                foreach (Frame frame in packet.Frames)
                {
                    if (frame is StreamFrame)
                    {
                        StreamFrame sf = frame as StreamFrame;
                        Logger.Write($"Received StreamFrame in packet number {packet.PacketNumber} with message: {System.Text.Encoding.UTF8.GetString(sf.Data)}");
                        QuicStream stream;
                        try
                        {
                            stream = GetStream(sf._streamID.Value);
                        }
                        catch (ArgumentException e)
                        {
                            stream = CreateStream(0x00);
                        }
                        stream.AddFrameToRead(sf);
                    }
                    if (frame is AckFrame)
                    {
                        AckFrame af = frame as AckFrame;
                        Logger.Write($"Received AckFrame in packet #{packet.PacketNumber}");
                        _packetManager.ProcessAckFrame(af);
                    }
                }

                // Store received PacketNumber for further implementation of acknowledgement procedure
                _received.Add(packet.PacketNumber);
            }
            else
            {
                // The packet was sent again so we send another ack for it
                packet.IsAckEliciting = true;
            }

            // Generate a new Ack Frame and send it directly
            // Even if the packet is old, we send a new ack for this ; the ack packet may not have been received
            if (packet.IsAckEliciting)
            {
                AckFrame ack = new AckFrame(new List<UInt32>() { packet.PacketNumber }, 100);
                AddFrame(ack);
                SendCurrentPacket();
                Logger.Write($"Acked packet #{packet.PacketNumber}");
            }
        }

        /// <summary>
        /// Background task to resend packets that have not been ack yet.
        /// </summary>
        public void ResendNonAckPackets()
        {
            Random rnd = new Random();

            while (!_resendToken.IsCancellationRequested)
            {
                // Wait for the packet manager to receive the packet
                Thread.Sleep(_ackDelay);
                _packetManager.HistoryMutex.WaitOne();
                // Send every packet not ack with a packet number lower than the highest packet number acknowledgded by the AckFrame
                foreach (KeyValuePair<UInt32, Packet> packet in _packetManager.History)
                {
                    byte[] data = packet.Value.Encode();

                    Logger.Write($"Packet #{packet.Key} sent again as it was not acknowledged in time");

                    // Simulate packet loss
                    if (rnd.Next(100) > PacketLossPercentage)
                        _socket.Send(data, data.Length, Endpoint);
                    else
                        Logger.Write($"Packet #{packet.Key} not sent because of simulated packet loss");
                }
                _packetManager.HistoryMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Send a packet to this endpoint
        /// </summary>
        /// <param name="packet">The packet to send</param>
        /// <returns>Number of byte sent</returns>
        public int SendPacket(Packet packet)
        {
            Random rnd = new Random();
            _packetManager.PreparePacket(packet);

            byte[] data = packet.Encode();

            int sent = 0;
            if (rnd.Next(100) > PacketLossPercentage)
            {
                sent = _socket.Send(data, data.Length, Endpoint);
            }
            else
            {
                Logger.Write($"Packet #{packet.PacketNumber} not sent because of simulated packet loss");
            }
            // If some bytes were sent
            return sent;
        }

        /// <summary>
        /// Add frame to the current packet. This current packet will be sent when the QuicConnection decides so.
        /// For the moment the current packet is always a ShortHeaderPacket, and it is sent after one frame is added.
        /// </summary>
        /// <param name="frame">The frame to add</param>
        public void AddFrame(Frame frame)
        {
            if (_socket == null || Endpoint == null)
                throw new NullReferenceException();
            // TODO: only ShortHeaderPacket for now
            if (_currentPacket == null)
                // TO CHECK: use 0-RTT packet to send non encrypted data
                _currentPacket = new RTTPacket();

            _currentPacket.AddFrame(frame);
        }

        /// <summary>
        /// Send the packet in construction
        /// </summary>
        /// <returns>Number of byte sent</returns>
        public int SendCurrentPacket()
        {
            if (_currentPacket == null)
                throw new CorruptedPacketException();
            int sentBytes = SendPacket(_currentPacket);

            _currentPacket = null;

            return sentBytes;
        }

        /// <summary>
        /// Get a specific stream
        /// </summary>
        /// <param name="id">The id of the stream wanted</param>
        /// <returns>The wanted stream</returns>
        public QuicStream GetStream(UInt64 id)
        {
            if (!_streams.ContainsKey(id))
                throw new ArgumentException($"The Quic Stream id {id} does not exist");

            return _streams[id];
        }

        /// <summary>
        /// Return the List of QuicStreams opened by the connection
        /// </summary>
        /// <returns></returns>
        public List<QuicStream> GetStreams()
        {
            List<QuicStream> list = new List<QuicStream>();
            foreach (KeyValuePair<ulong, QuicStream> item in _streams)
            {
                list.Add(item.Value);
            }
            return list;
        }

        /// <summary>
        /// Get a specific stream, creates it if it does not exist yet
        /// </summary>
        /// <param name="id">The id of the stream wanted</param>
        /// <returns></returns>
        public QuicStream GetStreamOrCreate(ulong id)
        {
            while (!_streams.ContainsKey(id))
            {
                CreateStream(0x00);
            }
            return _streams[id];
        }
    }
}
