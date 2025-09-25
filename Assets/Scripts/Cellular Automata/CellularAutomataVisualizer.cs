using UnityEngine;

public class CellularAutomataVisualizer : MonoBehaviour
{
    public Vector2Int mapSize;
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
                pixels[x + mapSize.x * y] = map[x, y] == 1 ? Color.black : Color.white;
            }
        }

        texture2D.SetPixels(pixels);
        texture2D.filterMode = FilterMode.Point;
        texture2D.wrapMode = TextureWrapMode.Clamp;
        texture2D.Apply();

        renderer.sharedMaterial.mainTexture = texture2D;

    }
}
