using System;
using System.Collections.Generic;
using UnityEngine;

public class ThreadManager : MonoBehaviour {
    private static readonly List<Action> executeOnMainThread = new List<Action>();
    private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
    private static bool actionToExecuteOnMainThread = false;

    private void Update() {
        UpdateMain();
    }

    public static void ExecuteOnMainThread(Action action) {
        if(action == null) {
            Logger.Log("Threads", "No action to execute on main thread! Exiting...");
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
