using Godot;
using System;

namespace VirtualBrightPlayz.MultiplayerBackend
{
    // [NetworkClass("NetworkVector3")]
    public class NetworkVector3
    {
        [NetworkProperty("long")]
        public long networkId;

        [NetworkProperty("Vector3")]
        public Vector3 pos;
    }

}