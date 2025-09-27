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
    public float minTunnelThickness;
    public float maxTunnelThickness;
    public List<IEdge[]> CreateTunnel(IEdge edge)
    {
        List<IPoint> points = new List<IPoint>();

        float edgeLength = Vector2.Distance(edge.P.ToVector2(), edge.Q.ToVector2());
        // The number of points along the tunnel not including the start and end
        int numberOfExtraPoints = Mathf.RoundToInt(resolutionPerUnit * edgeLength);
        Vector2 distanceBetweenEachPoint = (edge.Q.ToVector2() - edge.P.ToVector2()) / numberOfExtraPoints;

        // Add starting position
        points.Add(edge.P);

        for (int i = 1; i < numberOfExtraPoints; i++)
        {
            // Vertex position before displacement
            Vector2 center = new Vector2((float)edge.P.X + distanceBetweenEachPoint.x * i, (float)edge.P.Y + distanceBetweenEachPoint.y * i);

            // Get Displacement
            Vector2 displacement = GetRandomNormalDisplacement(edge.P.ToVector2() - edge.Q.ToVector2(), randomDistributionRadius);


            IPoint newPoint = new Point(center.x + displacement.x, center.y + displacement.y);
            points.Add(newPoint);
        }

        // Add End position
        points.Add(edge.Q);

        // Initialize Baseline array
        IEdge[] baseLine = new IEdge[points.Count - 1];

        // Package points into edges
        for (int i = 0; i < points.Count - 1; i++)
        {
            baseLine[i] = new Edge(i, points[i], points[i + 1]);
        }

        // Convert baseline into spline
        Spline spline = ConvertBaseLineToSpline(baseLine);
        // Create polygon from spline
        List<IEdge[]> thickSpline = CreatePolygonLine(spline);

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
    }

    public List<IEdge[]> CreatePolygonLine(Spline spline)
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
            tangents[i] = tangent.normalized;
        }

        Vector3 firstTangent = SplineUtility.EvaluateTangent(spline, 0.0001f);
        tangents[0] = firstTangent.normalized;

        List<IEdge[]> tunnelSegments = new List<IEdge[]>();

        float[] thicknessAtPoint = new float[points.Length];
        // Create the thickness 
        for (int i = 0; i < points.Length; i++)
        {
            thicknessAtPoint[i] = Random.Range(minTunnelThickness, maxTunnelThickness);
        }

        // Create the two "Parallel Lines"
        for (int i = 0; i < points.Length - 1; i++)
        {
            IEdge[] tunnelSegment = new IEdge[4];

            IPoint startPoint = points[i];
            IPoint endPoint = points[i + 1];

            Vector2 startPointOrthag = Vector2.Perpendicular((Vector2)tangents[i]);
            Vector2 startPointPosDisplace = startPoint.ToVector2() + thicknessAtPoint[i] * startPointOrthag;
            Vector2 startPointNegDisplace = startPoint.ToVector2() - thicknessAtPoint[i] * startPointOrthag;

            Vector2 endPointOrthag = Vector2.Perpendicular((Vector2)tangents[i + 1]);
            Vector2 endPointPosDisplace = endPoint.ToVector2() + thicknessAtPoint[i+1] * endPointOrthag;
            Vector2 endPointNegDisplace = endPoint.ToVector2() - thicknessAtPoint[i+1] * endPointOrthag;

            IPoint startPointPos = new Point(startPointPosDisplace.x, startPointPosDisplace.y);
            IPoint endPointPos = new Point(endPointPosDisplace.x, endPointPosDisplace.y);
            IPoint startPointNeg = new Point(startPointNegDisplace.x, startPointNegDisplace.y);
            IPoint endPointNeg = new Point(endPointNegDisplace.x, endPointNegDisplace.y);

            tunnelSegment[0] = new Edge(0, startPointPos, endPointPos);
            tunnelSegment[1] = new Edge(1, startPointNeg, endPointNeg);
            tunnelSegment[2] = new Edge(1, startPointNeg, startPointPos);
            tunnelSegment[3] = new Edge(1, endPointNeg, endPointPos);


            tunnelSegments.Add(tunnelSegment);
        }
       

        return tunnelSegments;
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
