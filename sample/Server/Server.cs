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
            int port;
            bool isAvailable;
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            Random rnd = new Random();

            do
            {
                port = 8908;
                isAvailable = true;
                TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

                foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
                {
                    if (tcpi.LocalEndPoint.Port == port)
                    {
                        isAvailable = false;
                        break;
                    }
                }
            } while (!isAvailable);

            Console.WriteLine("First port available chosen : {0}", port);

            QuicListener server = new QuicListener(port);

            server.Start();

            while (true)
            {
                server.Receive();
            }

        }
    }
}
