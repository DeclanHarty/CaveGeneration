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
        for (int i = 0; i < points.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3((float)points[i].X, (float)points[i].Y), .1f);
        }

        IEdge[] edges = delaunator.GetEdges().ToArray();
        foreach (IEdge edge in edges)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(edge.P.ToVector3(), edge.Q.ToVector3());
        }

        HashSet<Vector2Int> renderedEdges = new HashSet<Vector2Int>();
        for (int startPointIndex = 0; startPointIndex < MST.Count; startPointIndex++)
            {
                foreach (int endPointIndex in MST[startPointIndex])
                {

                    if (!renderedEdges.Contains(new Vector2Int(startPointIndex, endPointIndex)))
                    {
                        Gizmos.color = Color.white;
                        Vector3 startPoint = new Vector3((float)points[startPointIndex].X, (float)points[startPointIndex].Y);
                        Vector3 endPoint = new Vector3((float)points[endPointIndex].X, (float)points[endPointIndex].Y);
                        Gizmos.DrawLine(startPoint, endPoint);

                        renderedEdges.Add(new Vector2Int(startPointIndex, endPointIndex));
                        renderedEdges.Add(new Vector2Int(endPointIndex, startPointIndex));
                    }

                }

            }
        
    }
}
