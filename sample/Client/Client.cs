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
        private QuicClient _client;
        private QuicStream _stream;
        private string _username;

        public Client(string username)
        {
            _username = username;
        }

        public void Start()
        {
            _client = new QuicClient();

            try
            {
                _client.Connect("127.0.0.1", 8880);

                _stream = _client.CreateStream();
                Thread t = new Thread(new ThreadStart(ReceiveMessage));

                t.Start();

                while (true)
                {
                    Console.Write("# Your message: ");
                    string input = Console.ReadLine();
                    string str = _username + ": " + input;
                    int currentLineCursor = Console.CursorTop - 1;
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(0, currentLineCursor);
                    Console.WriteLine("You: " + input);

                    byte[] b = System.Text.Encoding.UTF8.GetBytes(str);
                    _stream.Write(b, 0, b.Length);
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
            while (true)
            {
                byte[] msg = _stream.Read();

                int leftCursor = Console.CursorLeft;
                int currentLineCursor = Console.CursorTop;
                Console.MoveBufferArea(0, currentLineCursor, Console.WindowWidth, 1, 0, currentLineCursor + 1);

                Console.SetCursorPosition(0, currentLineCursor);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, currentLineCursor);

                Console.WriteLine(System.Text.Encoding.UTF8.GetString(msg));
                Console.SetCursorPosition(leftCursor, currentLineCursor + 1);
            }
        }
    }
}
