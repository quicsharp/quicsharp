using quicsharp;
using System;
using System.Threading;

namespace Client
{
    class ClientChat
    {
        static void Main()
        {
            Console.Write("Before connecting, what will be your username? -> ");
            string username = Console.ReadLine();

            // Simulate 10% packet loss
            QuicConnection.PacketLossPercentage = 10;

            while (username.Length > 32 || username.Length == 0)
            {
                Console.Write("The username must be less than 32 bytes. Try again -> ");
                username = Console.ReadLine();
            }

            Client client = new Client(username);
            client.Start();
        }
    }

    class Client
    {
        private QuicClient client_;
        private QuicStream stream_;
        private string username_;

        public Client(string username)
        {
            username_ = username;
        }

        public void Start()
        {
            client_ = new QuicClient();
            Thread t = new Thread(new ThreadStart(ReceiveMessage));

            t.Start();

            try
            {
                client_.Connect("127.0.0.1", 8880);

                stream_ = client_.CreateStream();
                while (true)
                {
                    Console.Write("# Your message: ");
                    string input = Console.ReadLine();
                    string str = username_ + ": " + input;
                    int currentLineCursor = Console.CursorTop - 1;
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, currentLineCursor);
                    Console.WriteLine("You: " + input);

                    byte[] b = System.Text.Encoding.UTF8.GetBytes(str);
                    stream_.Write(b, 0, b.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't connect to the server port {port}");
                Console.WriteLine(e.Message);
            }
        }

        public void ReceiveMessage()
        {
            byte[] buffer = new byte[512];

            Console.WriteLine("Listening to messages...");

            while (true)
            {
                Thread.Sleep(10000);
                Console.SetCursorPosition(0, Console.CursorTop);

                int currentLineCursor = Console.CursorTop;
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, currentLineCursor);

                Console.WriteLine("--- New Message ---");
                Console.Write("# Your message: ");
            }
        }
    }
}
