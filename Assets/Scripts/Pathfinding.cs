using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    // The grid to be used for pathfinding
    AStarGrid grid;

    private void Awake()
    {
        {
            // Associates grid to correct component in Unity editor
            grid = GetComponent<AStarGrid>();
        }
    }

    public void FindPath(PathRequest request, Action<PathResult> callback)
    {
        // Creates an array to store waypoints and a boolean to store whether the calculation is done or not
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;
        // Gathers start and target nodes from passed in world points and stores the nodes here
        Node startNode = grid.NodeFromWorldPoint(request.pathStart);
        Node targetNode = grid.NodeFromWorldPoint(request.pathEnd);

        if (startNode.walkable && targetNode.walkable)
        {
            // Defines list/set for open nodes and closed nodes
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();
            // Adds start node to the open set to start the process
            openSet.Add(startNode);

            // Continues until there are no open nodes remaining
            while (openSet.Count > 0)
            {
                // Performs analysis on first node, removes it from open set and adds to closed set
                Node node = openSet.RemoveFirst();
                closedSet.Add(node);

                if (node == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                // Gather and iterate through all neighbours of the selected node
                foreach (Node neighbour in grid.GetNeighbours(node))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                    {
                        // Ignore the neighbour if it is impassable or if it has already been examined
                        continue;
                    }

                    // Assign a movement cost. Check also accounts for if new branch in path leads to a cheaper cost and applies movementPenalty to cost
                    int newMovementCostToNeighbour = node.gCost + GetDistance(node, neighbour) + neighbour.movementPenalty;
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        // If the neighbour is now cheaper to traverse or hasn't been in open set, adjust properties accordingly and add to open set if necessary
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = node;

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }
        }
        if (pathSuccess)
        {
            // Create the path of waypoints and store it in a variable so it can be passed to other operations
            waypoints = RetracePath(startNode, targetNode);
            pathSuccess = waypoints.Length > 0;
        }
        // Returns waypoint path and success message when done processing
        callback(new PathResult(waypoints, pathSuccess, request.callback));
    }

    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        // Creates list to store nodes representing the "path"
        List<Node> path = new List<Node>();
        // Start at the end
        Node currentNode = endNode;

        // Add current node to path, move to its parent, repeat until the start node is reached
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        // Put path in order from start to finish and return
        Array.Reverse(waypoints);
        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        // Creates list to store waypoints and a 2D vector to store the direction of the path at a specific point
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            // Calculates the direction of the path at each point compared to its previous point
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld)
            {
                // Only add nodes to the waypoints list if the direction changes, so as to simplify the path that needs to be communicated
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        // Gather x and y distances between passed in nodes
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
        {
            // Construct the calculation for overall distance between nodes if x distance is greater than y distance
            return 14 * dstY + 10 * (dstX - dstY);
        }

        // Construct the calculation for overall distance between nodes if y distance is greater than x distance
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
