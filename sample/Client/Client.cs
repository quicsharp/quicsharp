using quicsharp;
using System;
using System.Net.NetworkInformation;

namespace Client
{
    class Client
    {
        static void Main()
        {
            Console.Write("Before connecting, what will be your username? -> ");
            string username = Console.ReadLine();

            while (username.Length > 32 || username.Length == 0)
            {
                Console.Write("The username must be less than 32 bytes. Try again -> ");
                username = Console.ReadLine();
            }

            QuicClient client = new QuicClient();

            try
            {
                client.Connect("127.0.0.1", 8880);

                QuicStream qc = client.CreateStream();
                while (true)
                {
                    Console.Write("Your message: ");
                    string str = username + ": " + Console.ReadLine();
                    byte[] b = System.Text.Encoding.UTF8.GetBytes(str);
                    qc.Write(b, 0, b.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't connect to the server port {port}");
                Console.WriteLine(e.Message);
            }
        }
    }
}
