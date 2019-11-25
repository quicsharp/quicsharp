using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using quicsharp;
using Server;

namespace quicsharp.sample
{
    class Server
    {
        static void Main()
        {
            int port = 8880;

            // Simulate 10% packet loss
            QuicConnection.PacketLossPercentage = 10;
            QuicListener server = new QuicListener(port);

            Chatroom chatroom = new Chatroom();

            List<Task> tasks = new List<Task>();

            Console.WriteLine("Server listening on port : {0}", port);

            server.Start();

            while (true)
            {
                foreach (QuicConnection connection in server.getConnectionPool().GetPool())
                {
                    if(!chatroom.containsConnection(connection)){
                        chatroom.addConnection(connection);
                        tasks.Add(Task.Run(() => ProcessMessagesFromConnection(connection, server)));
                    }              
                }
            }
        }

        static public void ProcessMessagesFromConnection(QuicConnection connection, QuicListener server)
        {
            while (true)
            {
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
