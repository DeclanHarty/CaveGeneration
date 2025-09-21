using System.Collections;
using System.Collections.Generic;
using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Splines;

[System.Serializable]
public class TunnelCreator
{
    public int resolutionPerUnit;
    public float splineUpscale;
    public float randomDistributionRadius;
    public IEdge[] CreateTunnel(IEdge edge)
    {
        List<IPoint> points = new List<IPoint>();

        float edgeLength = Vector2.Distance(edge.P.ToVector2(), edge.Q.ToVector2());
        // The number of points along the tunnel not including the start and end
        int numberOfExtraPoints = Mathf.RoundToInt(resolutionPerUnit * edgeLength);
        Vector2 distanceBetweenEachPoint = (edge.Q.ToVector2() - edge.P.ToVector2()) / numberOfExtraPoints;

        for (int i = 0; i < numberOfExtraPoints; i++)
        {
            Vector2 center = new Vector2((float)edge.P.X + distanceBetweenEachPoint.x * i, (float)edge.P.Y + distanceBetweenEachPoint.y * i);

            Vector2 displacement;
            while (true)
            {
                // Calculate the random displacement for the point
                float randX = Random.Range(-randomDistributionRadius, randomDistributionRadius);
                float randY = Random.Range(-randomDistributionRadius, randomDistributionRadius);
                float distance = Mathf.Sqrt(Mathf.Pow(randX, 2) + Mathf.Pow(randY, 2));
                // If point is inside radius accept otherwise reject and repeat
                if (distance <= randomDistributionRadius)
                {
                    displacement = new Vector2(randX, randY);
                    break;
                }

            }



            IPoint newPoint = new Point(center.x + displacement.x, center.y + displacement.y);
            points.Add(newPoint);
        }

        points.Add(edge.Q);

        foreach (IPoint point in points)
        {
            Debug.Log(point.ToVector2());
        }

        IEdge[] baseLine = new IEdge[points.Count - 1];

        for (int i = 0; i < points.Count - 1; i++)
        {
            baseLine[i] = new Edge(i, points[i], points[i + 1]);
        }

        IEdge[] spline = ConvertBaseLineToSpline(baseLine);
        
        return spline;
    }

    public IEdge[] ConvertBaseLineToSpline(IEdge[] edges)
    {

        Vector3 startingPoint = edges[0].P.ToVector3();
        BezierKnot startingKnot = new BezierKnot(startingPoint);
        Spline spline = new Spline(new BezierKnot[] { startingKnot });

        spline.Add(startingKnot);

        foreach (IEdge edge in edges)
        {
            Vector3 point = edge.Q.ToVector3();
            BezierKnot knot = new BezierKnot(point);
            spline.Add(knot, TangentMode.AutoSmooth);
        }

        float tunnelLength = spline.GetLength();

        int numberOfVertices = Mathf.RoundToInt(resolutionPerUnit * splineUpscale * tunnelLength);
        float tPerVertex = 1f / (float)numberOfVertices;

        // Get Points from the spline
        IPoint[] points = new IPoint[numberOfVertices + 1];
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 position = SplineUtility.EvaluatePosition(spline, i * tPerVertex);
            IPoint point = new Point(position.x, position.y);
            points[i] = point;
        }

        foreach (IPoint point in points)
        {
            Debug.Log(point.ToVector2());
        }

        IEdge[] newEdges = new IEdge[points.Length - 1];
        // Package Points into edges
        for (int i = 0; i < points.Length - 1; i++)
        {
            IEdge edge = new Edge(i, points[i], points[i + 1]);
            newEdges[i] = edge;
        }

        return newEdges;
        
    }
}
