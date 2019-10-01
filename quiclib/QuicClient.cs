﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace quicsharp
{
    public class QuicClient
    {
        private UdpClient client_;
        private UInt32 packetNumber_;

        public QuicClient()
        {
            client_ = new UdpClient();
            packetNumber_ = 0;
        }

        // Connect to a remote server.
        public void Connect(string ip, int port)
        {
            byte[] pack = { 0, 0, 1, 0, 0, 0, 0, 0, 0 };
            Array.Copy(Encoding.Default.GetBytes("Hello"), 0, pack, 4, 5);

            client_.Send(pack, pack.Length, ip, port);
        }

        public int Send(byte[] payload)
        {
            packetNumber_++;

            return 0;
        }

        public void Close()
        {
            client_.Close();
        }
    }
}
