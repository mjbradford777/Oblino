using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    // Each node stores for itself whether or not it's walkable
    public bool walkable;
    // Each node stores its own position in the world
    public Vector3 worldPosition;
    // Each node stores its x and y position relative to the 'grid' of nodes
    public int gridX;
    public int gridY;
    // Creates variable for movementPenalty of different terrains
    public int movementPenalty;

    // Each node stores its g cost (distance from start) and h cost (distance from end) for A* purposes
    public int gCost;
    public int hCost;
    // Each node stores the information for its parent so a path can be constructed
    public Node parent;

    int heapIndex;

    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY, int _penalty)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _penalty;
    }

    // f cost has a getter only as it should not be reassignable
    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    // Stores heap index in node and allows it to be updated through setter
    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    // Allows node to compare its own costs against the costs of another node
    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }
}
