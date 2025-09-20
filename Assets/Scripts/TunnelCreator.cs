using System.Collections;
using System.Collections.Generic;
using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using UnityEngine;

[System.Serializable]
public class TunnelCreator
{
    public int resolutionPerUnit;
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
                float randX = Random.Range(-randomDistributionRadius, randomDistributionRadius);
                float randY = Random.Range(-randomDistributionRadius, randomDistributionRadius);
                float distance = Mathf.Sqrt(Mathf.Pow(randX, 2) + Mathf.Pow(randY, 2));
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
        
        IEdge[] tunnel = new IEdge[points.Count - 1];

        for (int i = 0; i < points.Count - 1; i++)
        {
            tunnel[i] = new Edge(i, points[i], points[i + 1]);
        }

        return tunnel;
    }
}
