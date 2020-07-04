using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Pathfinding))]
public class PathfindingEditor : MonoBehaviour
{
    [SerializeField] private Pathfinding _pathfinding;
    [SerializeField] private Camera _mainCamera;

    //public Vector2Int mapSize = new Vector2Int(10, 10);

    [TextArea(5, 20)] public string mapData;
    public float delayShowPath = 0.3f;
    public int textSize = 10;
    public float textPosMult = 0.3f;

    public GameObject viewPrefab;
    private Dictionary<Node, NodeView> views = new Dictionary<Node, NodeView>();


    private Vector2Int startPoint = new Vector2Int(0, 0);
    private Vector2Int endPoint = new Vector2Int(9, 9);
    private bool isSimulating = false;

    private List<Node> pathNode = new List<Node>();
    private List<Node> openListNode = new List<Node>();
    private List<Node> selectedNodes = new List<Node>();
    private Node hoverNode = null;

    private const string INPUT_KEY_LEFT_MOUSE_CLICK = "Fire1";
    private const string INPUT_KEY_RIGHT_MOUSE_CLICK = "Fire2";
    private const string INPUT_KEY_MIDDLE_MOUSE_CLICK = "Fire3";

    private void Awake()
    {
        if (_pathfinding == null) _pathfinding = GetComponent<Pathfinding>();
        if (_mainCamera == null) _mainCamera = Camera.main;
    }

    private void Start()
    {
        GenerateMap();
    }

    private void Update()
    {
        if (isSimulating) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            GenerateMap();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _pathfinding.ScanMap();
            this.pathNode.Clear();
            this.openListNode.Clear();
            UpdateAllNodeColor();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FindPath();
        }

        if (Input.GetButtonDown(INPUT_KEY_LEFT_MOUSE_CLICK))
        {
            selectedNodes.Clear();
        }
        else if (Input.GetButton(INPUT_KEY_LEFT_MOUSE_CLICK))
        {
            SetActionOnNode((targetNode) =>
            {
                if (!selectedNodes.Contains(targetNode))
                {
                    targetNode.isObstacle = !targetNode.isObstacle;
                    _pathfinding.isMapScanned = false;
                    UpdateNodeColor(targetNode);
                    selectedNodes.Add(targetNode);
                }
            });
        }
        else if (Input.GetButtonUp(INPUT_KEY_LEFT_MOUSE_CLICK))
        {
            selectedNodes.Clear();
        }
        if (Input.GetButtonDown(INPUT_KEY_RIGHT_MOUSE_CLICK))
        {
            SetActionOnNode((targetNode) =>
            {
                bool isPrevObstacle = targetNode.isObstacle;
                targetNode.isObstacle = false;
                Node prevGoalNode = _pathfinding.map.GetNode(endPoint.x, endPoint.y);
                endPoint = new Vector2Int(targetNode.x, targetNode.y);
                UpdateNodeColor(prevGoalNode);
                UpdateNodeColor(targetNode);
                if (isPrevObstacle)
                {
                    _pathfinding.isMapScanned = false;
                }
            });
        }

        if (Input.GetButtonDown(INPUT_KEY_MIDDLE_MOUSE_CLICK))
        {
            SetActionOnNode((targetNode) =>
            {
                bool isPrevObstacle = targetNode.isObstacle;
                targetNode.isObstacle = false;
                Node prevStartNode = _pathfinding.map.GetNode(startPoint.x, startPoint.y);
                startPoint = new Vector2Int(targetNode.x, targetNode.y);
                UpdateNodeColor(prevStartNode);
                UpdateNodeColor(targetNode);
                if (isPrevObstacle)
                {
                    _pathfinding.isMapScanned = false;
                }
            });
        }

