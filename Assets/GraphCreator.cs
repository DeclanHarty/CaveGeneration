using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using UnityEngine;

public class GraphCreator
{
    public static IPoint[] CreatePoints(Vector2 topLeftCorner, Vector2 bottomRightCorner, int gridSize)
    {
        // Determines the size of each cell in the grid based on the gridSize
        float cellScale = Mathf.Abs(topLeftCorner.x - bottomRightCorner.x) / gridSize;
        IPoint[] points = new IPoint[gridSize * gridSize];
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                // Generates a random offset from the center of the cell with max absolute 
                // distance from center being 1/3 of a cell in each axis
                Vector2 randOffset = new Vector2(UnityEngine.Random.Range(-cellScale / 3, cellScale / 3), UnityEngine.Random.Range(-cellScale / 3, cellScale / 3));

                // Creates new point position for given cell x,y
                Vector2 pointPos = new Vector2(x * cellScale + +cellScale / 2 + randOffset.x + topLeftCorner.x, y * cellScale + cellScale / 2 + randOffset.y + bottomRightCorner.y);
                points[y * gridSize + x] = new Point(pointPos.x, pointPos.y);
            }
        }

        return points;
    }

    public static Delaunator CreateDelauneyTriangulation(IPoint[] points)
    {
        return new Delaunator(points);
    }

    public static List<List<int>> CreateMinimumSpanningTree(Delaunator delaunayTriangulation)
    {
        IEdge[] edges = delaunayTriangulation.GetEdges().ToArray();
        IPoint[] points = delaunayTriangulation.Points;

        List<Dictionary<int, double>> adjList = PrimsAlgo.CreateWeightedAdjacencyList(points, edges);
        List<List<int>> mst = PrimsAlgo.PrimMST(adjList);
        return mst;
    }

    public static HashSet<Vector2Int> CreateEdgeIndexSetFromDelaunayTriagnulation(Delaunator delaunayTriangulation)
    {
        HashSet<Vector2Int> edgeIndices = new HashSet<Vector2Int>();

        IEdge[] edges = delaunayTriangulation.GetEdges().ToArray();
        IPoint[] points = delaunayTriangulation.Points;

        Dictionary<IPoint, int> pointsToIndices = new Dictionary<IPoint, int>();

        for (int i = 0; i < points.Length; i++)
        {
            pointsToIndices[points[i]] = i;
        }

        foreach (IEdge edge in edges)
        {
            int startIndex = pointsToIndices[edge.P];
            int endIndex = pointsToIndices[edge.Q];

            if (startIndex <= endIndex)
            {
                edgeIndices.Add(new Vector2Int(startIndex, endIndex));
            }
            else
            {
                edgeIndices.Add(new Vector2Int(endIndex, startIndex));
            }
        }

        return edgeIndices;
    }
    


}
