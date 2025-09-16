using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunatorSharp;
using Unity.VisualScripting;
using System.Linq;
using DelaunatorSharp.Unity.Extensions;
using UnityEditor.UI;

public class VisualsManager : MonoBehaviour
{
    public Vector2 topLeftCorner;
    public Vector2 bottomRightCorner;
    public int gridScale;
    public int numberOfLoops;

    public float maxEdgeDistanceToCellSizeRatio;

    public Delaunator delaunator;
    HashSet<Vector2Int> MST = new HashSet<Vector2Int>();
    HashSet<Vector2Int> loopEdges;


    public void Start()
    {
        float cellSize = Mathf.Abs(topLeftCorner.x - bottomRightCorner.x) / gridScale;

        IPoint[] points = GraphCreator.CreatePoints(topLeftCorner, bottomRightCorner, gridScale);
        delaunator = GraphCreator.CreateDelauneyTriangulation(points);

        List<List<int>> ListMST = GraphCreator.CreateMinimumSpanningTree(delaunator);
        MST = PrimsAlgo.ConvertAdjacencyListToSet(ListMST);

        HashSet<Vector2Int> edges = GraphCreator.CreateEdgeIndexSetFromDelaunayTriagnulation(delaunator);
        edges.ExceptWith(MST);
        loopEdges = new HashSet<Vector2Int>();

        int i = 0;
        while (i < numberOfLoops && edges.Count() > 0)
        {
            Vector2Int[] edgesToSelectFrom = edges.ToArray();
            int randomIndex = Random.Range(0, edgesToSelectFrom.Length);
            Vector2Int selectedEdge = edgesToSelectFrom[randomIndex];
            float edgeLength = Vector3.Distance(points[selectedEdge.x].ToVector3(), points[selectedEdge.y].ToVector3());
            if (edgeLength > maxEdgeDistanceToCellSizeRatio * cellSize)
            {
                edges.Remove(selectedEdge);
                continue;
            }
            loopEdges.Add(selectedEdge);
            edges.Remove(selectedEdge);
            i++;
        }

    }

    void OnDrawGizmos()
    {
        IPoint[] points = delaunator?.Points;
        // Renders edges in the MST
        foreach (Vector2Int edge in MST)
        {
            Gizmos.color = Color.white;
            Vector3 startPoint = new Vector3((float)points[edge.x].X, (float)points[edge.x].Y);
            Vector3 endPoint = new Vector3((float)points[edge.y].X, (float)points[edge.y].Y);
            Gizmos.DrawLine(startPoint, endPoint);
        }

        // Renders all other edges of the Delauney Triangulation
        if (delaunator != null)
        {
            HashSet<Vector2Int> edgesP = GraphCreator.CreateEdgeIndexSetFromDelaunayTriagnulation(delaunator);
            edgesP.ExceptWith(MST);
            edgesP.ExceptWith(loopEdges);
            foreach (Vector2Int edge in edgesP)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawLine(points[edge.x].ToVector3(), points[edge.y].ToVector3());
            }
            // Renders the added Loops
            foreach (Vector2Int edge in loopEdges)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(points[edge.x].ToVector3(), points[edge.y].ToVector3());
            }
        }

        
        // Renders the Points on the graph
        if (points != null)
        {
            for (int i = 0; i < points.Length; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(new Vector3((float)points[i].X, (float)points[i].Y), .1f);
            }
        }
    }
}
