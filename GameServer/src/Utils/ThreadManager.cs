using System;
using System.Collections.Generic;

public class ThreadManager {
    private static readonly List<Action> executeOnMainThread = new List<Action>();
    private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
    private static bool actionToExecuteOnMainThread = false;

    public static void ExecuteOnMainThread(Action action) {
        if(action == null) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("No action to execute on main thread! Exiting...");
            Console.ForegroundColor = ConsoleColor.White;
            return;
        }

        lock(executeOnMainThread) {
            executeOnMainThread.Add(action);
            actionToExecuteOnMainThread = true;
        }
    }

    public static void UpdateMain() {
        if(!actionToExecuteOnMainThread) {
            return;
        }
        
        executeCopiedOnMainThread.Clear();
        lock(executeOnMainThread) {
            executeCopiedOnMainThread.AddRange(executeOnMainThread);
            executeOnMainThread.Clear();
            actionToExecuteOnMainThread = false;
        }

        foreach(var action in executeCopiedOnMainThread) {
            action();
        }
    }
}