        SetActionOnNode((targetNode) =>
        {
            hoverNode = targetNode;
        });
    }

    public void FindPath()
    {
        if (isSimulating) return;

        isSimulating = true;
        this.pathNode.Clear();
        this.openListNode.Clear();
        var tempPath = _pathfinding.FindPath(startPoint, endPoint, (node) =>
        {
            if (!openListNode.Contains(node)) openListNode.Add(node);
            UpdateNodeColor(node);
        });
        UpdateAllNodeColor();
        StartCoroutine(Coroutine_SimulatePath(tempPath));
    }

    private void SetActionOnNode(System.Action<Node> action)
    {
        Vector3 cursorPos = Input.mousePosition;
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(cursorPos);

        int x = (int)(worldPos.x + 0.5f);
        int y = (int)(worldPos.y + 0.5f);
        Node targetNode = _pathfinding.map.GetNode(x, y);
        if (targetNode != null)
        {
            if (action != null) action.Invoke(targetNode);
        }
    }

    private IEnumerator Coroutine_SimulatePath(List<Node> path)
    {
        this.pathNode.Clear();
        if (path != null)
        {
            for (int i = 0; i < path.Count; i++)
            {
                this.pathNode.Add(path[i]);
                UpdateNodeColor(path[i]);
                yield return new WaitForSeconds(delayShowPath);
            }
        }
        else
        {
            Debug.Log("Path Not Found");
        }

        isSimulating = false;
        yield return null;
    }

    private void GenerateMap()
    {
        _pathfinding.GenerateMap(mapData);

        if (viewPrefab != null)
        {
            if (views.Count > 0)
            {
                foreach (var viewObj in views)
                {
                    Destroy(viewObj.Value.gameObject);
                }
            }
            views.Clear();

            for (int i = 0; i < _pathfinding.map.Height; i++)
            {
                for (int j = 0; j < _pathfinding.map.Width; j++)
                {
                    Node node = _pathfinding.map.GetNode(j, i);

                    GameObject go = Instantiate(viewPrefab);
                    go.transform.position = new Vector3(j, i, 1);
                    var nodeView = go.GetComponent<NodeView>();
                    views.Add(node, nodeView);
                    UpdateNodeColor(node);
                }
            }
            UpdateAllNodeColor();
        }
    }

    private void UpdateAllNodeColor()
    {
        this.pathNode.Clear();
        if (views != null && views.Count > 0)
        {
            foreach (var node in views)
            {
                UpdateNodeColor(node.Key);
            }
        }
    }

    private void UpdateNodeColor(Node node)
    {
        if (node == null) return;
        if (!views.ContainsKey(node)) return;
        views[node].SetColor(GetGizmoColor(node));
    }

    private void OnDrawGizmos()
    {
        if (_pathfinding == null) _pathfinding = GetComponent<Pathfinding>();

        Color defaultGizmoColor = Gizmos.color;
        /*
        for (int i = 0; i < _pathfinding.map.Height; i++)
        {
            for (int j = 0; j < _pathfinding.map.Width; j++)
            {
                Node node = _pathfinding.map.GetNode(j, i);
                Gizmos.color = GetGizmoColor(node);
                Gizmos.DrawCube(new Vector3(node.x, node.y), Vector3.one);

                if (node == hoverNode)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireCube(new Vector3(node.x, node.y), Vector3.one);
                }
            }
        }
        */
        if (hoverNode != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(new Vector3(hoverNode.x, hoverNode.y), Vector3.one);
        }
        Gizmos.color = defaultGizmoColor;

        GUIStyle style = new GUIStyle();
        style.fontSize = textSize;
        style.normal.textColor = Color.black;

        if (_pathfinding.distances != null && _pathfinding.distances.Count > 0)
        {
            foreach (var distanceData in _pathfinding.distances)
            {
                if (distanceData.Value != null && distanceData.Value.Count > 0)
                {
                    foreach (var distance in distanceData.Value)
                    {
                        Vector3 pos = new Vector3(distanceData.Key.x, distanceData.Key.y);
                        pos += (new Vector3(distance.Key.x, distance.Key.y) * this.textPosMult);

                        Handles.BeginGUI();
                        Vector2 pos2D = HandleUtility.WorldToGUIPoint(pos);
                        GUI.Label(new Rect(pos2D.x, pos2D.y, 100, 100), distance.Value.ToString(), style);
                        Handles.EndGUI();
                    }
                }
            }
        }
    }

    private Color GetGizmoColor(Node node)
    {
        Color color = Color.black;
        int colorCount = 0;
        if (node != null)
        {
            if (pathNode.Contains(node))
            {
                color = Color.yellow;
                colorCount++;
            }
            else if (openListNode.Contains(node))
            {
                color = Color.cyan;
                colorCount++;
            }
            else if (_pathfinding.jumpPointsData.ContainsKey(node))
            {
                color += Color.blue;
                colorCount++;
            }

            if (node.x == startPoint.x && node.y == startPoint.y)
            {
                color += Color.green;
                colorCount++;
            }
            else if (node.x == endPoint.x && node.y == endPoint.y)
            {
                color += Color.red;
                colorCount++;
            }

            if (node.isObstacle)
            {
                color += Color.gray;
                colorCount++;
            }
            else
            {
                if (colorCount == 0)
                {
                    color += Color.white;
                    colorCount = 1;
                }
            }
        }

        return color / colorCount;
    }
}
