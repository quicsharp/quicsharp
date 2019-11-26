using System;
using System.IO;
using System.Threading;

namespace quicsharp
{
    // Used to easily output the lgos
    internal class Logger
    {
        public static String LogFile = "log_" + DateTime.Now.ToFileTime() + ".txt";
        public static bool LogToStdout = false;
        public static Mutex LogMutex = new Mutex();
        public static void Write(string log)
        {
            LogMutex.WaitOne();

            if (!LogToStdout)
            {
                using (StreamWriter sw = File.AppendText(LogFile))
                {
                    sw.WriteLine($"[LOG] {DateTime.Now.ToLongTimeString()}: {log}");
                }
            }
            else
            {
                Console.WriteLine($"[LOG] {DateTime.Now.ToLongTimeString()}: {log}");
            }
            LogMutex.ReleaseMutex();
        }
    }
}
