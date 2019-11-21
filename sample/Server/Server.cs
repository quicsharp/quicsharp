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

                foreach (QuicConnection connection in server.getConnectionPool().GetPool())
                {
                    // read and handle system messages
                    Console.WriteLine("New loop iteration");
                    try
                    {
                        byte[] newSystemmessage = connection.GetStreamOrCreate(0).Read();
                        HandleSystemMessages(newSystemmessage);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                
            }

        }

        static public void HandleSystemMessages(byte[] message)
        {
            Console.WriteLine("Received Message :  " + ASCIIEncoding.UTF8.GetString(message));
        }
    }
}
