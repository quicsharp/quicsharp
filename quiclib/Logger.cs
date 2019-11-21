using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace quicsharp
{
    // Used to easily output the lgos
    internal class Logger
    {
        public static void Write(string log)
        {
            // Clear the actual line
            Console.SetCursorPosition(0, Console.CursorTop);
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);

            Console.WriteLine($"[LOG] {DateTime.Now.ToLongTimeString()}: {log}");
        }
    }
}
