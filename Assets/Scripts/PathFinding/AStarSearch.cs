using System.Collections.Generic;
using UnityEngine;

public class AStarSearch : MonoBehaviour
{
    public Transform startTransform;
    public Transform goalTransform;
    public LayerMask obstacleLayer;

    private Node startNode;
    private Node goalNode;
    private List<Node> openList;
    private List<Node> closedList;
    private List<Node> path;

    private void Start()
    {
        startNode = new Node(startTransform.position);
        goalNode = new Node(goalTransform.position);
        openList = new List<Node>();
        closedList = new List<Node>();
        path = new List<Node>();

        FindPath();
    }

    private void FindPath()
    {
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = GetNodeWithLowestFCost();

            // If we've reached the goal node
            if (currentNode.Position == goalNode.Position)
            {
                RetracePath(currentNode);
                return;
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (closedList.Contains(neighbor)) continue;
                if (IsObstacle(neighbor)) continue;

                int tentativeGCost = currentNode.GCost + GetDistance(currentNode, neighbor);

                if (tentativeGCost < neighbor.GCost || !openList.Contains(neighbor))
                {
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = GetDistance(neighbor, goalNode);
                    neighbor.Parent = currentNode;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        Vector3[] directions = new Vector3[]
        {
            Vector3.up, Vector3.down, Vector3.left, Vector3.right
        };

        foreach (Vector3 dir in directions)
        {
            Vector3 neighborPos = node.Position + dir;
            neighbors.Add(new Node(neighborPos));
        }

        return neighbors;
    }

    private Node GetNodeWithLowestFCost()
    {
        Node bestNode = openList[0];

        foreach (Node node in openList)
        {
            if (node.FCost < bestNode.FCost)
                bestNode = node;
        }

        return bestNode;
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        return Mathf.Abs((int)(nodeA.Position.x - nodeB.Position.x)) + Mathf.Abs((int)(nodeA.Position.y - nodeB.Position.y));
    }

    private bool IsObstacle(Node node)
    {
        // Cast a ray to check for obstacles in the grid tile
        RaycastHit hit;
        return Physics.Raycast(node.Position, Vector3.down, out hit, 1f, obstacleLayer);
    }

    private void RetracePath(Node endNode)
    {
        path.Clear();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }
        path.Reverse();

        // Optional: Do something with the path (like moving an agent along it)
        Debug.Log("Path Found:");
        foreach (Node node in path)
        {
            Debug.Log(node.Position);
        }
    }
}
