using System.Collections.Generic;
using UnityEngine;

public class JumpPoint
{
    public List<Vector3Int> directions = new List<Vector3Int>();
    public List<Node> forcedNeighbours = new List<Node>();

    public void AddDirection(Vector3Int dir)
    {
        if (directions.Contains(dir)) return;
        directions.Add(dir);
    }

    public void AddForcedNeighbours(Node node)
    {
        if (forcedNeighbours.Contains(node)) return;
        forcedNeighbours.Add(node);
    }

    public bool ContainDirection(Vector3Int dir)
    {
        return directions.Contains(dir);
    }
}
