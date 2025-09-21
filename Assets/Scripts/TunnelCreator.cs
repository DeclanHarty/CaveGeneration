using System.Collections;
using System.Collections.Generic;
using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using UnityEngine;
using UnityEngine.Splines;

[System.Serializable]
public class TunnelCreator
{
    public int resolutionPerUnit;
    public float splineUpscale;
    public float randomDistributionRadius;
    public float tunnelThickness;
    public IEdge[] CreateTunnel(IEdge edge)
    {
        List<IPoint> points = new List<IPoint>();

        float edgeLength = Vector2.Distance(edge.P.ToVector2(), edge.Q.ToVector2());
        // The number of points along the tunnel not including the start and end
        int numberOfExtraPoints = Mathf.RoundToInt(resolutionPerUnit * edgeLength);
        Vector2 distanceBetweenEachPoint = (edge.Q.ToVector2() - edge.P.ToVector2()) / numberOfExtraPoints;

        for (int i = 0; i < numberOfExtraPoints; i++)
        {
            // Vertex position before displacement
            Vector2 center = new Vector2((float)edge.P.X + distanceBetweenEachPoint.x * i, (float)edge.P.Y + distanceBetweenEachPoint.y * i);

            // Get Displacement
            Vector2 displacement = GetRandomNormalDisplacement(edge.P.ToVector2() - edge.Q.ToVector2(), randomDistributionRadius);


            IPoint newPoint = new Point(center.x + displacement.x, center.y + displacement.y);
            points.Add(newPoint);
        }

        points.Add(edge.Q);

        IEdge[] baseLine = new IEdge[points.Count - 1];

        for (int i = 0; i < points.Count - 1; i++)
        {
            baseLine[i] = new Edge(i, points[i], points[i + 1]);
        }

        Spline spline = ConvertBaseLineToSpline(baseLine);
        IEdge[] thickSpline = CreatePolygonLine(spline);

        return thickSpline;
    }

    public Spline ConvertBaseLineToSpline(IEdge[] edges)
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

        return spline;

        // float tunnelLength = spline.GetLength();

        // int numberOfVertices = Mathf.RoundToInt(resolutionPerUnit * splineUpscale * tunnelLength);
        // float tPerVertex = 1f / (float)numberOfVertices;

        // // Get Points and Tangents from the spline
        // IPoint[] points = new IPoint[numberOfVertices + 1];
        // for (int i = 0; i < points.Length; i++)
        // {
        //     Vector3 position = SplineUtility.EvaluatePosition(spline, i * tPerVertex);
        //     IPoint point = new Point(position.x, position.y);
        //     points[i] = point;
        // }

        // IEdge[] newEdges = new IEdge[points.Length - 1];
        // // Package Points into edges
        // for (int i = 0; i < points.Length - 1; i++)
        // {
        //     IEdge edge = new Edge(i, points[i], points[i + 1]);
        //     newEdges[i] = edge;
        // }

        // return newEdges;

    }

    public IEdge[] CreatePolygonLine(Spline spline)
    {
        float tunnelLength = spline.GetLength();

        int numberOfVertices = Mathf.RoundToInt(resolutionPerUnit * splineUpscale * tunnelLength);
        float tPerVertex = 1f / (float)numberOfVertices;

        // Get Points and Tangents from the spline
        IPoint[] points = new IPoint[numberOfVertices + 1];
        Vector3[] tangents = new Vector3[numberOfVertices + 1];
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 position = SplineUtility.EvaluatePosition(spline, i * tPerVertex);
            Vector3 tangent = SplineUtility.EvaluateTangent(spline, i * tPerVertex);
            IPoint point = new Point(position.x, position.y);
            points[i] = point;
            tangents[i] = tangent;
        }

        IEdge[] edges = new IEdge[2 * points.Length];

        // Create the two "Parallel Lines"
        for (int i = 0; i < points.Length - 1; i++)
        {
            IPoint startPoint = points[i];
            IPoint endPoint = points[i + 1];

            Vector2 startPointOrthag = Vector2.Perpendicular((Vector2)tangents[i]);
            Vector2 startPointPosDisplace = startPoint.ToVector2() + tunnelThickness * startPointOrthag;
            Vector2 startPointNegDisplace = startPoint.ToVector2() - tunnelThickness * startPointOrthag;

            Vector2 endPointOrthag = Vector2.Perpendicular((Vector2)tangents[i + 1]);
            Vector2 endPointPosDisplace = endPoint.ToVector2() + tunnelThickness * endPointOrthag;
            Vector2 endPointNegDisplace = endPoint.ToVector2() - tunnelThickness * endPointOrthag;

            edges[2 * i] = new Edge(2 * i, new Point(startPointPosDisplace.x, startPointPosDisplace.y), new Point(endPointPosDisplace.x, endPointPosDisplace.y));
            edges[2 * i + 1] = new Edge(2 * i, new Point(startPointNegDisplace.x, startPointNegDisplace.y), new Point(endPointNegDisplace.x, endPointNegDisplace.y));
        }

        // Cap them off
        edges[edges.Length - 2] = new Edge(edges.Length - 2, edges[0].P, edges[1].P);
        edges[edges.Length - 1] = new Edge(edges.Length - 1, edges[edges.Length - 4].Q, edges[edges.Length - 3].Q);

        return edges;
    }

    public Vector2 GetRandomPositionInCircle(float randomDistributionRadius)
    {
        Vector2 position;
        while (true)
        {
            // Calculate the random displacement for the point
            float randX = Random.Range(-randomDistributionRadius, randomDistributionRadius);
            float randY = Random.Range(-randomDistributionRadius, randomDistributionRadius);
            float distance = Mathf.Sqrt(Mathf.Pow(randX, 2) + Mathf.Pow(randY, 2));
            // If point is inside radius accept otherwise reject and repeat
            if (distance <= randomDistributionRadius)
            {
                return position = new Vector2(randX, randY);
            }

        }
    }

    public Vector2 GetRandomNormalDisplacement(Vector2 tangentDirection, float maxNormalDisplacement)
    {
        float displacement = Random.Range(-maxNormalDisplacement, maxNormalDisplacement);

        Vector2 normalDirection = Vector2.Perpendicular(tangentDirection.normalized);

        return displacement * normalDirection;
    }
}
