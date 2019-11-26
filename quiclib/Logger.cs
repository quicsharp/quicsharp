using System;
using System.IO;
using System.Threading;

namespace quicsharp
{
    // Used to easily output the lgos
    internal class Logger
    {
        public static StreamWriter StreamOutput = File.AppendText("log_" + DateTime.Now.ToFileTime() + ".txt");
        public static bool LogToStdout = false;
        public static Mutex LogMutex = new Mutex();
        public static void Write(string log)
        {
            LogMutex.WaitOne();

            if (!LogToStdout)
            {
                StreamOutput.AutoFlush = true;
                StreamOutput.WriteLine($"[LOG] {DateTime.Now.ToLongTimeString()}: {log}");
            }
            else
            {
                Console.WriteLine($"[LOG] {DateTime.Now.ToLongTimeString()}: {log}");
            }
            LogMutex.ReleaseMutex();
        }
    }
}
