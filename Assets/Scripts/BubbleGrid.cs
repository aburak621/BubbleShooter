using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BubbleGrid : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private int rowCount = 11;
    [SerializeField] private int colCount = 9;
    [SerializeField] private List<AudioClip> poppingSounds;

    public event EventHandler BubblePlaced;

    private float _hexInnerRadius; // Radius of the inner circle of the hex
    private float _hexOuterRadius; // Radius of the outer circle of the hex
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
        transform.position = CalculateGridPosition(rowCount);

        // Populate and create the grid
        FillGridRandomly();
        InitializeGrid();
    }

    /**
     * Calculates what size the bubbles should be according to the camera's size and column count.
     */
    private void CalculateSizes()
    {
        _hexInnerRadius = _mainCamera.orthographicSize * _mainCamera.aspect / (colCount + 0.5f);
        _hexOuterRadius = _hexInnerRadius * 2 / Mathf.Sqrt(3);

        float bubbleScale = _hexInnerRadius / bubblePrefab.GetComponent<SpriteRenderer>().sprite.bounds.extents.x;
        bubblePrefab.transform.localScale = new Vector3(bubbleScale, bubbleScale, 1.0f);
    }

    /**
     * Sets the grid's initial position according to its height and width.
     */
    private Vector3 CalculateGridPosition(int rowCount, float yOffset = 0)
    {
        float gridX = -_mainCamera.orthographicSize * _mainCamera.aspect + _hexInnerRadius;
        float gridY = _mainCamera.orthographicSize - yOffset - _hexInnerRadius;
        float gridHeight = (rowCount - 1) * _hexOuterRadius * 3 / 2 + _hexInnerRadius;

        // If the grid is taller than the half size of the screen, push it upwards until enough rows are visible to fill the half of the screen
        if (gridHeight > _mainCamera.orthographicSize)
        {
            float gridOffset = (int)((_mainCamera.orthographicSize - _hexInnerRadius * 2) / (_hexOuterRadius * 3 / 2)) *
                _hexOuterRadius * 3 / 2;
            gridY += gridHeight - gridOffset - _hexInnerRadius;
        }

        return new Vector3(gridX, gridY, 0.0f);
    }

    /**
     * Fills the grid data with randomly colored bubbles.
     */
    private void FillGridRandomly()
    {
        for (int i = 0; i < rowCount; i++)
        {
            List<Bubble.BubbleColor> row = new List<Bubble.BubbleColor>();
            for (int j = 0; j < colCount; j++)
            {
                row.Add((Bubble.BubbleColor)Random.Range(0, Enum.GetValues(typeof(Bubble.BubbleColor)).Length));
            }

            _gridData.Add(row);
        }
    }

    /**
     * Initializes the Bubble objects according to the grid data and adds them to the grid.
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

    /**
     * Calculates the new position grid should be in and interpolate to it over time.
     */
    private void UpdateGridPosition()
    {
        Vector3 initialPosition = transform.position;

        // Find the last non empty row to place the grid according to it
        int lastNonEmptyRow = -1;
        for (int i = _bubbleGrid.Count - 1; i >= 0; i--)
        {
            for (int j = 0; j < _bubbleGrid[i].Count; j++)
            {
                if (_bubbleGrid[i][j] != null)
                {
                    lastNonEmptyRow = i;
                    break;
                }
            }

            if (lastNonEmptyRow != -1)
            {
                break;
            }
        }

        Vector3 targetPosition = CalculateGridPosition(lastNonEmptyRow + 1);
        float duration = Mathf.Lerp(0.25f, 0.5f, (initialPosition - targetPosition).magnitude / 5.0f);

        StartCoroutine(InterpolateGridPosition(initialPosition, targetPosition, duration));
    }

    /**
     * Interpolates the grid to its new position over time smoothly.
     */
    private IEnumerator InterpolateGridPosition(Vector3 initialPosition, Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            float ratio = elapsedTime / duration;

            transform.position = Vector3.Lerp(initialPosition, targetPosition, ratio);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = targetPosition;
    }

    /**
     * Places the new bubble to the correct position in the grid, and resolves matches.
     */
    public void HandleNewBubble(Bubble thrownBubble, Bubble gridBubble)
    {
        Vector2Int newBubbleCoordinate = new Vector2Int();

        if (gridBubble != null)
        {
            // Calculate in which angle the bubble hit
            Vector2 differenceVector = thrownBubble.transform.localPosition - gridBubble.transform.localPosition;
            float angleRadians = Mathf.Atan2(differenceVector.y, differenceVector.x);
            float angleDegrees = angleRadians * Mathf.Rad2Deg;
            if (angleDegrees < 0)
            {
                angleDegrees += 360;
            }

            // Place the bubble to the grid accordingly
            // Sides start from the hexagon's right side and goes counter clockwise
            // 0 => right, 1 => upper right, 2 => upper left, 3 => left, 4 => lower left, 5 => lower right
            float side = ((angleDegrees + 30) % 360) / 60;
            int sideIndex = (int)((angleDegrees + 30) % 360) / 60;
            // If that side is not empty we will offset it to the closest other side
            int sideOffset = side % 1 >= 0.5 ? 1 : 5;

            newBubbleCoordinate = gridBubble.gridCoordinate +
                                  (gridBubble.gridCoordinate.x % 2 == 0
                                      ? _evenNeighborOffsets[sideIndex]
                                      : _oddNeighborOffsets[sideIndex]);

            // Add a new row if the bubble is placed in the bottom
            if (newBubbleCoordinate.x >= _bubbleGrid.Count)
            {
                AddEmptyRow();
            }

            // If the hex is not empty or it is out of bounds, find the closest available hex
            for (int i = 1;
                 i <= 6 && !CheckForBounds(newBubbleCoordinate) || GetBubble(newBubbleCoordinate) != null;
                 i++)
            {
                newBubbleCoordinate = gridBubble.gridCoordinate +
                                      (gridBubble.gridCoordinate.x % 2 == 0
                                          ? _evenNeighborOffsets[(sideIndex + sideOffset * i) % 6]
                                          : _oddNeighborOffsets[(sideIndex + sideOffset * i) % 6]);
            }

            // Add a new row if the bubble is placed in the bottom
            if (newBubbleCoordinate.x >= _bubbleGrid.Count)
            {
                AddEmptyRow();
            }
        }
        else
        {
            // If the gridBubble is null that means the bubble went beyond the first row without colliding with any bubbles
            // Place the bubble to the closest empty hex in the first row
            float minDistance = 999.0f;

            for (int i = 0; i < _bubbleGrid[0].Count; i++)
            {
                if (_bubbleGrid[0][i] != null)
                {
                    continue;
                }

                float distance = (thrownBubble.transform.position - (CalculateLocalPosition(0, i) + transform.position))
                    .magnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    newBubbleCoordinate = new Vector2Int(0, i);
                }
            }
        }


        // Set the attributes of the thrown bubble now that it is a bubble on the grid
        thrownBubble.transform.SetParent(transform);
        thrownBubble.currentBubble = false;
        thrownBubble.thrown = false;
        thrownBubble.trail.enabled = false;
        thrownBubble.rb.velocity = Vector2.zero;
        thrownBubble.gridCoordinate = newBubbleCoordinate;
        SetBubble(thrownBubble.gridCoordinate, thrownBubble);

        // Interpolate the bubble to its grid position and handle the matching
        Vector3 initialPosition = thrownBubble.transform.localPosition;
        Vector3 targetPosition = CalculateLocalPosition(newBubbleCoordinate.x, newBubbleCoordinate.y);
        StartCoroutine(InterpolateBubblePosition(thrownBubble, initialPosition, targetPosition, 0.075f));
    }

    /**
     * Interpolates the bubble to its new grid position over time smoothly.
     */
    private IEnumerator InterpolateBubblePosition(Bubble bubble, Vector3 initialPosition, Vector3 targetPosition,
        float duration)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            float ratio = elapsedTime / duration;

            bubble.transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, ratio);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        bubble.transform.localPosition = targetPosition;

        // Handle the matching at the end of the lerp
        HandleMatch(bubble.gridCoordinate);
    }

    /**
     * Checks for connected bubbles to the given Bubble and destroys them if there is more than 3 matches.
     * After that, checks for any floating bubbles that are present in the grid and destroys them too.
     */
    private void HandleMatch(Vector2Int thrownBubbleCoord)
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        // Find the matching bubbles
        DfsIslands(thrownBubbleCoord, visited);

        // Don't pop any bubbles if the match count is less than 3
        if (visited.Count < 3)
        {
            // Send the placed signal
            BubblePlaced?.Invoke(this, EventArgs.Empty);

            UpdateGridPosition();
            return;
        }

        // Remove the matching bubbles from the grid and add them to the list of bubbles to be destroyed
        List<Bubble> bubblesToPop = new List<Bubble>();
        foreach (Vector2Int bubbleCoord in visited)
        {
            bubblesToPop.Add(GetBubble(bubbleCoord));
            SetBubble(bubbleCoord, null);
        }

        // Find and add the floating bubbles to the list of bubbles to be destroyed
        bubblesToPop.AddRange(ClearFloatingIslands());
        // Pop the bubbles one by one
        StartCoroutine(ChainDestroyBubbles(bubblesToPop));
    }

    /**
     * Using a depth first search algorithm, finds the connected islands to the given Bubble.
     * Can be constrained to check to only the same colored bubbles or all bubbles.
     */
    private void DfsIslands(Vector2Int bubbleCoord, HashSet<Vector2Int> visited, bool searchForSameColor = true)
    {
        // If we have visited the this coordinate before, skip it
        if (visited.Contains(bubbleCoord))
        {
            return;
        }

        // Add the current coordinate to the visited list
        visited.Add(bubbleCoord);

        // Check every neighbor and call this function recursively on them
        List<Vector2Int> neighborOffsets = bubbleCoord.x % 2 == 0 ? _evenNeighborOffsets : _oddNeighborOffsets;
        foreach (Vector2Int offset in neighborOffsets)
        {
            Vector2Int neighborCoord = bubbleCoord + offset;
            Bubble neighbor = GetBubble(neighborCoord);

            if (neighbor == null)
            {
                continue;
            }

            // Can check for same colored bubbles or all bubbles
            if (!searchForSameColor || GetBubble(bubbleCoord).GetBubbleColor() == neighbor.GetBubbleColor())
            {
                // Recursively call this function for the neighbors
                DfsIslands(neighborCoord, visited, searchForSameColor);
            }
        }
    }

    /**
     * Finds the floating islands by finding the bubbles that are not connected to the root (topmost) bubbles and destroys them.
     */
    private List<Bubble> ClearFloatingIslands()
    {
        HashSet<Vector2Int> connectedToRoot = new HashSet<Vector2Int>();

        // For all bubbles in the first row, find all connected bubbles to them
        foreach (Bubble rootBubble in _bubbleGrid[0])
        {
            if (rootBubble == null)
            {
                continue;
            }

            DfsIslands(rootBubble.gridCoordinate, connectedToRoot, false);
        }

        // Find the bubbles that are not in the connected bubbles list
        List<Bubble> floatingBubbles = new List<Bubble>();
        foreach (List<Bubble> row in _bubbleGrid)
        {
            foreach (Bubble bubble in row.ToList())
            {
                if (bubble == null)
                {
                    continue;
                }

                if (!connectedToRoot.Contains(bubble.gridCoordinate))
                {
                    // Add them to a list and remove them from the grid
                    floatingBubbles.Add(bubble);
                    SetBubble(bubble.gridCoordinate, null);
                }
            }
        }

        return floatingBubbles;
    }

    /**
     * Visually pops the bubble from the grid with a random velocity and destroys it after a delay.
     */
    private void DestroyBubble(Bubble bubble)
    {
        bubble.rb.bodyType = RigidbodyType2D.Dynamic;
        float speed = 3.0f;
        bubble.rb.velocity = new Vector2(Random.Range(-speed, speed), Random.Range(speed * 0.5f, speed));

        bubble.circleCollider.enabled = false;

        bubble.spriteRenderer.sortingOrder = 1;

        Destroy(bubble.gameObject, 2.0f);
    }

    /**
     * Pops the bubbles one by one and plays a popping sound.
     * Lastly updates the position of the grid after every bubble is popped.
     */
    private IEnumerator ChainDestroyBubbles(List<Bubble> bubbles)
    {
        int bubbleCount = bubbles.Count;
        // Calculate a delay according to the number of bubbles to be popped
        float delayBetweenPops = Mathf.Lerp(0.3f, 0.7f, bubbleCount / 10.0f) / bubbleCount;

        for (int i = 0; i < bubbleCount; i++)
        {
            AudioSource.PlayClipAtPoint(poppingSounds[Random.Range(0, poppingSounds.Count)],
                _mainCamera.transform.position, 0.3f);
            DestroyBubble(bubbles[i]);

            yield return new WaitForSeconds(delayBetweenPops);
        }

        // Send the placed signal
        BubblePlaced?.Invoke(this, EventArgs.Empty);

        UpdateGridPosition();
    }

    /**
     * Adds an empty row to the end of the grid.
     */
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

    /**
     * Returns the position relative to the grid for the specified cell coordinates.
     */
    private Vector3 CalculateLocalPosition(int rowNum, int colNum)
    {
        // Offset by one if we are on an odd row
        float rowOffset = rowNum % 2 == 0 ? 0 : _hexInnerRadius;
        float localX = colNum * _hexInnerRadius * 2 + rowOffset;
        float localY = -rowNum * _hexOuterRadius * 3 / 2;

        return new Vector3(localX, localY);
    }

    /**
     * Returns if the coordinates are inside the grid bounds.
     */
    private bool CheckForBounds(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < rowCount && coord.y >= 0 && coord.y < colCount;
    }

    /**
     * Returns the Bubble object in the given coordinates. If it is empty, returns null.
     */
    public Bubble GetBubble(Vector2Int coord)
    {
        if (CheckForBounds(coord))
        {
            return _bubbleGrid[coord.x][coord.y];
        }

        return null;
    }

    /**
     * Puts the given Bubble object to the specified coordinates in the grid.
     */
    private void SetBubble(Vector2Int coord, Bubble bubble)
    {
        _bubbleGrid[coord.x][coord.y] = bubble;
    }
}