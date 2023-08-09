using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BubbleGrid : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private int rowCount = 11;
    [SerializeField] private int colCount = 9;

    private float hexInnerRadius;
    private float hexOuterRadius;
    private List<List<Bubble.BubbleColor>> _gridData = new();
    private List<List<Bubble>> _bubbleGrid = new();

    private Camera _mainCamera;

    // Neighbors start from right and go counter clockwise
    private List<Vector2Int> _evenNeighborOffsets = new()
    {
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1),
        new Vector2Int(1, 0),
    };

    private List<Vector2Int> _oddNeighborOffsets = new()
    {
        new Vector2Int(0, +1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(+1, 0),
        new Vector2Int(+1, +1),
    };

    private void Start()
    {
        _mainCamera = Camera.main;

        CalculateSizes();
        SetInitialGridPosition();

        FillGridRandomly();
        InitializeGrid();
    }

    /**
     * Calculates what size the bubbles should be according to the camera's size and column count
     */
    private void CalculateSizes()
    {
        hexInnerRadius = _mainCamera.orthographicSize * _mainCamera.aspect / (colCount + 0.5f);
        hexOuterRadius = hexInnerRadius * 2 / Mathf.Sqrt(3);

        float bubbleScale = hexInnerRadius / bubblePrefab.GetComponent<SpriteRenderer>().sprite.bounds.extents.x;
        bubblePrefab.transform.localScale = new Vector3(bubbleScale, bubbleScale, 1.0f);
    }

    /**
     * Sets the grid's initial position according to its height and width
     */
    private void SetInitialGridPosition()
    {
        float gridX = -_mainCamera.orthographicSize * _mainCamera.aspect + hexInnerRadius;
        float gridY = _mainCamera.orthographicSize - hexInnerRadius;
        float gridHeight = (rowCount - 1) * hexOuterRadius * 3 / 2 + hexInnerRadius;
        if (gridHeight > _mainCamera.orthographicSize)
        {
            float bruh = (int)((_mainCamera.orthographicSize - hexInnerRadius * 2) / (hexOuterRadius * 3 / 2)) *
                hexOuterRadius * 3 / 2;
            gridY += gridHeight - bruh - hexInnerRadius;
        }

        transform.position = new Vector3(gridX, gridY, 0.0f);
    }

    /**
     * Fills the grid data randomly
     */
    private void FillGridRandomly()
    {
        for (int i = 0; i < rowCount; i++)
        {
            List<Bubble.BubbleColor> row = new List<Bubble.BubbleColor>();
            for (int j = 0; j < colCount; j++)
            {
                row.Add((Bubble.BubbleColor)Random.Range(0, 3));
            }

            _gridData.Add(row);
        }
    }

    /**
     * Initializes the Bubble objects and adds them to the grid
     */
    private void InitializeGrid()
    {
        for (int i = 0; i < rowCount; i++)
        {
            List<Bubble> row = new List<Bubble>();
            for (int j = 0; j < colCount; j++)
            {
                Vector3 localPosition = CalculateLocalPosition(i, j);
                GameObject bubbleObject = Instantiate(bubblePrefab, transform);
                bubbleObject.transform.localPosition = localPosition;
                Bubble bubble = bubbleObject.GetComponent<Bubble>().SetBubbleColor(_gridData[i][j]);
                bubble.gridCoordinate = new Vector2Int(i, j);

                row.Add(bubble);
            }

            _bubbleGrid.Add(row);
        }
    }

    public void HandleBubble(Bubble thrownBubble, Bubble gridBubble)
    {
        // Calculate in which angle the ball hit
        Vector2 differenceVector = thrownBubble.transform.position - gridBubble.transform.position;
        float angleRadians = Mathf.Atan2(differenceVector.y, differenceVector.x);
        float angleDegrees = angleRadians * Mathf.Rad2Deg;
        if (angleDegrees < 0)
        {
            angleDegrees += 360;
        }

        // Place it to the grid accordingly
        // Sides start from the hexagon's right side and goes counter clockwise
        // 0 => right, 1 => upper right, 2 => upper left, 3 => left, 4 => lower left, 5 => lower right, 
        float side = ((angleDegrees + 30) % 360) / 60;
        int sideIndex = (int)((angleDegrees + 30) % 360) / 60;
        // If that side is not empty we will offset it to the closes other side
        int sideOffset = side % 1 >= 0.5 ? 1 : 5;

        Vector2Int newBubbleCoordinate = gridBubble.gridCoordinate +
                                         (gridBubble.gridCoordinate.x % 2 == 0
                                             ? _evenNeighborOffsets[sideIndex]
                                             : _oddNeighborOffsets[sideIndex]);

        if (newBubbleCoordinate.x >= _bubbleGrid.Count)
        {
            AddEmptyRow();
        }

        // If the hex is not empty or out of bounds, find the closest hex
        if (!CheckForBounds(newBubbleCoordinate) || GetBubble(newBubbleCoordinate) != null)
        {
            newBubbleCoordinate = gridBubble.gridCoordinate +
                                  (gridBubble.gridCoordinate.x % 2 == 0
                                      ? _evenNeighborOffsets[(sideIndex + sideOffset) % 6]
                                      : _oddNeighborOffsets[(sideIndex + sideOffset) % 6]);
        }

        thrownBubble.transform.SetParent(transform);
        thrownBubble.transform.localPosition = CalculateLocalPosition(newBubbleCoordinate.x, newBubbleCoordinate.y);
        thrownBubble.throwingBubble = false;
        thrownBubble.gridCoordinate = newBubbleCoordinate;
        SetBubble(thrownBubble.gridCoordinate, thrownBubble);

        HandleMatch(thrownBubble.gridCoordinate);
    }

    private void HandleMatch(Vector2Int thrownBubbleCoord)
    {
        List<Vector2Int> visited = new List<Vector2Int>();

        DfsMatch(thrownBubbleCoord, visited);

        foreach (Vector2Int bruh in visited)
        {
            Debug.Log(bruh);
        }

        // Find the separated islands
        // Remove them, maybe animations
    }

    private void DfsMatch(Vector2Int bubbleCoord, List<Vector2Int> visited)
    {
        if (visited.Contains(bubbleCoord))
        {
            return;
        }

        visited.Add(bubbleCoord);

        List<Vector2Int> neighborOffsets = bubbleCoord.x % 2 == 0 ? _evenNeighborOffsets : _oddNeighborOffsets;
        foreach (Vector2Int offset in neighborOffsets)
        {
            Vector2Int neighborCoord = bubbleCoord + offset;
            Bubble neighbor = GetBubble(neighborCoord);

            if (neighbor == null)
            {
                continue;
            }

            if (GetBubble(bubbleCoord).GetBubbleColor() == neighbor.GetBubbleColor())
            {
                DfsMatch(neighborCoord, visited);
            }
        }
    }

    private void AddEmptyRow()
    {
        List<Bubble> row = new List<Bubble>();
        for (int i = 0; i < colCount; i++)
        {
            row.Add(null);
        }

        _bubbleGrid.Add(row);
        rowCount = _bubbleGrid.Count;
    }

    private Vector3 CalculateLocalPosition(int rowNum, int colNum)
    {
        // Offset by one if we are on an odd row
        float rowOffset = rowNum % 2 == 0 ? 0 : hexInnerRadius;
        float localX = colNum * hexInnerRadius * 2 + rowOffset;
        float localY = -rowNum * hexOuterRadius * 3 / 2;

        return new Vector3(localX, localY);
    }

    private bool CheckForBounds(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < rowCount && coord.y >= 0 && coord.y < colCount;
    }

    public Bubble GetBubble(Vector2Int coord)
    {
        if (CheckForBounds(coord))
        {
            // Debug.Log("CoordX: " + coord.x + "CoordY: " + coord.y + "colCount: " + colCount + "rowCount: " + rowCount);
            return _bubbleGrid[coord.x][coord.y];
        }

        return null;
    }

    public void SetBubble(Vector2Int coord, Bubble bubble)
    {
        _bubbleGrid[coord.x][coord.y] = bubble;
    }
}