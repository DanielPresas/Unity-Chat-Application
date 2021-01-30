namespace GameServer {
    class GameLogic {
        public static void Update() {
            foreach(var c in Server.clients.Values) {
                if(c.player == null) continue;
                c.player.Update();
            }

            ThreadManager.UpdateMain();
        }
    }
}