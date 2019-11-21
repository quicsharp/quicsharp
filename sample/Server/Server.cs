using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using quicsharp;

namespace quicsharp.sample
{
    class Server
    {
        static void Main()
        {
            int port = 8880;

            // Simualte 10% packet loss
            QuicConnection.PacketLossPercentage = 10;
            QuicListener server = new QuicListener(port);

            Console.WriteLine("Server listening on port : {0}", port);

            server.Start();

            while (true)
            {
                server.Receive();
            }

        }
    }
}
