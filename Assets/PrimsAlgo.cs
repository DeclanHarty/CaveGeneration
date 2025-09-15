using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PrimsAlgo
{
    /*
    * Creates an 2d double array as an adjacency matrix using an array of points and edges. 
    */
    public static double[,] CreateWeightedAdjacencyMatrix(IPoint[] points, IEdge[] edges)
    {
        // Initialize adjacency matrix
        double[,] adjacencyMatrix = new double[points.Length, points.Length];

        Dictionary<IPoint, int> pointIndexDictionary = new Dictionary<IPoint, int>();

        for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
        {
            pointIndexDictionary.Add(points[pointIndex], pointIndex);
        }

        for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
        {
            for (int edgeIndex = 0; edgeIndex < edges.Length; edgeIndex++)
            {
                if (edges[edgeIndex].P == points[pointIndex])
                {
                    IPoint startPoint = points[pointIndex];
                    IPoint endPoint = edges[edgeIndex].Q;

                    double distance = Math.Sqrt(Math.Pow(startPoint.X - endPoint.X, 2) + Math.Pow(startPoint.Y - endPoint.Y, 2));

                    adjacencyMatrix[pointIndex, pointIndexDictionary[endPoint]] = distance;
                    adjacencyMatrix[pointIndexDictionary[endPoint], pointIndex] = distance;
                }
            }
        }

        return adjacencyMatrix;
    }

    public static List<Dictionary<int, double>> CreateWeightedAdjacencyList(IPoint[] points, IEdge[] edges)
    {
        // Weighted Adjacency List where the first List index indicates represents the index of the startPoint.
        // The int key represents the index of the endPoint and the double represents the distance.
        List<Dictionary<int, double>> adjacencyList = new List<Dictionary<int, double>>();

        for (int i = 0; i < points.Length; i++)
        {
            adjacencyList.Add(new Dictionary<int, double>());
        }


        Dictionary<IPoint, int> pointIndexDictionary = new Dictionary<IPoint, int>();

        for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
        {
            pointIndexDictionary.Add(points[pointIndex], pointIndex);
        }

        for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
        {
            for (int edgeIndex = 0; edgeIndex < edges.Length; edgeIndex++)
            {
                if (edges[edgeIndex].P == points[pointIndex])
                {
                    IPoint startPoint = points[pointIndex];
                    IPoint endPoint = edges[edgeIndex].Q;

                    double distance = Math.Sqrt(Math.Pow(startPoint.X - endPoint.X, 2) + Math.Pow(startPoint.Y - endPoint.Y, 2));
                    adjacencyList[pointIndex][pointIndexDictionary[endPoint]] = distance;
                    adjacencyList[pointIndexDictionary[endPoint]][pointIndex] = distance;
                }
            }
        }

        return adjacencyList;
    }

    public static int[,] PrimMST(double[,] adjacencyGraph)
    {
        // Create an adjacency matrix to store the MST
        int[,] treeAdjacencyGraph = new int[adjacencyGraph.GetLength(0), adjacencyGraph.GetLength(1)];
        // Initialize Hashset for storing visited points indices
        HashSet<int> visitedPoints = new HashSet<int>();
        // Decide on a starting index and add that index to the visitedSet
        int algorithmStartingPoint = 0; // UnityEngine.Random.Range(0, adjacencyGraph.GetLength(0));
        visitedPoints.Add(algorithmStartingPoint);

        while (true)
        {
            // indices off the points that make up the current shortest edge
            // init at -1 to flag if the shortest edge has not been updated
            int shortestEdgeStartPoint = -1;
            int shortestEdgeEndPoint = -1;
            double currentShortestDistance = double.MaxValue;
            foreach (int point in visitedPoints)
            {
                for (int endPointIndex = 0; endPointIndex < adjacencyGraph.GetLength(1); endPointIndex++)
                {
                    // Checks if there is an edge between these 2 points
                    if (adjacencyGraph[point, endPointIndex] == 0)
                    {
                        continue;
                    }

                    // Checks if the edge is the new shortest edge and if the point has already been visited before
                    if (adjacencyGraph[point, endPointIndex] < currentShortestDistance && !visitedPoints.Contains(endPointIndex))
                    {
                        // Update shortest edge
                        shortestEdgeStartPoint = point;
                        shortestEdgeEndPoint = endPointIndex;
                        currentShortestDistance = adjacencyGraph[point, endPointIndex];
                    }
                }
            }

            // If a valid shortest edge has not been found all points in the graph must have been added
            // and the algorithm is complete
            if (shortestEdgeStartPoint == -1 && shortestEdgeEndPoint == -1)
            {
                break;
            }

            // Otherwise add the new edge to the adjacency graph and add the point
            // to the visited points set
            treeAdjacencyGraph[shortestEdgeStartPoint, shortestEdgeEndPoint] = 1;
            treeAdjacencyGraph[shortestEdgeEndPoint, shortestEdgeStartPoint] = 1;
            visitedPoints.Add(shortestEdgeEndPoint);
        }

        return treeAdjacencyGraph;
    }

    // Performs an Prim's MST Algorithm
    // Takes in a weighted adjacency list and returns an
    // unweighted adjacency list
    public static List<List<int>> PrimMST(List<Dictionary<int, double>> weightedAdjacencyList)
    {
        // Create an adjacency List to store the MST
        List<List<int>> treeAdjacencyList = new List<List<int>>();
        for (int i = 0; i < weightedAdjacencyList.Count; i++)
        {
            treeAdjacencyList.Add(new List<int>());
        }
        // Initialize Hashset for storing visited points indices
        HashSet<int> visitedPoints = new HashSet<int>();
        // Decide on a starting index and add that index to the visitedSet
        int algorithmStartingPoint = 0; // UnityEngine.Random.Range(0, adjacencyGraph.GetLength(0));
        visitedPoints.Add(algorithmStartingPoint);

        while (true)
        {
            // indices off the points that make up the current shortest edge
            // init at -1 to flag if the shortest edge has not been updated
            int shortestEdgeStartPoint = -1;
            int shortestEdgeEndPoint = -1;
            double currentShortestDistance = double.MaxValue;
            foreach (int point in visitedPoints)
            {
                foreach (int endPointIndex in weightedAdjacencyList[point].Keys)
                {
                    // Checks if the edge is the new shortest edge and if the point has already been visited before
                    if (weightedAdjacencyList[point][endPointIndex] < currentShortestDistance && !visitedPoints.Contains(endPointIndex))
                    {
                        // Update shortest edge
                        shortestEdgeStartPoint = point;
                        shortestEdgeEndPoint = endPointIndex;
                        currentShortestDistance = weightedAdjacencyList[point][endPointIndex];
                    }
                }
            }

            // If a valid shortest edge has not been found all points in the graph must have been added
            // and the algorithm is complete
            if (shortestEdgeStartPoint == -1 && shortestEdgeEndPoint == -1)
            {
                break;
            }

            // Otherwise add the new edge to the adjacency list and add the point
            // to the visited points set
            treeAdjacencyList[shortestEdgeStartPoint].Add(shortestEdgeEndPoint);
            treeAdjacencyList[shortestEdgeEndPoint].Add(shortestEdgeStartPoint);
            visitedPoints.Add(shortestEdgeEndPoint);
        }

        return treeAdjacencyList;
    }

    public static HashSet<Vector2Int> ConvertAdjacencyListToSet(List<List<int>> adjacencyList) {
        HashSet<Vector2Int> edges = new HashSet<Vector2Int>();

        // Search through each starting vertex
        for (int startIndex = 0; startIndex < adjacencyList.Count; startIndex++)
        {
            // Search through each neighbor
            foreach (int endIndex in adjacencyList[startIndex])
            {
                Vector2Int edge = new Vector2Int(startIndex, endIndex);
                Vector2Int edgeMirror = new Vector2Int(endIndex, startIndex);

                if (edges.Contains(edge) || edges.Contains(edgeMirror))
                {
                    continue;
                }
                else
                {
                    edges.Add(edge);
                }
            }
        }

        return edges;
    }
}
