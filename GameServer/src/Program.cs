using System;
using System.Threading;

namespace GameServer {
    class Program {
        private static bool _isRunning = true;

        static void Main(string[] args) {
            Console.Title = "Game Server";

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(50, 26950);
        }

        private static void MainThread() {
            Logger.Info("Main", "Main thread started.");
            Logger.Info("Main", $"Running server at {Constants.TICKS_PER_SECOND} ticks per second.");
            
            DateTime nextLoop = DateTime.Now;
            while(_isRunning) {
                while(nextLoop < DateTime.Now) {
                    GameLogic.Update();
                    nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);
                    if(nextLoop > DateTime.Now) Thread.Sleep(nextLoop - DateTime.Now);
                }
            }
        }
    }
}
