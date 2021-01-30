using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour {
    public static void SendTCPData(Packet packet) {
        packet.WriteLength();
        Client.get.tcp.SendData(packet);
    }

    public static void SendUDPData(Packet packet) {
        packet.WriteLength();
        Client.get.udp.SendData(packet);
    }


    public static void WelcomeReceived() {
        using(var packet = new Packet(ClientPacket.WelcomeReceived)) {
            packet.Write(Client.get.id);
            packet.Write(UIManager.get.usernameField.text);

            SendTCPData(packet);
        }
    }

    public static void PlayerMovement(bool[] inputs) {
        using(var packet = new Packet(ClientPacket.PlayerMovement)) {
            packet.Write(inputs.Length);
            foreach(var i in inputs) {
                packet.Write(i);
            }
            packet.Write(GameManager.players[Client.get.id].transform.rotation);
            SendUDPData(packet);
        }
    }

    public static void PlayerChatMessage(string message) {
        using(var packet = new Packet(ClientPacket.PlayerChatMessage)) {
            packet.Write(message);
            SendTCPData(packet);
        }
    }
}
