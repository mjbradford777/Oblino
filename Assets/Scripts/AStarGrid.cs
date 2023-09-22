using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarGrid : MonoBehaviour
{
    // A mask representing impassable terrain
    public LayerMask unwalkableMask;
    // Vector for size of grid in 2D space
    public Vector2 gridWorldSize;
    // Radius of nodes
    public float nodeRadius;
    // Creates array of walkable terrains
    public TerrainType[] walkableRegions;
    LayerMask walkableMask;
    // Creates penalty for being near obstacles to discourage movement near obstacles
    public int obstacleProximityPenalty = 100;
    // Creates a dictionary of walkable terrains for more efficient searching
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
    // 2D array grid for nodes
    Node[,] grid;

    // Diameter of nodes
    float nodeDiameter;
    // Width and height of grid stored in these variables
    int gridSizeX, gridSizeY;

    int penaltyMin = int.MaxValue;
    int penaltyMax = int.MinValue;

    private void Awake()
    {
        // Initialise starting values and create the grid as soon as the game starts
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        foreach (TerrainType region in walkableRegions)
        {
            // Uses bitwise operations to apply the bit values associated with the layers to the terrain mask, then adds to dictionary
            walkableMask.value |= region.terrainMask.value;
            walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
        }
        CreateGrid();
    }

    // Determines max size for the heap
    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    void CreateGrid()
    {
        // Initialises grid to 2D array of appropriate size
        grid = new Node[gridSizeX, gridSizeY];
        // Identify corner of grid to use as starting point
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                // Identify location of each given node in world space
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                // Identify if node should be considered impassable based on if it intersects with any impassable obstacles
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                // Initialises movement penalty at 0
                int movementPenalty = 0;

                // Uses raycasts to hit each node and attempt to gather the movement penalty associated with the terrain there, outputting it to movement penalty
                Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, walkableMask))
                {
                    walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                }

                if (!walkable)
                {
                    movementPenalty += obstacleProximityPenalty;
                }

                // Fill in grid with appropriately detailed nodes
                grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
            }
        }

        // Calls method to blur the map for more organic unit movement
        BlurPenaltyMap(3);
    }

    void BlurPenaltyMap(int blurSize)
    {
        // Creates a kernel for blur map to run calculations
        int kernelSize = blurSize * 2 + 1;
        int kernelExtents = (kernelSize - 1) / 2;

        // Creates two 2D arrays for storing the mapped data (using horizontal pass, then vertical pass, rather than one kernel doing both directions)
        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
        int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = -kernelExtents; x <= kernelExtents; x++)
            {
                // Populates first pass of kernel values for use below
                int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;
            }

            for (int x = 1; x < gridSizeX; x++)
            {
                // Blur map pass for remaining x. Removes far left item and adds far right item to do calculations, then saves calculated value in 2D array
                int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
                int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);

                penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
            }
        }

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = -kernelExtents; y <= kernelExtents; y++)
            {
                // Populates first pass of kernel values for use below
                int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
            }

            // Handles missing calculations for y == 0 row
            int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
            grid[x, 0].movementPenalty = blurredPenalty;

            for (int y = 1; y < gridSizeY; y++)
            {
                // Blur map pass for remaining y, similar to x calculations above
                int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
                int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);

                // Finalises calculation by using both horizontal and vertical pass data to create overall blur value
                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
                grid[x, y].movementPenalty = blurredPenalty;

                // Creates max values for visual illustration in debug mode
                if (blurredPenalty > penaltyMax)
                {
                    penaltyMax = blurredPenalty;
                }
                if (blurredPenalty < penaltyMin)
                {
                    penaltyMin = blurredPenalty;
                }
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        // A list to hold all neighbours of the passed in node
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) 
                {
                    // If x and y are 0 then this is the passed in node and should not be considered a neighbour of itself
                    continue;
                }

                // Variables to hold the 2D indices of the neighbour node
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    // Check to be sure the node exists (ie is not outside the grid) then add it to the neighbours list
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        // Return the list of neighbours
        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        // Create a percentage representation of where the passed in point is in relation to the grid x and y
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        // Error handling in case a passed in point sits outside the grid
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        // Map the percentage to the grid to create int values for array indices
        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        // Return the node that encompasses the passed in point
        return grid[x, y];
    }

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }
}
