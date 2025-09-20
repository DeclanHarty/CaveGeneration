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

    public TunnelCreator tunnelCreator;

    private Delaunator delaunator;
    public float cellSize;
    private IPoint[] points;
    private HashSet<Vector2Int> MST;
    private HashSet<Vector2Int> loopEdges;

    public List<IEdge[]> rooms;
    List<IEdge[]> tunnels;

    public void Start()
    {
        GenerateGraph();
        GenerateRooms();
        UpdateMap();
        GenerateTunnels();
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

    // Creates Tunnels
    public void GenerateTunnels()
    {
        
    }

    public void UpdateMap()
    {
        Texture2D texture2D = new Texture2D(imageParams.imageResolution.x, imageParams.imageResolution.y);
        Color[] pixels = new Color[imageParams.imageResolution.x * imageParams.imageResolution.y];

        // Sets the image to all gray to start
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.gray;
        }

        // Colors each room black
        for (int i = 0; i < rooms.Count; i++)
        {
            IEdge[] room = rooms[i];
            IEdge[] translatedRoom = SimpleRoomCreator.TranslateRoom(room, points[i].ToVector2());

            List<IEdge> roomToFill = new List<IEdge>(imageParams.EdgesFromWorldPosToImagePos(translatedRoom, graphParams.gridScale * cellSize));

            List<Vector2Int> raster = Scanline.PolygonFill(roomToFill);

            foreach (Vector2Int pixel in raster)
            {
                // Check if pixel is outside image
                // Skip if it is
                if (pixel.x < 0 || pixel.x >= imageParams.imageResolution.x || pixel.y < 0 || pixel.y >= imageParams.imageResolution.y)
                {
                    continue;
                }
                // Calculate index in pixel array that corresponds to pixel position
                int index = pixel.x + imageParams.imageResolution.x * pixel.y;
                pixels[index] = Color.black;
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
        RenderTunnelTest();
    }
    // Render Tunnel
    public void RenderTunnelTest()
    {
        
        // foreach (IEdge edge in tunnel)
        // {
        //     Gizmos.color = Color.black;
        //     Gizmos.DrawLine(edge.P.ToVector3(), edge.Q.ToVector3());
        // }
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

        IEdge[] MSTAsEdges = GraphCreator.ConvertIndexRepresentationToEdges(points, MST.ToArray());

        // Renders edges in the MST
        foreach (IEdge edge in MSTAsEdges)
        {
            Gizmos.color = Color.white;
            Vector3 startPoint = new Vector3((float)edge.P.X, (float)edge.P.Y);
            Vector3 endPoint = new Vector3((float)edge.Q.X, (float)edge.Q.Y);
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
            IEdge[] loopEdgesAsEdges = GraphCreator.ConvertIndexRepresentationToEdges(points, loopEdges.ToArray());
            foreach (IEdge edge in loopEdgesAsEdges)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(edge.P.ToVector3(), edge.Q.ToVector3());
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
            Vector2 translatedPosition = position + new Vector2(imageSize.x / 2, imageSize.y / 2);
            float lerpX = Mathf.Lerp(0, imageResolution.x - 1, translatedPosition.x / imageSize.x);
            float lerpY = Mathf.Lerp(0, imageResolution.y - 1, translatedPosition.y / imageSize.y);

            Vector2 scaledPosition = new Vector2(lerpX, lerpY);

            Vector2Int roundedPosition = new Vector2Int((int)Mathf.Round(scaledPosition.x), (int)Mathf.Round(scaledPosition.y));

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


