using static System.Array;
using System.Collections;
using System.Collections.Generic;
using DelaunatorSharp;
using UnityEngine;
using System.Linq;
using UnityEngine.Splines;
using DelaunatorSharp.Unity.Extensions;

public class SimpleRoomCreator
{
    public static IEdge[] CreateSimpleCircularRoom(float minRadius, float maxRadius, int numberOfVertices)
    {
        IEdge[] edges = new IEdge[numberOfVertices];
        IPoint[] points = new IPoint[numberOfVertices];

        // Get the degrees change between each point
        float degreesPerVertex = 2 * Mathf.PI / numberOfVertices;

        // Get Random radius of starting point and add the starting point
        float radius = Random.Range(minRadius, maxRadius);
        Vector2 startingPosition = new Vector2(radius, 0);
        points[0] = new Point(startingPosition.x, startingPosition.y);

        // Continue looping through the circle
        for (int i = 1; i < numberOfVertices; i++)
        {
            float cos = Mathf.Cos(degreesPerVertex * i);
            float sin = Mathf.Sin(degreesPerVertex * i);

            radius = Random.Range(minRadius, maxRadius);
            startingPosition = new Vector2(radius, 0);

            // Perform rotation translation vector [chosenRadius, 0]
            Vector2 position = new Vector2(startingPosition.x * cos - startingPosition.y * sin, startingPosition.x * sin + startingPosition.y * cos);
            points[i] = new Point(position.x, position.y);

            edges[i - 1] = new Edge(i - 1, points[i - 1], points[i]);
        }

        // Add edge to closing edge
        edges[numberOfVertices - 1] = new Edge(numberOfVertices - 1, points[numberOfVertices - 1], points[0]);

        return edges;
    }

    public static IEdge[] CreateSmoothCircularRoom(float minRadius, float maxRadius, int numberOfVertices, float splinePointPerUnit)
    {
        IEdge[] controlEdges = CreateSimpleCircularRoom(minRadius, maxRadius, numberOfVertices);

        Spline spline = new Spline();

        // Extract All Control Points
        foreach (IEdge edge in controlEdges)
        {
            BezierKnot knot = new BezierKnot(edge.P.ToVector3());
            spline.Add(knot, TangentMode.AutoSmooth);
        }

        float splineLength = spline.GetLength();

        // Calculate t per point on spline
        int numberOfSplinePoints = Mathf.RoundToInt(splinePointPerUnit * splineLength);
        float tPerVertex = 1f / numberOfSplinePoints;

        IPoint[] points = new IPoint[numberOfSplinePoints];
        // Get positions on spline
        for (int i = 0; i < numberOfSplinePoints; i++)
        {
            Vector3 splinePosition = SplineUtility.EvaluatePosition(spline, i * tPerVertex);
            IPoint point = new Point(splinePosition.x, splinePosition.y);

            points[i] = point;
        }

        // Package Spline Positions into Edges
        IEdge[] edges = new IEdge[numberOfSplinePoints];
        for (int i = 0; i < numberOfSplinePoints - 1; i++)
        {
            IEdge edge = new Edge(i, points[i], points[i + 1]);
            edges[i] = edge;
        }

        // Close Edges
        IEdge capEdge = new Edge(numberOfSplinePoints - 1, points[numberOfSplinePoints - 1], points[0]);
        edges[numberOfSplinePoints - 1] = capEdge;

        return edges;
    }

    public static IEdge[] TranslateRoom(IEdge[] edges, Vector2 translation)
    {
        IEdge[] translatedEdges = new IEdge[edges.Length];

        for (int i = 0; i < edges.Length; i++)
        {
            IEdge edge = edges[i];
            IPoint translatedP = new Point(edge.P.X + translation.x, edge.P.Y + translation.y);
            IPoint translatedQ = new Point(edge.Q.X + translation.x, edge.Q.Y + translation.y);
            IEdge translatedEdge = new Edge(edge.Index, translatedP, translatedQ);

            translatedEdges[i] = translatedEdge;
        }

        return translatedEdges;
    }
}
