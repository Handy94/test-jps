using System.Collections.Generic;
using UnityEngine;

public static class Direction
{
    public static Vector3Int NORTH = new Vector3Int(0, 1, 0);
    public static Vector3Int SOUTH = new Vector3Int(0, -1, 0);

    public static Vector3Int WEST = new Vector3Int(-1, 0, 0);
    public static Vector3Int EAST = new Vector3Int(1, 0, 0);

    public static Vector3Int NORTH_WEST = new Vector3Int(-1, 1, 0);
    public static Vector3Int NORTH_EAST = new Vector3Int(1, 1, 0);

    public static Vector3Int SOUTH_WEST = new Vector3Int(-1, -1, 0);
    public static Vector3Int SOUTH_EAST = new Vector3Int(1, -1, 0);

    public static List<Vector3Int> CardinalDirections = new List<Vector3Int>()
    {
        NORTH, SOUTH, WEST, EAST
    };

    public static List<Vector3Int> DiagonalDirections = new List<Vector3Int>()
    {
        NORTH_WEST, NORTH_EAST, SOUTH_WEST, SOUTH_EAST
    };

    public static bool IsCardinal(Vector3Int direction)
    {
        return CardinalDirections.Contains(direction);
    }

    public static bool IsDiagonal(Vector3Int direction)
    {
        return DiagonalDirections.Contains(direction);
    }

    public static Vector3Int GetHorizontalCardinalDirections(Vector3Int direction)
    {
        if (direction.x > 0) return Direction.EAST;
        else if (direction.x < 0) return Direction.WEST;
        return Vector3Int.zero;
    }

    public static Vector3Int GetVerticalCardinalDirections(Vector3Int direction)
    {
        if (direction.y > 0) return Direction.NORTH;
        else if (direction.y < 0) return Direction.SOUTH;
        return Vector3Int.zero;
    }
}
