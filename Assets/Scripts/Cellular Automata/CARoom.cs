using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CARoom
{
    public SerializableVector2[] tunnelPoints;
    public int[] map;
    public Vector2Int mapSize;

    public void PrintJsonString()
    {
        Debug.Log(Newtonsoft.Json.JsonConvert.SerializeObject(this));
    }
}
