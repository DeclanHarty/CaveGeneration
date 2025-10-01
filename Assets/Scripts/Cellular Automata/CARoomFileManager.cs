using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SFB;
using UnityEditor.Analytics;
using UnityEngine;

public class CARoomFileManager
{
    public static void SaveCARoom(CARoom room)
    {
        ExtensionFilter[] extensions = new ExtensionFilter[]
        {
            new ExtensionFilter("JSON", "json")
        };

        var path = StandaloneFileBrowser.SaveFilePanel("Save Room", ".", "untitled_room", extensions);

        try
        {
            using (FileStream fs = File.Create(path))
            {
                byte[] json = Encoding.UTF8.GetBytes(room.GetJsonString());
                fs.Write(json, 0, json.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing file: {ex.Message}");
        }
    }

    public static CARoom OpenCARoom()
    {
        ExtensionFilter[] extensions = new ExtensionFilter[]
        {
            new ExtensionFilter("JSON", "json"),
            new ExtensionFilter("Binary", "bin")
        };

        var path = StandaloneFileBrowser.OpenFilePanel("Open Room", ".", extensions, false);

        CARoom room;

        if (Regex.IsMatch(path[0], ".json$"))
        {
            room = ReadRoomFromJsonFile(path[0]);
        }
        else
        {
            throw new Exception("Cannot open invalid file type.");
        }

        return room;
    }

    public static CARoom ReadRoomFromJsonFile(string path)
    {
        using (FileStream fs = File.OpenRead(path))
        {
            List<char> data = new List<char>();
            int byteData;
            while ((byteData = fs.ReadByte()) != -1) {
                data.Add((char)byteData);
            }

            Debug.Log("Finished");

            return CARoom.CreateCARoomFromJson(new string(data.ToArray()));
        }
    }
}
