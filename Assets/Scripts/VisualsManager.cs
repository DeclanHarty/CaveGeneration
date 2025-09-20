using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunatorSharp;
using System.Linq;
using DelaunatorSharp.Unity.Extensions;
using System;

public class VisualsManager : MonoBehaviour
{
    public bool renderGraph;
    public bool renderRoom;
    public GraphParams graphParams;
    public RoomParams roomParams;
    public ImageParams imageParams;

    private Delaunator delaunator;
    public float cellSize;
    private IPoint[] points;
    private HashSet<Vector2Int> MST = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> loopEdges;

    public List<IEdge[]> rooms;

    public void Start()
    {
        GenerateGraph();
        GenerateRooms();
        UpdateMap();

        Comparer<float> comparer = Comparer<float>.Default;
        Debug.Log(comparer.Compare(1, 2));
    }


    public void GenerateGraph()
    {
        cellSize = Mathf.Abs(graphParams.topLeftCorner.x - graphParams.bottomRightCorner.x) / graphParams.gridScale;

        // Creates a grid of points that are randomly offset
        points = GraphCreator.CreatePoints(graphParams.topLeftCorner, graphParams.bottomRightCorner, graphParams.gridScale);
        delaunator = GraphCreator.CreateDelauneyTriangulation(points);

        // Creates Minimum Spanning Tree
        List<List<int>> ListMST = GraphCreator.CreateMinimumSpanningTree(delaunator);
        MST = PrimsAlgo.ConvertAdjacencyListToSet(ListMST);

        // Gets edges from Delauney Triagnulation and Converts them to edges that correspond to the point indices
        HashSet<Vector2Int> edges = GraphCreator.CreateEdgeIndexSetFromDelaunayTriagnulation(delaunator);
        edges.ExceptWith(MST);
        loopEdges = new HashSet<Vector2Int>();

        int i = 0;
        while (i < graphParams.numberOfLoops && edges.Count() > 0)
        {
            Vector2Int[] edgesToSelectFrom = edges.ToArray();
            int randomIndex = UnityEngine.Random.Range(0, edgesToSelectFrom.Length);
            Vector2Int selectedEdge = edgesToSelectFrom[randomIndex];
            float edgeLength = Vector3.Distance(points[selectedEdge.x].ToVector3(), points[selectedEdge.y].ToVector3());
            if (edgeLength > graphParams.maxEdgeDistanceToCellSizeRatio * cellSize)
            {
                edges.Remove(selectedEdge);
                continue;
            }
            loopEdges.Add(selectedEdge);
            edges.Remove(selectedEdge);
            i++;
        }
    }

    // Creates Rooms
    public void GenerateRooms()
    {
        rooms = new List<IEdge[]>();
        for (int roomIndex = 0; roomIndex < graphParams.gridScale * graphParams.gridScale; roomIndex++)
        {
            IEdge[] room = SimpleRoomCreator.CreateSimpleCircularRoom(roomParams.minRadius * roomParams.roomScale, roomParams.maxRadius * roomParams.roomScale, roomParams.numberOfVertices);
            rooms.Add(room);
        }
    }

    public void UpdateMap()
    {
        Texture2D texture2D = new Texture2D(imageParams.imageResolution.x, imageParams.imageResolution.y);
        Color[] pixels = new Color[imageParams.imageResolution.x * imageParams.imageResolution.y];

        for (int i = 0; i < pixels.Length; i++)
        {

            pixels[i] = Color.gray;
        }

        
        for (int i = 0; i < rooms.Count; i++)
        {
            IEdge[] room = rooms[i];
            IEdge[] translatedRoom = SimpleRoomCreator.TranslateEdges(room, points[i].ToVector2());

            List<IEdge> roomToFill = new List<IEdge>(imageParams.EdgesFromWorldPosToImagePos(translatedRoom, graphParams.gridScale * cellSize));

            List<Vector2Int> raster = Scanline.PolygonFill(roomToFill);
            foreach (Vector2Int pixel in raster)
            {
                Vector2Int adjustedPoint = pixel;
                Debug.Log(adjustedPoint);
                int index = adjustedPoint.x + imageParams.imageResolution.x * adjustedPoint.y;
                if (index < 0 || index >= pixels.Length)
                {
                    continue;
                }
                pixels[adjustedPoint.x + imageParams.imageResolution.x * adjustedPoint.y] = Color.black;
            }
        }

        texture2D.SetPixels(pixels);
        texture2D.filterMode = FilterMode.Point;
        texture2D.wrapMode = TextureWrapMode.Clamp;
        texture2D.Apply();

        imageParams.renderer.sharedMaterial.mainTexture = texture2D;
    }

