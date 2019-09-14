using Godot;
using System;
using System.Collections.Generic;

// this class is the very backbone of the network, managing direct udp and tcp sockets and packets.

namespace VirtualBrightPlayz.MultiplayerBackend
{
    public class NetworkBackend : Node
    {
        public PacketPeerStream tcppeer;
        public PacketPeerUDP udppeer;
        public StreamPeerTCP tcpstream;
        public TCP_Server tcpserver;

        private long counter;

        public Dictionary<long, StreamPeerTCP> connections;

        public void Join(string ip = "127.0.0.1", int port = 27015)
        {
            // counter = 1;
            // connections.Clear();
            tcpstream = new StreamPeerTCP();
            // tcppeer = new PacketPeerStream();
            // tcppeer.StreamPeer = tcpstream;
            tcpstream.ConnectToHost(ip, port);
        }

        public void Host(int tcpport = 27015, int udpport = 27016)
        {
            counter = 1;
            connections.Clear();
            // udppeer = new PacketPeerUDP();
            // udppeer.Listen(udpport);
            tcpserver = new TCP_Server();
            tcpserver.Listen(tcpport);
        }

        public void CloseHost()
        {
            foreach (var conn in connections)
            {
                conn.Value.DisconnectFromHost();
            }
            tcpserver.Stop();
            udppeer.Close();
            udppeer = null;
            tcpserver = null;
            connections.Clear();
        }

        public override void _Ready()
        {
            connections = new Dictionary<long, StreamPeerTCP>();
        }

        public override void _Process(float delta)
        {
            if (tcpserver != null && tcpserver.IsConnectionAvailable())
            {
                connections.Add(counter, tcpserver.TakeConnection());
                GD.Print("Client connected. ", connections[counter].GetConnectedHost() + ":" + connections[counter].GetConnectedPort());
                GetParent<NetworkManager>().NewConnection(connections[counter], counter);
                counter++;
            }
        }
    }
}