using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;

[CustomEditor(typeof(CellularAutomataVisualizer))]
public class CAVisualizerEditor : Editor
{
    private CellularAutomataVisualizer CAV;
    private void OnEnable()
    {
        CAV = target as CellularAutomataVisualizer;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Make Starter Map"))
        {
            CAV.CreateMap();
        }

        if (GUILayout.Button("Run CA"))
        {
            CAV.PerformCA();
        }
    }
}
