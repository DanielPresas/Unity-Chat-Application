using System;
using System.Numerics;

namespace GameServer {
    class Player {
        public int id;
        public string username;

        public Vector3 position;
        public Quaternion rotation;

        private float _moveSpeed = 5.0f * Constants.DELTA_TIME;
        private bool[] _inputs;


        public Player(int playerId, string playerName, Vector3 spawnPos) {
            id = playerId;
            username = playerName;
            position = spawnPos;
            rotation = Quaternion.Identity;
            _inputs = new bool[4];
        }

        public void Update() {
            var inputDirection = Vector2.Zero;
            // inputDirection.X += (_inputs[3] ? 1.0f : 0.0f) - (_inputs[2] ? 1.0f : 0.0f);
            // inputDirection.Y += (_inputs[0] ? 1.0f : 0.0f) - (_inputs[1] ? 1.0f : 0.0f);
            
            if(_inputs[0]) inputDirection.Y += 1.0f;
            if(_inputs[1]) inputDirection.Y -= 1.0f;
            if(_inputs[2]) inputDirection.X += 1.0f;
            if(_inputs[3]) inputDirection.X -= 1.0f;

            var forward = Vector3.Transform(new Vector3(0, 0, 1), rotation);
            var right   = Vector3.Normalize(Vector3.Cross(forward, new Vector3(0, 1, 0)));

            var moveDirection = right * inputDirection.X + forward * inputDirection.Y;
            position += moveDirection * _moveSpeed;

            ServerSend.UpdatePlayerPosition(this);
            ServerSend.UpdatePlayerRotation(this);
        }

        public void SetInput(bool[] inputs, Quaternion rot) {
            _inputs = inputs;
            rotation = rot;
        }
    }
}