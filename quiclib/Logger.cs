using System;

namespace quicsharp
{
    // Used to easily output the lgos
    internal class Logger
    {
        public static void Write(string log)
        {
            // Clear the actual line
            int currentLineCursor = Console.CursorTop;
            int leftCursor = Console.CursorLeft;

            Console.MoveBufferArea(0, currentLineCursor, Console.WindowWidth, 1, 0, currentLineCursor + 1);

            Console.SetCursorPosition(0, currentLineCursor);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);

            Console.WriteLine($"[LOG] {DateTime.Now.ToLongTimeString()}: {log}");

            Console.SetCursorPosition(leftCursor, currentLineCursor + 1);
        }
    }
}
