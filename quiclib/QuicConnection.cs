﻿using quicsharp.Frames;
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
        private IPEndPoint endpoint_;
        private UdpClient socket_;
        private UInt64 lastStreamId_;
        public byte[] scid_;
        public byte[] dcid_;

        protected PacketManager packetManager_;
        // TODO : split awaiting frames by packet space
        protected Queue<Frame> awaitingFrames_;
        public List<UInt32> Received = new List<UInt32>();
        protected Dictionary<UInt64, QuicStream> streams_;

        protected Packet currentPacket_;

        // Background task to send packets again
        private Task resendTask_;
        private CancellationTokenSource resendToken_;

        // Debug variable. Set it from 0 to 100
        // Simulate packet loss by not sending the packet.
        static public int PacketLossPercentage = 0;

        /// <summary>
        /// Create the QUIC connection information. Use the QuicConnectionWithClient or QuicConnectionWithServer
        /// to correctly initiate the Source ID and the Destination ID.
        /// </summary>
        /// <param name="socket">The related UDP socket</param>
        /// <param name="endPoint">The endpoint to save</param>
        /// <param name="connID">DCID of incoming packets, SCID of outgoing packets</param>
        /// <param name="peerID">DCID of outgoing packets, SCID of incoming packets</param>
        public QuicConnection(UdpClient socket, IPEndPoint endPoint, byte[] connID, byte[] peerID)
        {
            socket_ = socket;
            endpoint_ = endPoint;
            streams_ = new Dictionary<UInt64, QuicStream>();
            packetManager_ = new PacketManager(connID, peerID);
            lastStreamId_ = 0;
            resendToken_ = new CancellationTokenSource();
            resendTask_ = Task.Run(() => ResendNonAckPackets(), resendToken_.Token);
        }

        ~QuicConnection()
        {
            resendToken_.Cancel();
        }

        /// <summary>
        /// Creata a new stream (TODO: streams are always bidirectionnal for now)
        /// </summary>
        /// <param name="type">Type of the stream (unidirectional, bidirectional)</param>
        /// <returns>The new stream</returns>
        public QuicStream CreateStream(byte type)
        {
            lastStreamId_++;
            QuicStream stream = new QuicStream(this, new VariableLengthInteger(lastStreamId_), type);
            streams_.Add(lastStreamId_, stream);

            return stream;
        }

        /// <summary>
        /// Read a received packet and process its frames.
        /// </summary>
        /// <param name="packet">The received packet</param>
        public void ReadPacket(Packet packet)
        {
            // Process every new packet
            if (!packetManager_.IsPacketOld(packet))
            {
                packet.DecodeFrames();

                foreach (Frame frame in packet.Frames)
                {
                    if (frame is StreamFrame)
                    {
                        StreamFrame sf = frame as StreamFrame;
                        Logger.Write($"Received StreamFrame in packet #{packet.PacketNumber} with message: {System.Text.Encoding.UTF8.GetString(sf.Data)}");
                    }
                    if (frame is AckFrame)
                    {
                        AckFrame af = frame as AckFrame;
                        Logger.Write($"Received AckFrame in packet #{packet.PacketNumber}");
                        packetManager_.ProcessAckFrame(af);
                    }
                }

                // Store received PacketNumber for further implementation of acknowledgement procedure
                Received.Add(packet.PacketNumber);
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

            while (!resendToken_.IsCancellationRequested)
            {
                packetManager_.HistoryMutex.WaitOne();
                // Send every packet not ack with a packet number lower than the highest packet number acknowledgded by the AckFrame
                foreach (KeyValuePair<UInt32, Packet> packet in packetManager_.History)
                {
                    byte[] data = packet.Value.Encode();

                    Logger.Write($"Packet #{packet.Key} sent again");

                    // Simulate packet loss
                    if (rnd.Next(100) > PacketLossPercentage)
                        socket_.Send(data, data.Length, endpoint_);
                    else
                        Logger.Write($"Packet #{packet.Key} not sent because of simulated packet loss");
                }
                packetManager_.HistoryMutex.ReleaseMutex();

                Thread.Sleep(30);
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
            packetManager_.PreparePacket(packet);

            byte[] data = packet.Encode();

            int sent = 0;
            if (rnd.Next(100) > PacketLossPercentage)
            {
                sent = socket_.Send(data, data.Length, endpoint_);
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
            if (socket_ == null || endpoint_ == null)
                throw new NullReferenceException();
            // TODO: only ShortHeaderPacket for now
            if (currentPacket_ == null)
                // TO CHECK: use 0-RTT packet to send non encrypted data
                currentPacket_ = new RTTPacket();

            currentPacket_.AddFrame(frame);
        }

        /// <summary>
        /// Send the packet in construction
        /// </summary>
        /// <returns>Number of byte sent</returns>
        public int SendCurrentPacket()
        {
            if (currentPacket_ == null)
                throw new CorruptedPacketException();
            int sentBytes = SendPacket(currentPacket_);

            currentPacket_ = null;

            return sentBytes;
        }

        /// <summary>
        /// Get a specific stream
        /// </summary>
        /// <param name="id">The id of the stream wanted</param>
        /// <returns>The wanted stream</returns>
        public QuicStream GetStream(UInt64 id)
        {
            if (!streams_.ContainsKey(id))
                throw new ArgumentException($"The Quic Stream id {id} does not exist");

            return streams_[id];
        }
    }
}
