using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;
using UnityEditor;
using UnityEngine;

public class MapEditor : MonoBehaviour
{
    public List<TunnelPoint> tunnelPoints;
    public TunnelPoint selectedTunnelPoint;
    public GameObject tunnelPointPrefab;
    public CellularAutomataVisualizer cellularAutomataVisualizer;
    public Tool selectedTool;

    public void Update()
    {
        if (Input.GetMouseButton(0) && selectedTool != Tool.NONE)
        {
            cellularAutomataVisualizer.PaintPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition), (CellularAutomataVisualizer.TileType)(int)selectedTool);
        }
    }

    public void SetTool(Tool tool)
    {
        selectedTool = tool;
    }

    public void SetToolDraw()
    {
        selectedTool = Tool.DRAW_WALL;
        UnselectTunnelPoint();
    }

    public void SetToolErase()
    {
        selectedTool = Tool.ERASE_WALL;
        UnselectTunnelPoint();
    }

    public void SetSelectedTunnelPoint(TunnelPoint tunnelPoint)
    {
        UnselectTunnelPoint();
        selectedTunnelPoint = tunnelPoint;
        selectedTool = Tool.NONE;
    }

    public void UnselectTunnelPoint()
    {
        if (selectedTunnelPoint)
        {
            selectedTunnelPoint.Unselect();
            selectedTunnelPoint = null;
        }
        
    }

    public void ResetEditor()
    {
        cellularAutomataVisualizer.CreateMap();
    }

    public void AddTunnelPoint(Vector2 position)
    {
        TunnelPoint tunnelPoint = Instantiate(tunnelPointPrefab, position, Quaternion.identity).GetComponent<TunnelPoint>();
        SetSelectedTunnelPoint(tunnelPoint);

        tunnelPoint.SetMapEditor(this);
        tunnelPoint.Select();
        tunnelPoints.Add(tunnelPoint);
    }
    
    public void AddTunnelPoint()
    {
        TunnelPoint tunnelPoint = Instantiate(tunnelPointPrefab, Vector2.zero, Quaternion.identity).GetComponent<TunnelPoint>();
        SetSelectedTunnelPoint(tunnelPoint);

        tunnelPoint.SetMapEditor(this);
        tunnelPoint.Select();
        tunnelPoints.Add(tunnelPoint);
    }

    public void DeleteSelectedPoint()
    {
        if (selectedTunnelPoint)
        {
            tunnelPoints.Remove(selectedTunnelPoint);
            Destroy(selectedTunnelPoint.gameObject);
            selectedTunnelPoint = null;
        }

    }

    public void SaveRoom()
    {
        CARoom room = new CARoom();

        List<Vector2> tunnelPointsPositions = new List<Vector2>(tunnelPoints.Count);
        for (int i = 0; i < tunnelPoints.Count; i++)
        {
            tunnelPointsPositions.Add(tunnelPoints[i].GetPosition());
        }



        room.tunnelPoints = SerializableVector2.GetSerializableList(tunnelPointsPositions).ToArray();

        int[,] map = cellularAutomataVisualizer.GetMap();
        Vector2Int mapSize = cellularAutomataVisualizer.mapSize;

        int[] map1D = new int[mapSize.x * mapSize.y];
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                map1D[x + mapSize.x * y] = map[x, y];
            }
        }

        room.map = map1D;

        room.mapSize = mapSize;

        CARoomFileManager.SaveCARoom(room);
    }

    public void OpenRoom()
    {
        CARoom room = CARoomFileManager.OpenCARoom();

        ClearAllTunnelPoints();
        CreateTunnelPointsFromPositions(SerializableVector2.GetSerializableList(room.tunnelPoints.ToList()).ToArray());
        selectedTool = Tool.NONE;
        cellularAutomataVisualizer.SetMap(room.map, room.mapSize);
    }

    public void ClearAllTunnelPoints()
    {

        foreach (TunnelPoint tunnelPoint in tunnelPoints)
        {
            Destroy(tunnelPoint.gameObject);
        }

        tunnelPoints.Clear();
        selectedTunnelPoint = null;
    }

    public void CreateTunnelPointsFromPositions(Vector2[] positions)
    {
        foreach (Vector2 position in positions)
        {
            AddTunnelPoint(position);
        }
    }

    public enum Tool : int
    {
        DRAW_WALL = 1,
        ERASE_WALL = 0,
        NONE = -1
    }
}
