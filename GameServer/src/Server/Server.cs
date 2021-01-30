using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace GameServer {
    class Server {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }

        public delegate void PacketHandler(int fromClient, Packet packet);
        public static Dictionary<int, PacketHandler> packetHandlers;
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        private static TcpListener _tcpListener;
        private static UdpClient _udpListener;

        public static void Start(int maxPlayers, int port) {
            MaxPlayers = maxPlayers;
            Port = port;

            Logger.Info("Server", "Starting server...");
            InitializeServerData();

            _tcpListener = new TcpListener(IPAddress.Any, port);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            _udpListener = new UdpClient(port);
            _udpListener.BeginReceive(UDPReceiveCallback, null);

            Logger.Info("Server", $"Server started on port {Port}.");
        }

        private static void InitializeServerData() {
            for(int i = 1; i <= MaxPlayers; ++i) {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>() {
                { (int)ClientPacket.WelcomeReceived,   ServerHandle.WelcomeReceived   },
                { (int)ClientPacket.PlayerMovement,    ServerHandle.PlayerMovement    },
                { (int)ClientPacket.PlayerChatMessage, ServerHandle.PlayerChatMessage },
            };

            Logger.Info("Server", "Initialized server packet handlers.");
        }

        public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet) {
            try {
                if(clientEndPoint == null) return;
                _udpListener.BeginSend(packet.Buffer, packet.Length, clientEndPoint, null, null);
            } catch(Exception e) {
                Logger.Error("Server", $"Error sending UDP data to {clientEndPoint}: {e}");
            }
        }

        private static void TCPConnectCallback(IAsyncResult result) {
            TcpClient client = _tcpListener.EndAcceptTcpClient(result);
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Logger.Log("Server", $"Incoming connection request from {client.Client.RemoteEndPoint}...");

            for(int i = 1; i <= MaxPlayers; i++) {
                if(clients[i].tcp.socket == null) {
                    clients[i].tcp.Connect(client);
                    return;
                }
            }

            Logger.Error("Server", $"{client.Client.RemoteEndPoint} failed to connect: Server is full!");
        }

        private static void UDPReceiveCallback(IAsyncResult result) {
            try {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _udpListener.EndReceive(result, ref clientEndPoint);
                _udpListener.BeginReceive(UDPReceiveCallback, null);
                if(data.Length < 4) return;

                using(var packet = new Packet(data)) {
                    int clientId = packet.ReadInt();
                    if(clientId == 0) return;

                    if(clients[clientId].udp.endPoint == null) {
                        clients[clientId].udp.Connect(clientEndPoint);
                        return;
                    }

                    if(clients[clientId].udp.endPoint.ToString() == clientEndPoint.ToString()) {
                        clients[clientId].udp.HandleData(packet);
                    }
                }
            } catch(Exception e) {
                Logger.Error("Server", $"Error receiving UDP data: {e}");
            }
        }
    }
}