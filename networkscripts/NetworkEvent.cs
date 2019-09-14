using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace VirtualBrightPlayz.MultiplayerBackend
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NetworkEventClass : Attribute
    {
        public string key { get; private set; }

        public NetworkEventClass(string key)
        {
            this.key = key;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class NetworkEventProperty : Attribute
    {
        public string key { get; private set; }

        public NetworkEventProperty(string key)
        {
            this.key = key;
        }
    }

    public static class NetworkEventParser
    {
        public static string CallNetworkEvent(object networkeventclass, string eventKey, object[] args)
        {
            var fields = new Godot.Collections.Dictionary();
            // if (((NetworkClass)obj.GetType().GetCustomAttribute(typeof(NetworkClass))).key)
            // fields.Add("ObjectType", ((NetworkClass)obj.GetType().GetCustomAttribute(typeof(NetworkClass))).key);
            fields.Add("EventType", networkeventclass.GetType().AssemblyQualifiedName);
            foreach (var field in networkeventclass.GetType().GetMethods())
            {
                if (Attribute.IsDefined(field, typeof(NetworkEventProperty))
                && !((NetworkEventProperty)field.GetCustomAttribute(typeof(NetworkEventProperty))).key.Equals("EventType")
                && !((NetworkEventProperty)field.GetCustomAttribute(typeof(NetworkEventProperty))).key.Equals("args")
                && !((NetworkEventProperty)field.GetCustomAttribute(typeof(NetworkEventProperty))).key.Equals("event")
                && ((NetworkEventProperty)field.GetCustomAttribute(typeof(NetworkEventProperty))).key.Equals(eventKey))
                {
                    // fields.Add(((NetworkProperty)field.GetCustomAttribute(typeof(NetworkProperty))).key, field.GetValue(obj));
                    fields.Add("event", ((NetworkEventProperty)field.GetCustomAttribute(typeof(NetworkEventProperty))).key);
                    break;
                }
            }
            var eventargs = new Godot.Collections.Array();
            foreach (var arg in args)
            {
                eventargs.Add(arg);
            }
            fields.Add("args", eventargs);
            return JSON.Print(fields);
        }

        public static string Deconstruct(object obj)
        {
            var fields = new Godot.Collections.Dictionary();
            // if (((NetworkClass)obj.GetType().GetCustomAttribute(typeof(NetworkClass))).key)
            // fields.Add("ObjectType", ((NetworkClass)obj.GetType().GetCustomAttribute(typeof(NetworkClass))).key);
            fields.Add("EventType", obj.GetType().AssemblyQualifiedName);
            foreach (var field in obj.GetType().GetMethods())
            {
                if (Attribute.IsDefined(field, typeof(NetworkEventProperty)) && !((NetworkEventProperty)field.GetCustomAttribute(typeof(NetworkEventProperty))).key.Equals("EventType")&& !((NetworkEventProperty)field.GetCustomAttribute(typeof(NetworkEventProperty))).key.Equals("args"))
                {
                    // fields.Add(((NetworkProperty)field.GetCustomAttribute(typeof(NetworkProperty))).key, field.GetValue(obj));
                    fields.Add(field.Name, ((NetworkEventProperty)field.GetCustomAttribute(typeof(NetworkEventProperty))).key);
                }
            }
            return JSON.Print(fields);
        }

        public static object Construct(string data)
        {
            var jsondata = JSON.Parse(data);
            if (jsondata.Result.GetType() != typeof(Godot.Collections.Dictionary))
            {
                return null;
            }
            var dict = (Godot.Collections.Dictionary)jsondata.Result;
            if (!dict.Keys.Contains("EventType") || dict["EventType"].GetType() != typeof(string) || !dict.Keys.Contains("event") || dict["event"].GetType() != typeof(string) || !dict.Keys.Contains("args") || dict["args"].GetType() != typeof(Godot.Collections.Array))
            {
                return null;
            }
            Type objtype = Type.GetType(((string)dict["EventType"]).Trim());
            if (objtype == null || objtype.GetCustomAttribute(typeof(NetworkEventClass)) == null)
            {
                GD.Print(dict["EventType"]);
                return null;
            }
            var obj = Activator.CreateInstance(objtype);
            foreach (var pair in dict)
            {
                if (pair.Key.Equals("EventType") || pair.Key.Equals("args")) continue;
                if (!pair.Key.Equals("event")) continue;
                foreach (var field in obj.GetType().GetMethods())
                {
                    if (Attribute.IsDefined(field, typeof(NetworkEventProperty)) && ((NetworkEventProperty)field.GetCustomAttribute(typeof(NetworkEventProperty))).key.Equals(pair.Value))
                    {
                        try
                        {
                            object[] argsall = new List<object>(((Godot.Collections.Array)dict["args"])).ToArray();
                            field.Invoke(obj, argsall);
                        }
                        catch (Exception e)
                        {
                            GD.Print(e.ToString());
                        }
                        //fields.Add(((NetworkProperty)field.GetCustomAttribute(typeof(NetworkProperty))).key, field.GetValue(obj));
                    }
                }
            }
            return obj;
        }
    }

    [NetworkEventClass("")]
    public class NetworkEvent
    {
    }
}