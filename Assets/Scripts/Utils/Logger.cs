using UnityEngine;

class Logger {
    public static void Log(string id, string msg) {
        Debug.Log($"[{id.ToUpper()}] {msg}");
    }

    public static void Info(string id, string msg) {
        Debug.Log($"[{id.ToUpper()}] {msg}");
    }

    public static void Warning(string id, string msg) {
        Debug.LogWarning($"[{id.ToUpper()}] {msg}");
    }

    public static void Error(string id, string msg) {
        Debug.LogError($"[{id.ToUpper()}] {msg}");
    }
}