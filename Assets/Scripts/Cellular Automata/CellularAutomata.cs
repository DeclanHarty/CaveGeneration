using UnityEngine;
using UnityEngine.UIElements;

public class CellularAutomata
{
    static Vector2Int[] MOORES_NEIGHBORS = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, new Vector2Int(1, 1), new Vector2Int(-1, -1), new Vector2Int(1, -1), new Vector2Int(-1, 1) };

    public static int[,] RunGeneration(int[,] map)
    {
        int mapXLength = map.GetLength(0);
        int mapYLength = map.GetLength(1);
        int[,] newMap = new int[mapXLength, mapYLength];

        for (int x = 0; x < mapXLength; x++)
        {
            for (int y = 0; y < mapYLength; y++)
            {
                int numberOfWallNeighbors = GetNumberOfWallNeighbors(new Vector2Int(x, y), map);
                newMap[x, y] = CaveRuleset(map[x, y], numberOfWallNeighbors);
            }
        }

        return newMap;
    }

    public static int CaveRuleset(int currentState, int numberOfWallNeighbors)
    {
        if (currentState == 1 && numberOfWallNeighbors >= 4)
        {
            return 1;
        }
        else if (currentState == 0 && numberOfWallNeighbors >= 5)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public static int GetNumberOfWallNeighbors(Vector2Int center, int[,] map, bool borderIsWall = true, bool OOBIsWall = true)
    {
        int numberOfWallNeighbors = 0;
        int mapXLength = map.GetLength(0);
        int mapYLength = map.GetLength(1);
        foreach (Vector2Int direction in MOORES_NEIGHBORS)
        {
            Vector2Int positionToCheck = center + direction;
            // Check if position is OOB
            if (positionToCheck.x < 0 || positionToCheck.x >= mapXLength || positionToCheck.y < 0 || positionToCheck.y >= mapYLength)
            {
                if (OOBIsWall)
                {
                    if (borderIsWall)
                    {
                        return 8;
                    }
                    else
                    {
                        numberOfWallNeighbors++;
                    }
                    
                }
            }
            else
            {
                numberOfWallNeighbors += map[positionToCheck.x, positionToCheck.y];
            }
        }

        return numberOfWallNeighbors;
    }
}
