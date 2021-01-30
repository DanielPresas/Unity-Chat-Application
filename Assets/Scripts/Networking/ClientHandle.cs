using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour {
    public static void Welcome(Packet packet) {
        var message = packet.ReadString();
        var id      = packet.ReadInt();

        Logger.Log("Server", $"Connected as player {id}. {message}");
        Client.get.id = id;
        ClientSend.WelcomeReceived();

        var localEndPoint = (IPEndPoint)Client.get.tcp.socket.Client.LocalEndPoint;
        Client.get.udp.Connect(localEndPoint.Port);
    }

    public static void SpawnPlayer(Packet packet) {
        int        id       = packet.ReadInt();
        string     username = packet.ReadString();
        Vector3    position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.get.SpawnPlayer(id, username, position, rotation);
    }

    public static void UpdatePlayerPosition(Packet packet) {
        var id = packet.ReadInt();
        var position = packet.ReadVector3();
        GameManager.players[id].transform.position = position;
    }

    public static void UpdatePlayerRotation(Packet packet) {
        var id = packet.ReadInt();
        var rotation = packet.ReadQuaternion();
        GameManager.players[id].transform.rotation = rotation;
    }

    public static void PlayerChatMessage(Packet packet) {
        var username = packet.ReadString();
        var message = packet.ReadString();
        UIManager.get.AddToChatHistory($"{username}: {message}");
        Logger.Log("Chat", $"{username}: {message}");
    }

    public static void ServerMessage(Packet packet) {
        var message = packet.ReadString();
        Logger.Log("Chat", $"SERVER: {message}");
    }
}