    void OnDrawGizmos()
    {
        if (renderRoom)
        {
            RenderRooms();
        }
        if (renderGraph)
        {
            RenderGraph();
        }

    }
    // Renders generated Rooms
    public void RenderRooms()
    {
        if (rooms == null)
        {
            return;
        }
        for (int roomIndex = 0; roomIndex < rooms.Count; roomIndex++)
        {
            foreach (IEdge edge in rooms[roomIndex])
            {
                Gizmos.color = Color.black;
                Gizmos.DrawLine(edge.P.ToVector3() + points[roomIndex].ToVector3(), edge.Q.ToVector3() + points[roomIndex].ToVector3());
            }
        }

    }

    public void RenderGraph()
    {
        IPoint[] points = delaunator?.Points;

        // Renders edges in the MST
        foreach (Vector2Int edge in MST)
        {
            Gizmos.color = Color.white;
            Vector3 startPoint = new Vector3((float)points[edge.x].X, (float)points[edge.x].Y);
            Vector3 endPoint = new Vector3((float)points[edge.y].X, (float)points[edge.y].Y);
            Gizmos.DrawLine(startPoint, endPoint);
        }

        // Renders all other edges of the Delauney Triangulation
        if (delaunator != null)
        {
            HashSet<Vector2Int> edgesP = GraphCreator.CreateEdgeIndexSetFromDelaunayTriagnulation(delaunator);
            edgesP.ExceptWith(MST);
            edgesP.ExceptWith(loopEdges);
            foreach (Vector2Int edge in edgesP)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawLine(points[edge.x].ToVector3(), points[edge.y].ToVector3());
            }
            // Renders the added Loops
            foreach (Vector2Int edge in loopEdges)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(points[edge.x].ToVector3(), points[edge.y].ToVector3());
            }
        }

        // Renders the Points on the graph
        if (points != null)
        {
            for (int i = 0; i < points.Length; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(new Vector3((float)points[i].X, (float)points[i].Y), .1f);
            }
        }
    }

    [System.Serializable]
    public class GraphParams
    {
        public Vector2 topLeftCorner;
        public Vector2 bottomRightCorner;
        public int gridScale;
        public int numberOfLoops;

        public float maxEdgeDistanceToCellSizeRatio;
    }

    [System.Serializable]
    public class RoomParams
    {
        public float minRadius;
        public float maxRadius;
        public int numberOfVertices;
        public float roomScale;
    }

    [System.Serializable]
    public class ImageParams
    {
        public Vector2Int imageResolution;
        public Vector2 imageSize;
        public Renderer renderer;

        public Vector2Int WorldPosToImagePos(Vector2 position, float gridRatio)
        {
            Vector2 translatedPosition = (position + new Vector2(imageSize.x / 2, imageSize.y / 2)) * (imageResolution.x / gridRatio);

            Vector2Int roundedPosition = new Vector2Int((int)Mathf.Round(translatedPosition.x), (int)Mathf.Round(translatedPosition.y));

            return roundedPosition;
        }

        public List<IEdge> EdgesFromWorldPosToImagePos(IEnumerable<IEdge> edges, float gridRatio)
        {
            List<IEdge> updatedEdges = new List<IEdge>();
            foreach (IEdge edge in edges)
            {
                IEdge updatedEdge;

                Point updatedP;
                Point updatedQ;

                Vector2Int updatedPPos = WorldPosToImagePos(edge.P.ToVector2(), gridRatio);
                Vector2Int updatedQPos = WorldPosToImagePos(edge.Q.ToVector2(), gridRatio);

                updatedP = new Point(updatedPPos.x, updatedPPos.y);
                updatedQ = new Point(updatedQPos.x, updatedQPos.y);

                updatedEdge = new Edge(0, updatedP, updatedQ);
                updatedEdges.Add(updatedEdge);
            }

            return updatedEdges;
        }
    }
}


