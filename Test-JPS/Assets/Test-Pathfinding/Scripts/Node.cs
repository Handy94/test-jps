public class Node
{
    public int x;
    public int y;
    public Node parentNode;
    public float givenCost = 0;
    public float finalCost = 0;
    public bool isObstacle = false;

    public string Name => $"Node {x}-{y}";

    public void Reset()
    {
        parentNode = null;
        givenCost = 0;
        finalCost = 0;
    }
}
