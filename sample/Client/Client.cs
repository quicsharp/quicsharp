using quicsharp;
using System;
using System.IO;
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
            // Write the quicsharp logs in an external file
            Logger.StreamOutput = File.AppendText("log_" + DateTime.Now.ToFileTime() + ".txt");
            Logger.StreamOutput.AutoFlush = true;
        }

        public void Start()
        {
            client_ = new QuicClient();

            try
            {
                client_.Connect("127.0.0.1", 8880);

                stream_ = client_.CreateStream();
                Thread t = new Thread(new ThreadStart(ReceiveMessage));

                t.Start();

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
            while (true)
            {
                byte[] msg = stream_.Read();

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
