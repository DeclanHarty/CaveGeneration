using System.Collections;
using System.Collections.Generic;
using DelaunatorSharp;
using UnityEngine;

public class SimpleRoomCreator
{
    public static IEdge[] CreateSimpleCircularRoom(float minRadius, float maxRadius, int numberOfVertices)
    {
        IEdge[] edges = new IEdge[numberOfVertices];
        IPoint[] points = new IPoint[numberOfVertices];

        float degreesPerVertex = 2 * Mathf.PI / numberOfVertices;

        float radius = Random.Range(minRadius, maxRadius);
        Vector2 startingPosition = new Vector2(radius, 0);
        points[0] = new Point(startingPosition.x, startingPosition.y);

        for (int i = 1; i < numberOfVertices; i++)
        {
            float cos = Mathf.Cos(degreesPerVertex * i);
            float sin = Mathf.Sin(degreesPerVertex * i);

            radius = Random.Range(minRadius, maxRadius);
            startingPosition = new Vector2(radius, 0);

            Vector2 position = new Vector2(startingPosition.x * cos - startingPosition.y * sin, startingPosition.x * sin + startingPosition.y * cos);
            points[i] = new Point(position.x, position.y);

            edges[i - 1] = new Edge(i - 1, points[i - 1], points[i]);
        }

        edges[numberOfVertices - 1] = new Edge(numberOfVertices - 1, points[numberOfVertices - 1], points[0]);

        return edges;
    }
}
