using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunatorSharp;
using Unity.VisualScripting;
using System.Linq;
using DelaunatorSharp.Unity.Extensions;

public class VisualsManager : MonoBehaviour
{
    public Vector2 topLeftCorner;
    public Vector2 bottomRightCorner;
    public int gridScale;

    public Delaunator delaunator;
    List<List<int>> MST;

    public void Start()
    {
        IPoint[] points = GraphCreator.CreatePoints(topLeftCorner, bottomRightCorner, gridScale);
        delaunator = GraphCreator.CreateDelauneyTriangulation(points);
        MST = GraphCreator.CreateMinimumSpanningTree(delaunator);
    }

    void OnDrawGizmos()
    {
        IPoint[] points = delaunator.Points;

        // Renders all edges of the Delauney Triangulation
        IEdge[] edges = delaunator.GetEdges().ToArray();
        foreach (IEdge edge in edges)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(edge.P.ToVector3(), edge.Q.ToVector3());
        }

        // Renders edges in the MST
        HashSet<Vector2Int> mstEdges = PrimsAlgo.ConvertAdjacencyListToSet(MST);
        foreach (Vector2Int edge in mstEdges)
        {
            Gizmos.color = Color.white;
            Vector3 startPoint = new Vector3((float)points[edge.x].X, (float)points[edge.x].Y);
            Vector3 endPoint = new Vector3((float)points[edge.y].X, (float)points[edge.y].Y);
            Gizmos.DrawLine(startPoint, endPoint);
        }

        // Renders the Points on the graph
        for (int i = 0; i < points.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3((float)points[i].X, (float)points[i].Y), .1f);
        }
        
    }
}
