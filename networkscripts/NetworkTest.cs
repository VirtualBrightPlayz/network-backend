using Godot;
using System;
using System.Collections.Generic;

// testing class

namespace VirtualBrightPlayz.MultiplayerBackend
{
    public class NetworkTest : Node
    {
        string ipaddr = "localhost";

        public override void _Ready()
        {
            var obj = new NetworkObject();
            obj.networkId = 1054;
            // GD.Print(obj.ToString());
            GD.Print(NetworkObjectParser.Deconstruct(obj));
            var obj2 = (NetworkObject)NetworkObjectParser.Construct(NetworkObjectParser.Deconstruct(obj));
            GD.Print(obj2.networkId);
            GetNode("./Host").Connect("pressed", this, "Host");
            GetNode("./Join").Connect("pressed", this, "Join");
            GetNode("./IP").Connect("text_changed", this, "SetIP");
        }

        public void SetIP(string ip)
        {
            ipaddr = ip;
        }

        public void Join()
        {
            var node = (NetworkManager)ResourceLoader.Load<PackedScene>("res://NetworkManager.tscn").Instance();
            AddChild(node);
            node.backend.Join(ipaddr == string.Empty ? "127.0.0.1" : ipaddr);
        }

        public void Host()
        {
            var node = (NetworkManager)ResourceLoader.Load<PackedScene>("res://NetworkManager.tscn").Instance();
            AddChild(node);
            node.backend.Host();
        }
    }
}