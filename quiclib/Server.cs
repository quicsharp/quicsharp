using System;
using System.Collections.Generic;
using System.Net;
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
                foreach (QuicConnection connection in server.getConnectionPool().GetPool())
                {
                    // read and handle system messages
                    try
                    {
                        byte[] newSystemmessage = connection.GetStreamOrCreate(0).Read();
                        HandleSystemMessages(newSystemmessage, connection.Endpoint, server);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

            }

        }

        static public void HandleSystemMessages(byte[] message, IPEndPoint sender, QuicListener server)
        {
            Console.WriteLine("Received Message :  " + ASCIIEncoding.UTF8.GetString(message));

            foreach (QuicConnection connection in server.getConnectionPool().GetPool())
            {
                if (sender != connection.Endpoint)
                {
                    connection.GetStreamOrCreate(0).Write(message, 0, message.Length);
                }
            }
        }
    }
}
