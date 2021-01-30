namespace GameServer {
    class ServerHandle {
        public static void WelcomeReceived(int fromClient, Packet packet) {
            var clientId = packet.ReadInt();
            var username = packet.ReadString();

            Logger.Log("Connect", $"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully as player {fromClient} with username \"{username}\".");
            if(fromClient != clientId) {
                Logger.Error("Connect", $"Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID {clientId}!");
            }

            Server.clients[clientId].SendIntoGame(username);
        }

        public static void PlayerMovement(int fromClient, Packet packet) {
            var inputLength = packet.ReadInt();
            bool[] inputs = new bool[inputLength];
            for (int i = 0; i < inputs.Length; ++i) {
                inputs[i] = packet.ReadBool();
            }
            var rotation = packet.ReadQuaternion();
            Server.clients[fromClient].player.SetInput(inputs, rotation);
        }

        public static void PlayerChatMessage(int fromClient, Packet packet) {
            var player = Server.clients[fromClient].player;
            var message = packet.ReadString();
            
            Logger.Log("Chat", $"{player.username}: {message}");
            ServerSend.PlayerChatMessage(player, message);
        }
    }
}