using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public static Dictionary<Vector3Int, List<Vector3Int>> ValidDirLookupTable = new Dictionary<Vector3Int, List<Vector3Int>>()
    {
        {
            Direction.SOUTH, new List<Vector3Int>()
            {
                Direction.WEST, Direction.SOUTH_WEST, Direction.SOUTH, Direction.SOUTH_EAST, Direction.EAST
            }
        },
        {
            Direction.SOUTH_EAST, new List<Vector3Int>()
            {
                Direction.SOUTH, Direction.SOUTH_EAST, Direction.EAST
            }
        },
        {
            Direction.EAST, new List<Vector3Int>()
            {
                Direction.SOUTH, Direction.SOUTH_EAST, Direction.EAST, Direction.NORTH_EAST, Direction.NORTH
            }
        },
        {
            Direction.NORTH_EAST, new List<Vector3Int>()
            {
                Direction.EAST, Direction.NORTH_EAST, Direction.NORTH
            }
        },
        {
            Direction.NORTH, new List<Vector3Int>()
            {
                Direction.EAST, Direction.NORTH_EAST, Direction.NORTH, Direction.NORTH_WEST, Direction.WEST
            }
        },
        {
            Direction.NORTH_WEST, new List<Vector3Int>()
            {
                Direction.NORTH, Direction.NORTH_WEST, Direction.WEST
            }
        },
        {
            Direction.WEST, new List<Vector3Int>()
            {
                Direction.NORTH, Direction.NORTH_WEST, Direction.WEST, Direction.SOUTH_WEST, Direction.SOUTH
            }
        },
        {
            Direction.SOUTH_WEST, new List<Vector3Int>()
            {
                Direction.WEST, Direction.SOUTH_WEST, Direction.SOUTH
            }
        }
    };
}
