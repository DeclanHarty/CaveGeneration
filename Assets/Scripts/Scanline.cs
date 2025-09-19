using System;
using System.Collections;
using System.Collections.Generic;
using DelaunatorSharp;
using Unity.VisualScripting;
using UnityEngine;

public static class Scanline
{
    /* Creates a List<float[]> where each entry represents the properties of an edge
    *  The float[] is 4 values : [min Y, max Y, X at min Y, slope_inv]
    *  Horizontal Edges are skipped Verticle edges are given float.PositiveInfinity
    */
    public static List<float[]> CreateEdgeTable(List<IEdge> edges)
    {
        // Initialize Edge Table
        List<float[]> edgeTable = new List<float[]>(edges.Count);

        foreach (IEdge edge in edges)
        {
            float[] edgeTableRecord = new float[4];

            float slope_inv;

            // Horizontal Edges are Skipped Over
            if (edge.P.Y == edge.Q.Y)
            {
                continue;
            }

            // If edge is vertical set slope_inv to 
            if (edge.P.X == edge.Q.X)
            {
                slope_inv = 0;
            }
            else if (edge.P.X < edge.Q.X)
            {
                float rise = (float)(edge.Q.Y - edge.P.Y);
                float run = (float)(edge.Q.X - edge.P.X);
                slope_inv = run / rise;
            }
            else
            {
                float rise = (float)(edge.P.Y - edge.Q.Y);
                float run = (float)(edge.P.X - edge.Q.X);
                slope_inv = run / rise;

            }

            edgeTableRecord[3] = slope_inv;

            if (edge.P.Y < edge.Q.Y)
            {
                edgeTableRecord[0] = (float)edge.P.Y;
                edgeTableRecord[1] = (float)edge.Q.Y;
                edgeTableRecord[2] = (float)edge.P.X;
            }
            else
            {
                edgeTableRecord[0] = (float)edge.Q.Y;
                edgeTableRecord[1] = (float)edge.P.Y;
                edgeTableRecord[2] = (float)edge.Q.X;
            }

            edgeTable.Add(edgeTableRecord);

        }

        return edgeTable;
    }

    public static void SortEdgeTable(List<float[]> edgeTable)
    {
        Comparer<float> comparer = Comparer<float>.Default;

        edgeTable.Sort((x, y) =>
        {
            int sort_value = comparer.Compare(x[0], y[0]);
            if (sort_value != 0)
            {
                return sort_value;
            }
            else
            {
                return comparer.Compare(x[2], y[2]);
            }
        });
    }
    // Updates both the global and active tables
    // Global Table is made of float[4] = {minY, maxY, X at minY, slope_inv} 
    // Active Table is made of float[3] = {maxY, currentX, slope_inv}
    public static void UpdateScanline(float y_value, List<float[]> globalTable, List<float[]> activeTable)
    {
        Debug.Log(activeTable.Count);
        int globalTableLength = globalTable.Count;
        int activeTableLength = activeTable.Count;
        List<float[]> globalCopy = new List<float[]>();

        List<float[]> activeTableEdgesToRemove = new List<float[]>();

        foreach (float[] edge in activeTable)
        {
            // Remove Edge if Scanline has reached the end of the edge ie. maxY is <= scanline value
            if ((int)Mathf.Floor(edge[0]) <= y_value)
            {
                activeTableEdgesToRemove.Add(edge);
            }
            else
            {
                // Otherwise update x-value by slope
                edge[1] += edge[2];
            }
        }

        foreach (float[] edge in activeTableEdgesToRemove)
        {
            activeTable.Remove(edge);
        }

        List<float[]> globalTableEdgesToRemove = new List<float[]>();

        foreach (float[] edge in globalTable)
        {
            // If globalEdge is intersecting scanline add it to the active table and remove it from the
            // globalTable
            if ((int)Mathf.Floor(edge[0]) <= y_value)
            {
                // Remove minY and add edge to activeTable
                activeTable.Add(new float[] { edge[1], edge[2], edge[3] });
                globalTableEdgesToRemove.Add(edge);
            }
        }

        foreach (float[] edge in globalTableEdgesToRemove)
        {
            globalTable.Remove(edge);
        }

        // Sorts the active table edges by x-value;
        activeTable.Sort((x, y) => Comparer<float>.Default.Compare(x[1], y[1]));
    }

    public static List<Vector2Int> PolygonFill(List<IEdge> edges)
    {
        List<Vector2Int> raster = new List<Vector2Int>();

        // Create and Sort Edge Table from edges
        List<float[]> globalTable = CreateEdgeTable(edges);
        SortEdgeTable(globalTable);

        // Init Active Table
        List<float[]> activeTable = new List<float[]>();

        // Set the scanline to min Y value
        int scanline_y = (int)Mathf.Floor(globalTable[0][0]);

        // Init Scanline
        UpdateScanline(scanline_y, globalTable, activeTable);

        while(activeTable.Count > 0){
            for (int i = 0; i < activeTable.Count; i += 2) {
                if (i >= activeTable.Count || i + 1 >= activeTable.Count) {
                    activeTable.Clear();
                    break;
                }
                int endValue = (int)Mathf.Round(Mathf.Max(activeTable[i][1], activeTable[i + 1][1]));
                int startValue = (int)Mathf.Round(Mathf.Min(activeTable[i][1], activeTable[i + 1][1]));
                for(int x = startValue; x < endValue; x++){
                    raster.Add(new Vector2Int(x, scanline_y));
                }  
                
            }

            scanline_y++;
            UpdateScanline(scanline_y, globalTable, activeTable);
        }

        return raster;
    }


}
