using Godot;
using System;
using System.Collections.Generic;

// this class is used to make and store network objects

namespace VirtualBrightPlayz.MultiplayerBackend
{
    public class NetworkManager : Node
    {
        public static NetworkManager instance { get; private set; }

        [Export]
        public NodePath backendPath;

        public NetworkBackend backend;

        public Dictionary<long, object> objects;
        private long objCounter = 0;

        public void AddObject(object obj)
        {
            long l;
            long.TryParse(obj.GetType().GetField("networkId").GetValue(obj).ToString(), out l);
            objects.Add(l, obj);
            // objCounter++;
        }

        public string NewObject(object obj)
        {
            objects.Add(objCounter, obj);
            var evnt = new NetworkObjectEvent();
            objCounter++;
            return NetworkEventParser.CallNetworkEvent(evnt, "AddNewObject", new[] { NetworkObjectParser.Deconstruct(obj) });
        }

        public string SendObject(object obj)
        {
            // objects.Add(objCounter, obj);
            var evnt = new NetworkObjectEvent();
            // objCounter++;
            return NetworkEventParser.CallNetworkEvent(evnt, "AddNewObject", new[] { NetworkObjectParser.Deconstruct(obj) });
        }

        public override void _Ready()
        {
            if (instance == null)
                instance = this;
            backend = GetNode<NetworkBackend>(backendPath);
            objects = new Dictionary<long, object>();
            NewObject(new NetworkVector3() {
                networkId = objCounter,
                pos = Vector3.Up
            });
        }

        public override void _Process(float delta)
        {
            if (backend.tcpstream != null && backend.tcpstream.GetStatus() == StreamPeerTCP.Status.Connected && backend.tcpstream.GetAvailableBytes() > 0)
            {
                instance = this;
                var objstr = backend.tcpstream.GetVar();
                if (objstr.GetType() == typeof(string))
                {
                    var obj = NetworkObjectParser.Construct((string)objstr);
                    // GD.Print(obj.GetType().AssemblyQualifiedName);
                    if (obj != null && obj.GetType() == typeof(NetworkVector3))
                    {
                        // GD.Print(obj);
                        var nwv3 = (NetworkVector3)obj;
                        GD.Print(nwv3.pos.ToString());
                    }
                    else if (obj == null)
                    {
                        var obj2 = NetworkEventParser.Construct((string)objstr);
                        if (obj2 != null)
                        {
                            GD.Print("Not null Event");
                            if (obj2.GetType() == typeof(NetworkObjectEvent))
                            {
                                GD.Print(objstr);
                            }
                        }
                    }
                }
            }
            if (backend.tcpserver != null)
            {
                instance = this;
                foreach (var conn in backend.connections)
                {
                    if (!conn.Value.IsConnectedToHost()) continue;
                    if (conn.Value.GetAvailableBytes() > 0)
                    {
                        var v = conn.Value.GetVar();
                        var obj = NetworkObjectParser.Construct(v.ToString());
                        if (obj == null) continue; // TODO: add code for instancing objects and more client side objects
                        if (objects.ContainsKey((long)obj.GetType().GetField("networkId").GetValue(obj)))
                        {
                            
                        }
                        else
                        {
                            continue;
                        }
                        // ((long)obj.GetType().GetField("networkId").GetValue(obj))
                        // var isplayer = obj.GetType().GetField("isPlayer").GetValue(obj);
                        foreach (var sendconn in backend.connections)
                        {
                            if (conn.Key == sendconn.Key) continue;
                            sendconn.Value.PutVar(v);
                        }
                    }
                    // foreach (var objpair in objects)
                    // {
                    //     var obj = NetworkObjectParser.Deconstruct(objpair.Value);
                    //     conn.Value.PutVar(obj);
                    // }
                }
            }
        }

        public void NewConnection(StreamPeerTCP conn, long counter)
        {
            foreach (var obj in objects)
            {
                conn.PutVar(SendObject(obj.Value));
            }
        }
    }
}