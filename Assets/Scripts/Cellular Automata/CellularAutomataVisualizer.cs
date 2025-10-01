using UnityEngine;

public class CellularAutomataVisualizer : MonoBehaviour
{
    public Vector2Int mapSize;
    public Vector2 imageSize;
    [Range(0, 1)]
    public float cutoff;
    public Renderer renderer;

    private int[,] map;





    public void Start()
    {
        CreateMap();
    }

    public void CreateMap()
    {
        map = StaticNoiseGenerator.GenerateStaticBWNoise(mapSize.x, mapSize.y, cutoff);
        UpdateImage();
    }

    public void SetMap(int[,] map, Vector2Int mapSize)
    {
        this.map = map;
        this.mapSize = mapSize;
        UpdateImage();
    }
    public void SetMap(int[] map, Vector2Int mapSize)
    {
        int[,] TwoDMap = new int[mapSize.x, mapSize.y];
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                TwoDMap[x, y] = map[x + mapSize.x * y];
            }
        }
        this.map = TwoDMap;
        this.mapSize = mapSize;
        UpdateImage();
    }

    public void PerformCA()
    {
        map = CellularAutomata.RunGeneration(map);
        UpdateImage();
    }

    public void UpdateImage()
    {
        Texture2D texture2D = new Texture2D(mapSize.x, mapSize.y);
        Color[] pixels = new Color[mapSize.x * mapSize.y];

        if (map == null)
        {
            return;
        }

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                pixels[x + mapSize.x * y] = map[x, y] == (int)TileType.WALL ? Color.black : Color.white;
            }
        }

        texture2D.SetPixels(pixels);
        texture2D.filterMode = FilterMode.Point;
        texture2D.wrapMode = TextureWrapMode.Clamp;
        texture2D.Apply();

        renderer.sharedMaterial.mainTexture = texture2D;

    }

    public Vector2Int WorldPosToMapPos(Vector2 position)
    {
        Vector2 translatedPosition = position + new Vector2(imageSize.x / 2, imageSize.y / 2);
        float lerpX = Mathf.LerpUnclamped(0, mapSize.x, translatedPosition.x / imageSize.x);
        float lerpY = Mathf.LerpUnclamped(0, mapSize.y, translatedPosition.y / imageSize.y);

        Vector2 scaledPosition = new Vector2(lerpX, lerpY);
        Vector2Int roundedPosition = new Vector2Int((int)Mathf.Floor(scaledPosition.x), (int)Mathf.Floor(scaledPosition.y));

        return roundedPosition;
    }

    public void PaintPosition(Vector2 worldPosition, TileType tileType)
    {
        Vector2Int mapPosition = WorldPosToMapPos(worldPosition);

        if (mapPosition.x < 0 || mapPosition.x >= mapSize.x || mapPosition.y < 0 || mapPosition.y >= mapSize.y)
        {
            return;
        }
        else
        {
            if (map[mapPosition.x, mapPosition.y] != (int)tileType)
            {
                map[mapPosition.x, mapPosition.y] = (int)tileType;
                UpdateImage();
            }
        }
    }

    public int[,] GetMap()
    {
        return map;
    }

    public enum TileType : int
    {
        WALL = 1,
        EMPTY = 0
    }
}
