using UnityEngine;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public class SerializableVector2{
    public float x;
    public float y;

    [JsonIgnore]
    public Vector2 UnityVector{
        get{
            return new Vector3(x, y);
        }
    }

    public SerializableVector2(Vector2 v){
        x = v.x;
        y = v.y;
    }

    public static List<SerializableVector2> GetSerializableList(List<Vector2> vList){
        List<SerializableVector2> list = new List<SerializableVector2>(vList.Count);
        for(int i = 0 ; i < vList.Count ; i++){
            list.Add(new SerializableVector2(vList[i]));
        }
        return list;
    }

    public static List<Vector2> GetSerializableList(List<SerializableVector2> vList){
        List<Vector2> list = new List<Vector2>(vList.Count);
        for(int i = 0 ; i < vList.Count ; i++){
            list.Add(vList[i].UnityVector);
        }
        return list;
    }
}