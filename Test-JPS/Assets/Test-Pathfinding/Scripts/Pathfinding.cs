using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public Map map = new Map();
    public Dictionary<Node, Dictionary<Vector3Int, int>> distances = new Dictionary<Node, Dictionary<Vector3Int, int>>();
    public Dictionary<Node, JumpPoint> jumpPointsData = new Dictionary<Node, JumpPoint>();

    public bool isMapScanned = false;
    public PathMode pathMode = PathMode.Path_Dijkstra;

    public void GenerateRandomMap(int width, int height)
    {
        map.GenerateRandomMap(width, height);
        isMapScanned = false;
    }

    public void GenerateMap(string mapData)
    {
        string[] rows = mapData.Split('\n');
        map.map = new Node[rows.Length, rows[0].Length];

        for (int i = 0; i < map.Height; i++)
        {
            for (int j = 0; j < map.Width; j++)
            {
                map.map[i, j] = new Node()
                {
                    x = j,
                    y = i,
                    isObstacle = rows[i][j] == '@'
                };
            }
        }
        isMapScanned = false;
    }

    public void ScanMap()
    {
        distances.Clear();
        jumpPointsData.Clear();

        // Scan Primary Jump Points
        int mapHeight = map.Height;
        int mapWidth = map.Width;

        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                Node currNode = map.GetNode(j, i);
                EvaluateNodePrimaryJumpPoint(currNode);
            }
        }

        // Scan Straight Jump Points
        foreach (var cardDir in Direction.CardinalDirections)
        {
            int i = 0;
            int maxi = 0;
            int istep = 0;
            if (cardDir == Direction.WEST || cardDir == Direction.EAST)
            {
                i = 0;
                maxi = mapHeight;
                istep = 1;
            }
            else if (cardDir == Direction.NORTH || cardDir == Direction.SOUTH)
            {
                i = 0;
                maxi = mapWidth;
                istep = 1;
            }

            for (; i < maxi; i += istep)
            {
                int count = -1;
                bool jumpPointLastSeen = false;

                int j = 0;
                int maxj = 0;
                int stepj = 1;

                if (cardDir == Direction.WEST)
                {
                    j = 0;
                    maxj = mapWidth;
                    stepj = 1;
                }
                else if (cardDir == Direction.EAST)
                {
                    j = mapWidth - 1;
                    maxj = mapWidth;
                    stepj = -1;
                }
                else if (cardDir == Direction.SOUTH)
                {
                    j = 0;
                    maxj = mapHeight;
                    stepj = 1;
                }
                else if (cardDir == Direction.NORTH)
                {
                    j = mapHeight - 1;
                    maxj = mapHeight;
                    stepj = -1;
                }

                for (; j >= 0 && j < maxj; j += stepj)
                {
                    Node currNode = null;
                    if (cardDir == Direction.WEST || cardDir == Direction.EAST)
                    {
                        currNode = map.GetNode(j, i);
                    }
                    else
                    {
                        currNode = map.GetNode(i, j);
                    }

                    if (currNode == null) continue;

                    if (currNode.isObstacle)
                    {
                        count = -1;
                        jumpPointLastSeen = false;
                        SetNodeDistanceData(currNode, cardDir, 0);
                        continue;
                    }

                    count++;

                    if (jumpPointLastSeen)
                    {
                        SetNodeDistanceData(currNode, cardDir, count);
                    }
                    else
                    {
                        SetNodeDistanceData(currNode, cardDir, -count);
                    }

                    if (jumpPointsData.ContainsKey(currNode))
                    {
                        if (jumpPointsData[currNode].ContainDirection(cardDir))
                        {
                            count = 0;
                            jumpPointLastSeen = true;
                        }
                    }
                }
            }
        }

        // Scan Diagonal Jump Points
        foreach (var diagonalDir in Direction.DiagonalDirections)
        {
            Vector3Int hDir = Direction.GetHorizontalCardinalDirections(diagonalDir);
            Vector3Int vDir = Direction.GetVerticalCardinalDirections(diagonalDir);

            int starti = 0;
            int maxi = mapHeight;
            int stepi = 1;

            if (diagonalDir == Direction.NORTH_WEST || diagonalDir == Direction.NORTH_EAST)
            {
                starti = mapHeight - 1;
                maxi = mapHeight;
                stepi = -1;
            }

            for (int i = starti; i >= 0 && i < maxi; i += stepi)
            {
                int startj = 0;
                int maxj = mapWidth;
                int stepj = 1;

                if (diagonalDir == Direction.NORTH_EAST || diagonalDir == Direction.SOUTH_EAST)
                {
                    startj = mapWidth - 1;
                    maxj = mapWidth;
                    stepj = -1;
                }

                for (int j = startj; j >= 0 && j < maxj; j += stepj)
                {
                    int x = j;
                    int y = i;
                    Node currNode = map.GetNode(x, y);
                    if (!currNode.isObstacle)
                    {
                        if (y == starti || x == startj || IsObstacle(x, y + diagonalDir.y) ||
                            IsObstacle(x + diagonalDir.x, y) || IsObstacle(x + diagonalDir.x, y + diagonalDir.y))
                        {
                            //Wall one away
                            SetNodeDistanceData(currNode, diagonalDir, 0);
                        }
                        else
                        {
                            bool checkJumpPoint = !IsObstacle(x, y + diagonalDir.y) && !IsObstacle(x + diagonalDir.x, y);
                            Node diagonalNode = map.GetNode(x + diagonalDir.x, y + diagonalDir.y);
                            checkJumpPoint &= (GetDistanceData(diagonalNode, hDir) > 0 || GetDistanceData(diagonalNode, vDir) > 0);

                            if (checkJumpPoint)
                            {
                                //Straight jump point one away
                                SetNodeDistanceData(currNode, diagonalDir, 1);
                            }
                            else
                            {
                                //Increment from last
                                int jumpDistance = GetDistanceData(diagonalNode, diagonalDir);
                                if (jumpDistance > 0)
                                {
                                    SetNodeDistanceData(currNode, diagonalDir, jumpDistance + 1);
                                }
                                else
                                {
                                    SetNodeDistanceData(currNode, diagonalDir, jumpDistance - 1);
                                }
                            }
                        }
                    }
                }
            }
        }

        isMapScanned = true;
    }

    public bool IsObstacle(int x, int y)
    {
        Node node = map.GetNode(x, y);
        return node == null || node.isObstacle;
    }

    private void EvaluateNodePrimaryJumpPoint(Node node)
    {
        if (node == null) return;
        if (node.isObstacle) return;

        foreach (var directionCheck in Direction.CardinalDirections)
        {
            Node checkNode = GetNode(node, directionCheck);

            if (checkNode == null) continue;
            if (checkNode.isObstacle) continue;

            List<Vector3Int> forceNeighbourCheckDir = new List<Vector3Int>();
            if (directionCheck == Direction.NORTH || directionCheck == Direction.SOUTH)
            {
                forceNeighbourCheckDir.Add(Direction.WEST);
                forceNeighbourCheckDir.Add(Direction.EAST);
            }
            else if (directionCheck == Direction.WEST || directionCheck == Direction.EAST)
            {
                forceNeighbourCheckDir.Add(Direction.NORTH);
                forceNeighbourCheckDir.Add(Direction.SOUTH);
            }
            List<Node> forcedNeighbourNodes = new List<Node>();

            for (int j = 0; j < forceNeighbourCheckDir.Count; j++)
            {
                Node tempFN = GetForceNeighbourNodeForDirection(node, checkNode, forceNeighbourCheckDir[j]);
                if (tempFN != null) forcedNeighbourNodes.Add(tempFN);
            }

            if (forcedNeighbourNodes.Count > 0)
            {
                AddJumpPointData(checkNode, directionCheck);
                foreach (var fnNode in forcedNeighbourNodes)
                {
                    AddForceNeighbourForJumpPoint(checkNode, fnNode);
                }
            }
        }
    }

    public Node GetForceNeighbourNodeForDirection(Node node, Node checkNode, Vector3Int dir)
    {
        Node checkObstacleNode = GetNode(node, dir);
        Node forceNeighbourNode = GetNode(checkNode, dir);

        bool isForceNeighbour = (checkObstacleNode != null && checkObstacleNode.isObstacle)
            && (forceNeighbourNode != null && !forceNeighbourNode.isObstacle);

        if (isForceNeighbour)
        {
            return forceNeighbourNode;
        }
        return null;
    }

    public void AddForceNeighbourForJumpPoint(Node node, Node forcedNeighbour)
    {
        if (node == null) return;
        if (forcedNeighbour == null) return;

        if (!jumpPointsData.ContainsKey(node))
        {
            jumpPointsData.Add(node, new JumpPoint());
        }
        jumpPointsData[node].AddForcedNeighbours(forcedNeighbour);
    }

    public void AddJumpPointData(Node node, Vector3Int dir)
    {
        if (node == null) return;
        if (!jumpPointsData.ContainsKey(node))
        {
            jumpPointsData.Add(node, new JumpPoint());
        }
        jumpPointsData[node].AddDirection(dir);
    }

    public int GetDistanceData(Node node, Vector3Int dir)
    {
        if (node == null) return 0;
        if (!distances.ContainsKey(node)) return 0;
        if (!distances[node].ContainsKey(dir)) return 0;
        return distances[node][dir];
    }

    public void AddDistanceData(Node node, Vector3Int dir)
    {
        if (node == null) return;
        if (!distances.ContainsKey(node))
        {
            distances.Add(node, new Dictionary<Vector3Int, int>());
        }
        if (!distances[node].ContainsKey(dir))
        {
            distances[node].Add(dir, 0);
        }
    }

    public void SetNodeDistanceData(Node node, Vector3Int dir, int distance)
    {
        if (node == null) return;
        AddDistanceData(node, dir);
        distances[node][dir] = distance;
    }

    public Node GetNode(Node node, Vector3Int dir)
    {
        if (node == null) return null;
        Node checkNode = map.GetNode(node.x + (int)dir.x, node.y + (int)dir.y);
        return checkNode;
    }

    public Node GetNode(Node node, int diff, Vector3Int dir)
    {
        // HACK: unknown diff calculation
        //if (diff <= 0) diff = 1;
        return GetNode(node, dir * diff);
    }

    public int DiffNodesRow(Node node1, Node node2)
    {
        if (node1 == null || node2 == null) return 0;
        return Mathf.Abs(node2.y - node1.y);
    }

    public int DiffNodesCol(Node node1, Node node2)
    {
        if (node1 == null || node2 == null) return 0;
        return Mathf.Abs(node2.x - node1.x);
    }

    public int DiffNodes(Node node1, Node node2)
    {
        return DiffNodesCol(node1, node2) + DiffNodesRow(node1, node2);
    }

    public bool IsInExactDirection(Node originNode, Node goalNode, Vector3Int dir)
    {
        Node checkNode = GetNode(originNode, dir);
        while (checkNode != null)
        {
            if (checkNode == goalNode) return true;
            checkNode = GetNode(checkNode, dir);
        }
        return false;
    }

    public HashSet<Node> GetGeneralDirectionNodes(Node goalNode)
    {
        HashSet<Node> checkGoalNodes = new HashSet<Node>();
        List<Vector3Int> checkDirs = new List<Vector3Int>();
        checkDirs.AddRange(Direction.CardinalDirections);
        checkDirs.AddRange(Direction.DiagonalDirections);

        checkGoalNodes.Add(goalNode);
        foreach (var tempDir in checkDirs)
        {
            Node tempNode = goalNode;
            do
            {
                tempNode = GetNode(tempNode, tempDir);
                if (tempNode != null && !tempNode.isObstacle)
                {
                    checkGoalNodes.Add(tempNode);
                }
            } while (tempNode != null);
        }
        return checkGoalNodes;
    }

    public Node GetNodeGeneralDirection(Node originNode, Node goalNode, Vector3Int dir, HashSet<Node> checkGoalNodes = null)
    {
        if (originNode == null) return null;
        if (goalNode == null) return null;

        if (checkGoalNodes == null) checkGoalNodes = GetGeneralDirectionNodes(goalNode);
        Node checkNode = originNode;
        do
        {
            checkNode = GetNode(checkNode, dir);
            if (checkNode == null || checkNode.isObstacle) return null;
            if (checkGoalNodes.Contains(checkNode)) return checkNode;
        } while (checkNode != null);

        return null;
    }

    public bool IsInGeneralDirection(Node originNode, Node goalNode, Vector3Int dir, HashSet<Node> checkGoalNodes = null)
    {
        return GetNodeGeneralDirection(originNode, goalNode, dir, checkGoalNodes) != null;
    }

    public int CalculateHeuristic(Node origin, Node target)
    {
        return DiffNodes(origin, target);
    }

    public List<Node> FindPath(Vector2Int startPos, Vector2Int goalPos, System.Action<Node> actionOnOpenList = null)
    {
        Node startNode = map.GetNode(startPos.x, startPos.y);
        Node goalNode = map.GetNode(goalPos.x, goalPos.y);
        UnityEngine.Profiling.Profiler.BeginSample(this.pathMode.ToString());
        List<Node> path = null;
        switch (this.pathMode)
        {
            case PathMode.Path_Dijkstra:
                path = FindDijkstraPath(startNode, goalNode, actionOnOpenList);
                break;
            case PathMode.Path_Heuristic:
                path = FindHeuristicPath(startNode, goalNode, actionOnOpenList);
                break;
            case PathMode.Path_AStar:
                path = FindAStarPath(startNode, goalNode, actionOnOpenList);
                break;
            case PathMode.Path_JPSPlus:
                path = FindJPSPath(startNode, goalNode, actionOnOpenList);
                break;
        }
        UnityEngine.Profiling.Profiler.EndSample();
        return path;
    }

    public List<Node> FindJPSPath(Node startNode, Node goalNode, System.Action<Node> actionOnOpenList = null)
    {
        if (startNode == null) return null;
        if (goalNode == null) return null;

        if (!isMapScanned) ScanMap();
        map.Reset();

        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();

        openList.Add(startNode);
        if (actionOnOpenList != null) actionOnOpenList.Invoke(startNode);

        var cachedGeneralDirection = GetGeneralDirectionNodes(goalNode);

        while (openList.Count > 0)
        {
            openList.Sort((n1, n2) => n1.finalCost.CompareTo(n2.finalCost));
            Node currNode = openList[0];
            openList.RemoveAt(0);
            Node parentNode = currNode.parentNode;

            if (currNode == goalNode)
            {
                List<Node> path = new List<Node>();
                Node tempPathNode = currNode;
                while (tempPathNode != null)
                {
                    path.Add(tempPathNode);
                    tempPathNode = tempPathNode.parentNode;
                }
                path.Reverse();
                return path;
            }

            foreach (var direction in GetValidDirectonGivenParentNode(currNode, parentNode))
            {
                Node newSuccessor = null;
                float givenCost = 0;

                if (Direction.IsCardinal(direction) &&
                    IsInExactDirection(currNode, goalNode, direction) &&
                    DiffNodes(currNode, goalNode) <= Mathf.Abs(GetDistanceData(currNode, direction))
                    )
                {
                    //Goal is closer than wall distance or
                    //closer than or equal to jump point distance
                    newSuccessor = goalNode;
                    givenCost = currNode.givenCost + DiffNodes(currNode, goalNode);
                }
                else if (Direction.IsDiagonal(direction) &&
                    IsInGeneralDirection(currNode, goalNode, direction, cachedGeneralDirection) &&
                        (
                            DiffNodesRow(currNode, goalNode) <= Mathf.Abs(GetDistanceData(currNode, direction))
                            || DiffNodesCol(currNode, goalNode) <= Mathf.Abs(GetDistanceData(currNode, direction))
                        )
                    )
                {
                    //Goal is closer or equal in either row or
                    //column than wall or jump point distance
                    //Create a target jump point
                    Node intersectNode = GetNodeGeneralDirection(currNode, goalNode, direction, cachedGeneralDirection);
                    int minDiff = Mathf.Min(DiffNodesRow(currNode, intersectNode), DiffNodesCol(currNode, intersectNode));
                    //newSuccessor = GetNode(currNode, minDiff, direction);
                    newSuccessor = intersectNode;
                    givenCost = currNode.givenCost + (Mathf.Sqrt(2) * minDiff);
                }
                else if (GetDistanceData(currNode, direction) > 0)
                {
                    // Jump point in this direction
                    newSuccessor = GetNode(currNode, GetDistanceData(currNode, direction), direction);
                    givenCost = DiffNodes(currNode, newSuccessor);
                    if (Direction.IsDiagonal(direction))
                    {
                        givenCost *= Mathf.Sqrt(2);
                    }
                    givenCost += currNode.givenCost;
                }

                // Traditional A* from this point
                if (newSuccessor != null)
                {
                    if (!openList.Contains(newSuccessor) && !closedList.Contains(newSuccessor))
                    {
                        newSuccessor.parentNode = currNode;
                        newSuccessor.givenCost = givenCost;
                        newSuccessor.finalCost = givenCost + CalculateHeuristic(newSuccessor, goalNode);
                        openList.Add(newSuccessor);
                        if (actionOnOpenList != null) actionOnOpenList.Invoke(newSuccessor);
                    }
                    else if (givenCost < newSuccessor.givenCost)
                    {
                        newSuccessor.parentNode = currNode;
                        newSuccessor.givenCost = givenCost;
                        newSuccessor.finalCost = givenCost + CalculateHeuristic(newSuccessor, goalNode);
                        int idxOpen = openList.IndexOf(newSuccessor);
                        if (idxOpen >= 0)
                        {
                            openList[idxOpen] = newSuccessor;
                            if (actionOnOpenList != null) actionOnOpenList.Invoke(newSuccessor);
                        }
                    }
                }
                closedList.Add(currNode);
            }
        }
        return null;
    }

    public Vector3Int GetDirection(Node originNode, Node targetNode)
    {
        Vector3Int diff = new Vector3Int();
        diff.x = System.Math.Sign(targetNode.x - originNode.x);
        diff.y = System.Math.Sign(targetNode.y - originNode.y);

        return diff;
    }

    public List<Vector3Int> GetValidDirectonGivenParentNode(Node currNode, Node parentNode)
    {
        if (currNode == null) return Constants.ValidDirLookupTable.Keys.ToList();
        if (parentNode == null) return Constants.ValidDirLookupTable.Keys.ToList();

        Vector3Int diff = GetDirection(parentNode, currNode);

        if (!Constants.ValidDirLookupTable.ContainsKey(diff)) return Constants.ValidDirLookupTable.Keys.ToList();
        List<Vector3Int> dirs = new List<Vector3Int>();

        dirs.AddRange(Constants.ValidDirLookupTable[diff]);

        if (jumpPointsData.ContainsKey(currNode))
        {
            for (int i = 0; i < jumpPointsData[currNode].forcedNeighbours.Count; i++)
            {
                Vector3Int tempDir = GetDirection(currNode, jumpPointsData[currNode].forcedNeighbours[i]);
                if (!dirs.Contains(tempDir)) dirs.Add(tempDir);

                Vector3Int addTempDir = new Vector3Int();
                addTempDir.x = (int)Mathf.Sign(diff.x + tempDir.x);
                addTempDir.y = (int)Mathf.Sign(diff.y + tempDir.y);
                if (!dirs.Contains(addTempDir)) dirs.Add(addTempDir);
            }
        }

        return dirs;
    }

    public List<Node> FindAStarPath(Node startNode, Node goalNode, System.Action<Node> actionOnOpenList = null)
    {
        if (startNode == null) return null;
        if (goalNode == null) return null;

        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();
        map.Reset();

        List<Vector3Int> availableDirections = new List<Vector3Int>();
        availableDirections.AddRange(Direction.CardinalDirections);
        availableDirections.AddRange(Direction.DiagonalDirections);

        startNode.givenCost = 0;
        startNode.finalCost = CalculateHeuristic(startNode, goalNode);
        openList.Add(startNode);
        if (actionOnOpenList != null) actionOnOpenList.Invoke(startNode);

        while (openList.Count > 0)
        {
            openList.Sort((n1, n2) => n1.finalCost.CompareTo(n2.finalCost));
            Node currNode = openList[0];
            openList.RemoveAt(0);

            if (currNode == goalNode)
            {
                List<Node> path = new List<Node>();
                Node tempPathNode = currNode;
                do
                {
                    path.Add(tempPathNode);
                    tempPathNode = tempPathNode.parentNode;
                } while (tempPathNode != null);
                path.Reverse();
                return path;
            }

            foreach (var dir in availableDirections)
            {
                Node checkNode = GetNode(currNode, dir);
                if (checkNode == null) continue;
                if (checkNode.isObstacle) continue;

                if (closedList.Contains(checkNode)) continue;

                bool shouldUpdateValue = true;
                if (openList.Contains(checkNode) && checkNode.givenCost <= currNode.givenCost + 1)
                {
                    shouldUpdateValue = false;
                }
                if (shouldUpdateValue)
                {
                    checkNode.givenCost = currNode.givenCost + 1;
                    checkNode.finalCost = checkNode.givenCost + CalculateHeuristic(checkNode, goalNode);
                    checkNode.parentNode = currNode;
                }

                if (!openList.Contains(checkNode))
                {
                    openList.Add(checkNode);
                }
                if (actionOnOpenList != null) actionOnOpenList.Invoke(checkNode);
            }

            closedList.Add(currNode);
        }

        return null;
    }

    public List<Node> FindDijkstraPath(Node startNode, Node goalNode, System.Action<Node> actionOnOpenList = null)
    {
        if (startNode == null) return null;
        if (goalNode == null) return null;

        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();
        map.Reset();

        List<Vector3Int> availableDirections = new List<Vector3Int>();
        availableDirections.AddRange(Direction.CardinalDirections);
        availableDirections.AddRange(Direction.DiagonalDirections);

        startNode.givenCost = 0;
        startNode.finalCost = CalculateHeuristic(startNode, goalNode);
        openList.Add(startNode);
        if (actionOnOpenList != null) actionOnOpenList.Invoke(startNode);

        while (openList.Count > 0)
        {
            openList.Sort((n1, n2) => n1.finalCost.CompareTo(n2.finalCost));
            Node currNode = openList[0];
            openList.RemoveAt(0);

            if (currNode == goalNode)
            {
                List<Node> path = new List<Node>();
                Node tempPathNode = currNode;
                do
                {
                    path.Add(tempPathNode);
                    tempPathNode = tempPathNode.parentNode;
                } while (tempPathNode != null);
                path.Reverse();
                return path;
            }

            foreach (var dir in availableDirections)
            {
                Node checkNode = GetNode(currNode, dir);
                if (checkNode == null) continue;
                if (checkNode.isObstacle) continue;

                if (closedList.Contains(checkNode)) continue;

                bool shouldUpdateValue = true;
                if (openList.Contains(checkNode) && checkNode.givenCost <= currNode.givenCost + 1)
                {
                    shouldUpdateValue = false;
                }
                if (shouldUpdateValue)
                {
                    checkNode.givenCost = currNode.givenCost + 1;
                    checkNode.finalCost = checkNode.givenCost;
                    checkNode.parentNode = currNode;
                }

                if (!openList.Contains(checkNode))
                {
                    openList.Add(checkNode);
                }
                if (actionOnOpenList != null) actionOnOpenList.Invoke(checkNode);
            }

            closedList.Add(currNode);
        }

        return null;
    }

    public List<Node> FindHeuristicPath(Node startNode, Node goalNode, System.Action<Node> actionOnOpenList = null)
    {
        if (startNode == null) return null;
        if (goalNode == null) return null;

        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();
        map.Reset();

        List<Vector3Int> availableDirections = new List<Vector3Int>();
        availableDirections.AddRange(Direction.CardinalDirections);
        availableDirections.AddRange(Direction.DiagonalDirections);

        startNode.givenCost = 0;
        startNode.finalCost = CalculateHeuristic(startNode, goalNode);
        openList.Add(startNode);
        if (actionOnOpenList != null) actionOnOpenList.Invoke(startNode);

        while (openList.Count > 0)
        {
            openList.Sort((n1, n2) => n1.finalCost.CompareTo(n2.finalCost));
            Node currNode = openList[0];
            openList.RemoveAt(0);

            if (currNode == goalNode)
            {
                List<Node> path = new List<Node>();
                Node tempPathNode = currNode;
                do
                {
                    path.Add(tempPathNode);
                    tempPathNode = tempPathNode.parentNode;
                } while (tempPathNode != null);
                path.Reverse();
                return path;
            }

            foreach (var dir in availableDirections)
            {
                Node checkNode = GetNode(currNode, dir);
                if (checkNode == null) continue;
                if (checkNode.isObstacle) continue;

                if (closedList.Contains(checkNode)) continue;

                bool shouldUpdateValue = true;
                int hcost = CalculateHeuristic(checkNode, goalNode);
                if (openList.Contains(checkNode) && checkNode.finalCost <= hcost)
                {
                    shouldUpdateValue = false;
                }
                if (shouldUpdateValue)
                {
                    checkNode.givenCost = hcost;
                    checkNode.finalCost = checkNode.givenCost;
                    checkNode.parentNode = currNode;
                }

                if (!openList.Contains(checkNode))
                {
                    openList.Add(checkNode);
                }
                if (actionOnOpenList != null) actionOnOpenList.Invoke(checkNode);
            }

            closedList.Add(currNode);
        }

        return null;
    }
}
