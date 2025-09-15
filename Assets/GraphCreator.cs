using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using UnityEngine;

public class GraphCreator
{
    public static IPoint[] CreatePoints(Vector2 topLeftCorner, Vector2 bottomRightCorner, int gridScale)
    {
        float cellScale = Mathf.Abs(topLeftCorner.x - bottomRightCorner.x) / gridScale;
        IPoint[] points = new IPoint[gridScale * gridScale];
        for (int y = 0; y < gridScale; y++)
        {
            for (int x = 0; x < gridScale; x++)
            {
                Vector2 randOffset = new Vector2(UnityEngine.Random.Range(0f, cellScale), UnityEngine.Random.Range(0f, cellScale));

                Vector2 pointPos = new Vector2(x * cellScale + randOffset.x + topLeftCorner.x, y * cellScale + randOffset.y + bottomRightCorner.y);
                points[y * gridScale + x] = new Point(pointPos.x, pointPos.y);
            }
        }

        return points;
    }

    public static Delaunator CreateDelauneyTriangulation(IPoint[] points)
    {
        return new Delaunator(points);
    }

    public static List<List<int>> CreateMinimumSpanningTree(Delaunator delaunyTriangulation)
    {
        IEdge[] edges = delaunyTriangulation.GetEdges().ToArray();
        IPoint[] points = delaunyTriangulation.Points;

        List<Dictionary<int, double>> adjList = PrimsAlgo.CreateWeightedAdjacencyList(points, edges);

        List<List<int>> mst = PrimsAlgo.PrimMST(adjList);
        return mst;
    }

}
