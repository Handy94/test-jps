using UnityEngine;

public class Map
{
    public Node[,] map;

    public int Width => (map == null) ? 0 : map.GetLength(1);
    public int Height => (map == null) ? 0 : map.GetLength(0);

    public void GenerateRandomMap(int width, int height)
    {
        map = new Node[height, width];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                map[i, j] = new Node()
                {
                    x = j,
                    y = i,
                    givenCost = 0f,
                    isObstacle = Random.Range(0, 100) < 50
                };
            }
        }
    }

    public Node GetNode(int x, int y)
    {
        if (x < 0 || x >= Width) return null;
        if (y < 0 || y >= Height) return null;

        return map[y, x];
    }

    public void Reset()
    {
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                map[i, j]?.Reset();
            }
        }
    }
}
