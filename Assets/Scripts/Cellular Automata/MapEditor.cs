using UnityEditor;
using UnityEngine;

public class MapEditor : MonoBehaviour
{
    public CellularAutomataVisualizer cellularAutomataVisualizer;

    public Tool selectedTool;



    public void Update()
    {
        if (Input.GetMouseButton(0) && selectedTool != Tool.PLACE_TUNNEL_POINT)
        {
            cellularAutomataVisualizer.PaintPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition), (CellularAutomataVisualizer.TileType)(int)selectedTool);
        }
    }

    public enum Tool : int{
        DRAW_WALL = 1,
        ERASE_WALL = 0,
        PLACE_TUNNEL_POINT = -1
    }
}
