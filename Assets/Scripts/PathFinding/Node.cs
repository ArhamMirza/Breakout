using UnityEngine; // Add this line

public class Node
{
    public Vector3 Position; 
    public int GCost;
    public int HCost;
    public int FCost { get { return GCost + HCost; } }
    public Node Parent;

    public Node(Vector3 position)
    {
        Position = position;
    }
}
