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

        }
    }
}
