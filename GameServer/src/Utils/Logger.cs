using System;

namespace GameServer {
    class Logger {
        public static void Log(string id, string msg) {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{id.ToUpper()}]".PadRight(15) + msg);
        }

        public static void Info(string id, string msg) {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[{id.ToUpper()}]".PadRight(15) + msg);
        }

        public static void Warning(string id, string msg) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{id.ToUpper()}]".PadRight(15) + msg);
        }

        public static void Error(string id, string msg) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{id.ToUpper()}]".PadRight(15) + msg);
        }
    }
}