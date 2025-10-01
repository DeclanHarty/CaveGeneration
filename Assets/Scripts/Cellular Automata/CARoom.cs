using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CARoom
{
    public SerializableVector2[] tunnelPoints;
    public int[] map;
    public Vector2Int mapSize;

    public string GetJsonString()
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(this);
    }

    public static CARoom CreateCARoomFromJson(string value)
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<CARoom>(value);
    }
}
