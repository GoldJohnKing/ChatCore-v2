using System;

namespace OpenBLive.Runtime.Utilities
{
    public static class Logger
    {
        public static void LogError(string logInfo)
        {
            Console.WriteLine(logInfo);
        }

        public static void Log(string logInfo)
        {
            Console.WriteLine(logInfo);
        }
    }
}