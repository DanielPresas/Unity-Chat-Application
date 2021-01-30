using System;

namespace GameServer {
    class ServerSend {
        private static void SendTCPData(int toClient, Packet packet) {
            packet.WriteLength();
            Server.clients[toClient].tcp.SendData(packet);
        }

        private static void SendTCPDataToAll(Packet packet) {
            packet.WriteLength();
            foreach(var c in Server.clients) { c.Value.tcp.SendData(packet); }
        }

        private static void SendTCPDataToAllExcept(int exceptClient, Packet packet) {
            packet.WriteLength();
            foreach(var c in Server.clients) {
                if(c.Key == exceptClient) continue;
                c.Value.tcp.SendData(packet);
            }
        }

        private static void SendUDPData(int toClient, Packet packet) {
            packet.WriteLength();
            Server.clients[toClient].udp.SendData(packet);
        }

        private static void SendUDPDataToAll(Packet packet) {
            packet.WriteLength();
            foreach(var c in Server.clients) {
                c.Value.udp.SendData(packet);
            }
        }

        private static void SendUDPDataToAllExcept(int exceptClient, Packet packet) {
            packet.WriteLength();
            foreach(var c in Server.clients) {
                if(c.Key == exceptClient) continue;
                c.Value.udp.SendData(packet);
            }
        }

        // ------------------------------------------------------------------------------

        public static void Welcome(int toClient, string message) {
            using(var packet = new Packet(ServerPacket.Welcome)) {
                packet.Write(message);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }

        public static void SpawnPlayer(int toClient, Player player) {
            using(var packet = new Packet(ServerPacket.PlayerSpawn)) {
                packet.Write(player.id);
                packet.Write(player.username);
                packet.Write(player.position);
                packet.Write(player.rotation);

                SendTCPData(toClient, packet);
            }
        }

        public static void UpdatePlayerPosition(Player player) {
            using(var packet = new Packet(ServerPacket.PlayerPosition)) {
                packet.Write(player.id);
                packet.Write(player.position);
                SendUDPDataToAll(packet);
            }
        }

        public static void UpdatePlayerRotation(Player player) {
            using(var packet = new Packet(ServerPacket.PlayerRotation)) {
                packet.Write(player.id);
                packet.Write(player.rotation);
                SendUDPDataToAllExcept(player.id, packet);
            }
        }

        public static void PlayerChatMessage(Player player, string message) {
            using(var packet = new Packet(ServerPacket.PlayerChatReceived)) {
                packet.Write(player.username);
                packet.Write(message);
                SendTCPDataToAll(packet);
            }
        }

        public static void ServerMessage(string message) {
            using(var packet = new Packet(ServerPacket.ServerMessage)) {
                packet.Write(message);
                SendTCPDataToAll(packet);
            }
        }
    }
}