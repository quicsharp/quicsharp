using System;
using System.IO;
using System.Threading;

namespace quicsharp
{
    // Used to easily output the lgos
    public class Logger
    {
        public static StreamWriter StreamOutput = new StreamWriter(Console.OpenStandardOutput());
        public static Mutex LogMutex = new Mutex();
        public static void Write(string log)
        {
            LogMutex.WaitOne();
            StreamOutput.WriteLine($"[LOG] {DateTime.Now.ToLongTimeString()}: {log}");
            LogMutex.ReleaseMutex();
        }
    }
}
