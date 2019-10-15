using quicsharp;
using System;
using System.Net.NetworkInformation;

namespace Client
{
    class Client
    {
        static void Main()
        {
            QuicClient client = new QuicClient();

            client.Connect("127.0.0.1", 8908);

            QuicStream qc = client.CreateStream();
            while (true)
            {
                string str = "Me: " + Console.ReadLine();
                qc.Write(System.Text.Encoding.UTF8.GetBytes(str), 0, str.Length);
            }
        }
    }
}
