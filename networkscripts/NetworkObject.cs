using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;

// this class is an semi abstract class of the base network object.

namespace VirtualBrightPlayz.MultiplayerBackend
{

    public static class VectorUtil
    {
        public static bool StrToVec3(string data, out Vector3 vec3)
        {
            var d = data.Remove(data.Length - 1, 1).Remove(0, 1).Split(',');
            float x;
            float y;
            float z;
            if (float.TryParse(d[0], out x) && float.TryParse(d[1], out y) && float.TryParse(d[2], out z))
            {
                vec3 = new Vector3(x, y, z);
                return true;
            }
            vec3 = Vector3.Zero;
            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class NetworkProperty : Attribute
    {
        public string key { get; private set; }

        public NetworkProperty(string key)
        {
            this.key = key;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NetworkClass : Attribute
    {
        public string key { get; private set; }

        public NetworkClass(string key)
        {
            this.key = key;
        }
    }

    public static class NetworkObjectParser
    {
        public static string Deconstruct(object obj)
        {
            var fields = new Godot.Collections.Dictionary();
            // if (((NetworkClass)obj.GetType().GetCustomAttribute(typeof(NetworkClass))).key)
            // fields.Add("ObjectType", ((NetworkClass)obj.GetType().GetCustomAttribute(typeof(NetworkClass))).key);
            fields.Add("ObjectType", obj.GetType().AssemblyQualifiedName);
            foreach (var field in obj.GetType().GetFields())
            {
                if (Attribute.IsDefined(field, typeof(NetworkProperty)) && !((NetworkProperty)field.GetCustomAttribute(typeof(NetworkProperty))).key.Equals("ObjectType"))
                {
                    // fields.Add(((NetworkProperty)field.GetCustomAttribute(typeof(NetworkProperty))).key, field.GetValue(obj));
                    fields.Add(field.Name, field.GetValue(obj));
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
            if (!dict.Keys.Contains("ObjectType") || !dict.Keys.Contains("networkId") || dict["ObjectType"].GetType() != typeof(string) || dict["networkId"].GetType() != typeof(Single))
            {
                return null;
            }
            Type objtype = Type.GetType(((string)dict["ObjectType"]).Trim());
            if (objtype == null)
            {
                GD.Print(dict["ObjectType"]);
                return null;
            }
            var obj = Activator.CreateInstance(objtype);
            foreach (var pair in dict)
            {
                if (pair.Key.Equals("ObjectType")) continue;
                foreach (var field in obj.GetType().GetFields())
                {
                    if (Attribute.IsDefined(field, typeof(NetworkProperty)) && field.Name.Equals(pair.Key)) // ((NetworkProperty)field.GetCustomAttribute(typeof(NetworkProperty))).key.Equals(pair.Key)
                    {
                        try
                        {
                            switch (((NetworkProperty)field.GetCustomAttribute(typeof(NetworkProperty))).key)
                            {
                                case "long":
                                    long l;
                                    if (long.TryParse(pair.Value.ToString(), out l))
                                    {
                                        field.SetValue(obj, l);
                                    }
                                    break;
                                case "byte":
                                    byte by;
                                    if (byte.TryParse(pair.Value.ToString(), out by))
                                    {
                                        field.SetValue(obj, by);
                                    }
                                    break;
                                case "bool":
                                    bool bo;
                                    if (bool.TryParse(pair.Value.ToString(), out bo))
                                    {
                                        field.SetValue(obj, bo);
                                    }
                                    break;
                                case "Vector3":
                                    Vector3 vector3;
                                    if (VectorUtil.StrToVec3(pair.Value.ToString(), out vector3))
                                    {
                                        field.SetValue(obj, vector3);
                                    }
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            GD.PrintErr(e.ToString());
                        }
                        //fields.Add(((NetworkProperty)field.GetCustomAttribute(typeof(NetworkProperty))).key, field.GetValue(obj));
                    }
                }
            }
            return obj;
        }
    }

    [NetworkClass("NetworkObject")]
    public class NetworkObject
    {
        [NetworkProperty("long")]
        public long networkId;

        public override string ToString()
        {
            return JSON.Print(GetDataArray());
        }

        public virtual Godot.Collections.Dictionary GetDataArray()
        {
            var dict = new Godot.Collections.Dictionary();
            dict.Add("id", networkId);
            return dict;
        }

        public virtual bool TryParse(string inputData)
        {
            var data = JSON.Parse(inputData);
            if (data.Error != Error.Ok)
            {
                return false;
            }
            if (data.Result.GetType() != typeof(Godot.Collections.Dictionary))
            {
                return false;
            }
            var dict = (Godot.Collections.Dictionary)data.Result;
            object idobj;
            long idobj2;
            if (!dict.TryGetValue("id", out idobj) || !long.TryParse(idobj.ToString(), out idobj2))
            {
                // GD.Print(idobj.GetType().ToString());
                return false;
            }
            GD.Print(idobj.ToString());
            GD.Print(long.TryParse(idobj.ToString(), out idobj2));
            networkId = idobj2;
            return true;
            // var data = inputData.Split(';');
            // return long.TryParse(data[0], out networkId);
        }
    }
}