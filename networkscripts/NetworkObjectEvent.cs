using Godot;
using System;

namespace VirtualBrightPlayz.MultiplayerBackend
{
    [NetworkEventClass("")]
    public class NetworkObjectEvent
    {

        [NetworkEventProperty("AddNewObject")]
        public void AddNewObject(object obj)
        {
            if (obj.GetType() == typeof(string))
            {
                var nwo = NetworkObjectParser.Construct((string)obj);
                NetworkManager.instance.AddObject(nwo);
                GD.Print("Added New Object " + obj.ToString());
            }
        }
    }
}