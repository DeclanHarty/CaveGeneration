using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VisualsManager))]
public class VisualsManagerEditor : Editor
{
    private VisualsManager visualsManager;
    private void OnEnable()
    {
        visualsManager = target as VisualsManager;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Reset Image"))
        {
            visualsManager.InitializeMap();
        }

        if (GUILayout.Button("Rasterize Rooms"))
        {
            visualsManager.RasterizeRooms();
        }

        if (GUILayout.Button("Rasterize Tunnels"))
        {
            visualsManager.RasterizeTunnels();
        }

        if (GUILayout.Button("Regenerate Map"))
        {
            visualsManager.RegenerateMap();
        }
        
    }
}
