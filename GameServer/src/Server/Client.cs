using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace GameServer {
    class Client {
        public static int dataBufferSize = 4096;

        public int id;
        public Player player;
        public TCPInfo tcp;
        public UDPInfo udp;

        public Client(int clientId) {
            id = clientId;
            tcp = new TCPInfo(clientId);
            udp = new UDPInfo(clientId);
        }

        public class TCPInfo {
            public TcpClient socket;
            private readonly int _id;
            private NetworkStream _stream;
            private Packet _receivedPacket;
            private byte[] _receiveBuffer;

            public TCPInfo(int id) {
                _id = id;
            }

            public void Connect(TcpClient tcpSocket) {
                socket = tcpSocket;
                socket.ReceiveBufferSize = dataBufferSize;
                _receiveBuffer = new byte[dataBufferSize];

                _stream = socket.GetStream();
                _receivedPacket = new Packet();
                _stream.BeginRead(_receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                ServerSend.Welcome(_id, "Welcome to the server!");
            }

            public void Disconnect() {
                socket.Close();
                _stream = null;
                _receiveBuffer = null;
                _receivedPacket = null;
                socket = null;
            }

            public void SendData(Packet packet) {
                try {
                    if(socket == null) return;
                    _stream.BeginWrite(packet.Buffer, 0, packet.Length, null, null);

                } catch(Exception e) {
                    Logger.Error("Client", $"Error sending TCP data to player {_id}:\n{e}");
                }
            }

            private bool HandleData(byte[] data) {
                int packetLength = 0;
                _receivedPacket.SetBytes(data);

                if(_receivedPacket.UnreadLength >= 4) {
                    packetLength = _receivedPacket.ReadInt();
                    if(packetLength <= 0) return true;
                }

                while(packetLength > 0 && packetLength <= _receivedPacket.UnreadLength) {
                    byte[] packetBytes = _receivedPacket.ReadBytes(packetLength);
                    ThreadManager.ExecuteOnMainThread(() => {
                        using var packet = new Packet(packetBytes);
                        int packetId = packet.ReadInt();
                        Server.packetHandlers[packetId](_id, packet);
                    });
                    packetLength = 0;
                    if(_receivedPacket.UnreadLength >= 4) {
                        packetLength = _receivedPacket.ReadInt();
                        if(packetLength <= 0) return true;
                    }
                }

                if(packetLength <= 1) return true;
                return false;
            }

            private void ReceiveCallback(IAsyncResult result) {
                try {
                    int rBufferSize = _stream.EndRead(result);
                    if(rBufferSize <= 0) {
                        Server.clients[_id].Disconnect("No data in TCP stream");
                        return;
                    }

                    byte[] data = new byte[rBufferSize];
                    Array.Copy(_receiveBuffer, data, rBufferSize);

                    _receivedPacket.Reset(HandleData(data));
                    _stream.BeginRead(_receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                } catch(Exception e) {
                    Logger.Error("Client", $"Error receiving TCP data from stream: {e}");
                    Server.clients[_id].Disconnect("Exception receiving TCP data from stream");
                }
            }
        }

        public class UDPInfo {
            public IPEndPoint endPoint;
            private int _id = 0;

            public UDPInfo(int clientId) {
                _id = clientId;
            }

            public void Connect(IPEndPoint ipEndPoint) {
                endPoint = ipEndPoint;
            }

            public void Disconnect() {
                endPoint = null;
            }

            public void SendData(Packet packet) {
                Server.SendUDPData(endPoint, packet);
            }

            public void HandleData(Packet packet) {
                int packetLength = packet.ReadInt();
                byte[] data = packet.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() => {
                    using var packet = new Packet(data);
                    int packetId = packet.ReadInt();
                    Server.packetHandlers[packetId](_id, packet);
                });
            }
        }

        public void SendIntoGame(string playerName) {
            player = new Player(id, playerName, Vector3.Zero);

            foreach(var c in Server.clients.Values) {
                if(c.player == null) continue;
                if(c.id == id) continue;
                ServerSend.SpawnPlayer(id, c.player);
            }

            foreach(var c in Server.clients.Values) {
                if(c.player == null) continue;
                ServerSend.SpawnPlayer(c.id, player);
            }
        }

        public void Disconnect(string message = "Unknown reason") {
            Logger.Warning("Disconnect", $"Player \"{player.username}\" ({tcp.socket.Client.RemoteEndPoint}) has been disconnected from the server ({message}).");
            player = null;

            tcp.Disconnect();
            udp.Disconnect();
        }
    }
}