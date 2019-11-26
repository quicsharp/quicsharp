using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
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

            // Print the logs in the console
            Logger.StreamOutput = new StreamWriter(Console.OpenStandardOutput());
            Logger.StreamOutput.AutoFlush = true;

            // Simulate 10% packet loss
            QuicConnection.PacketLossPercentage = 10;
            QuicListener server = new QuicListener(port);

            // We only use one chatroom in this sample, but multiple could be instantiated
            Chatroom chatroom = new Chatroom();

            Dictionary<QuicConnection, Thread> threads = new Dictionary<QuicConnection, Thread>();

            Console.WriteLine("Server listening on port : {0}", port);

            server.Start();

            while (true)
            {
                foreach (QuicConnection connection in server.getConnectionPool().GetPool())
                {
                    if (!chatroom.containsConnection(connection))
                    {
                        // Every new connection is added to the chatroom and a new listening thread is created
                        chatroom.addConnection(connection);
                        Thread t = new Thread(new ThreadStart(() => ProcessMessagesFromConnection(connection, chatroom)));
                        t.Start();
                        threads.Add(connection, t);
                    }
                }

                foreach (QuicConnection connection in chatroom.Connections)
                {
                    if (!server.getConnectionPool().GetPool().Contains(connection))
                    {
                        // Whenever a connection is closed by the client, we need to remove it from the chatroom and close the corresponding thread
                        chatroom.removeConnection(connection);
                        threads[connection].Abort();
                        threads.Remove(connection);
                    }
                }
            }
        }

        /// <summary>
        /// Infinite loop to process messages from the first stream of a connection.
        /// The Read method being blocking, this method needs to be processed in another thread in order to receive and process messages from multiple sources
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="server"></param>
        static private void ProcessMessagesFromConnection(QuicConnection connection, Chatroom chatroom)
        {
            while (true)
            {
                try
                {
                    byte[] newSystemmessage = connection.GetStreamOrCreate(0).Read();
                    HandleSystemMessages(newSystemmessage, connection.Endpoint, chatroom);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// Decodes string from message and logs it to the system
        /// Broadcasts it to all users in the chatroom
        /// </summary>
        /// <param name="message">byte array representing the raw data from the stream frame</param>
        /// <param name="sender"></param>
        /// <param name="server"></param>
        static private void HandleSystemMessages(byte[] message, IPEndPoint sender, Chatroom chatroom)
        {
            Console.WriteLine("Received Message :  " + ASCIIEncoding.UTF8.GetString(message));

            foreach (QuicConnection connection in chatroom.Connections)
            {
                if (sender != connection.Endpoint)
                {
                    connection.GetStreamOrCreate(0).Write(message, 0, message.Length);
                }
            }
        }
    }
}
