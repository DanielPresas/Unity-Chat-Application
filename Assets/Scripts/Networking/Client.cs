using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour {
    public static Client get;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int id = 0;

    public TCPInfo tcp;
    public UPDInfo udp;

    public bool isConnected { get; private set; } = false;

    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> _packetHandlers;

    public class TCPInfo {
        public TcpClient socket;

        private NetworkStream _stream;
        private Packet _receivedPacket;
        private byte[] _receiveBuffer;

        public void Connect() {
            socket = new TcpClient {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            _receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(get.ip, get.port, ConnectCallback, socket);
        }

        public void Disconnect(string message = "Unknown reason") {
            get.Disconnect(message);
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
                Logger.Error("Client", $"Error sending TCP data to server: {e}");
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
                    using(var packet = new Packet(packetBytes)) {
                        int packetId = packet.ReadInt();
                        _packetHandlers[packetId](packet);
                    }
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

        private void ConnectCallback(IAsyncResult result) {
            socket.EndConnect(result);
            if(!socket.Connected) return;

            _stream = socket.GetStream();
            _receivedPacket = new Packet();
            _stream.BeginRead(_receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult result) {
            if(socket == null || _stream == null) return;

            try {
                int rBufferSize = _stream.EndRead(result);
                if(rBufferSize <= 0) {
                    get.Disconnect("No data in TCP stream");
                    return;
                }

                byte[] data = new byte[rBufferSize];
                Array.Copy(_receiveBuffer, data, rBufferSize);

                _receivedPacket.Reset(HandleData(data));

                _stream.BeginRead(_receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            } catch /*(Exception e)*/ {
                // Logger.Error($"Error receiving TCP data from stream: {e}");
                Disconnect("Exception receiving TCP data from stream");
            }
        }


    }

    public class UPDInfo {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UPDInfo() {
            endPoint = new IPEndPoint(IPAddress.Parse(get.ip), get.port);
        }

        public void Connect(int localPort) {
            socket = new UdpClient(localPort);
            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using(var packet = new Packet()) {
                SendData(packet);
            }
        }

        public void Disconnect(string message = "Unknown reason") {
            get.Disconnect(message);
            endPoint = null;
            socket = null;
        }

        public void SendData(Packet packet) {
            try {
                packet.InsertInt(get.id);
                if(socket == null) return;
                socket.BeginSend(packet.Buffer, packet.Length, null, null);
            } catch(Exception e) {
                Logger.Error("Client", $"Error sending UDP data to server: {e}");
            }
        }

        public void ReceiveCallback(IAsyncResult result) {
            if(socket == null || endPoint == null) return;

            try {
                byte[] data = socket.EndReceive(result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if(data.Length < 4) {
                    get.Disconnect("No data in UDP socket");
                    return;
                }
                HandleData(data);
            } catch /*(Exception e)*/ {
                // Logger.Error($"Error receiving UDP data from stream: {e}");
                Disconnect("Exception receiving UDP data from stream");
            }
        }

        private void HandleData(byte[] data) {
            using(var packet = new Packet(data)) {
                int packetLength = packet.ReadInt();
                data = packet.ReadBytes(packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() => {
                using(var packet = new Packet(data)) {
                    int packetId = packet.ReadInt();
                    _packetHandlers[packetId](packet);
                }
            });
        }
    }


    private void Awake() {
        if(get != null) {
            Logger.Error("Client", "Instance already exists, destroying new object!");
            Destroy(this);
            return;
        }

        get = this;
    }

    private void Start() {
        tcp = new TCPInfo();
        udp = new UPDInfo();
    }

    private void OnApplicationQuit() {
        Disconnect("Application quit");
    }

    private void InitializeClientData() {
        _packetHandlers = new Dictionary<int, PacketHandler>() {
            { (int)ServerPacket.Welcome,            ClientHandle.Welcome },
            { (int)ServerPacket.PlayerSpawn,        ClientHandle.SpawnPlayer },
            { (int)ServerPacket.PlayerPosition,     ClientHandle.UpdatePlayerPosition },
            { (int)ServerPacket.PlayerRotation,     ClientHandle.UpdatePlayerRotation },
            { (int)ServerPacket.PlayerChatReceived, ClientHandle.PlayerChatMessage },
            { (int)ServerPacket.ServerMessage,      ClientHandle.ServerMessage },
        };

        Logger.Log("Client", "Initialized client packet handlers.");
    }

    public void ConnectToServer() {
        InitializeClientData();
        tcp.Connect();
        isConnected = true;
    }

    private void Disconnect(string message = "Unknown reason") {
        if(!isConnected) return;

        isConnected = false;
        tcp.socket.Close();
        udp.socket.Close();

        Logger.Warning("Client", $"Disconnected from server ({message}).");
    }
}
