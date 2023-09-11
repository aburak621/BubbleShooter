using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid<TGridObject> : Grid<TGridObject>
{
    private float _hexInnerRadius;
    private float _hexOuterRadius;

    public HexGrid(int width, int height, float cellSize = 1) : base(width, height, cellSize)
    {
        _hexInnerRadius = cellSize;
        _hexOuterRadius = _hexInnerRadius * 2 / Mathf.Sqrt(3);
    }

    public override GridCoordinate GetGridCoordinate(Vector3 localPosition)
    {
        GridCoordinate approxCoord = new GridCoordinate(
                Mathf.RoundToInt(localPosition.x / (_hexInnerRadius * 2)),
                Mathf.RoundToInt(localPosition.y / (_hexOuterRadius * 1.5f))
                );
        
        Debug.Log("Approx:" + approxCoord);

        GridCoordinate closestCoord = approxCoord;

        foreach (GridCoordinate neighborCoord in GetNeighborCoordinates(approxCoord))
        {
            if (Vector3.Distance(localPosition, GetLocalPosition(neighborCoord)) <
                Vector3.Distance(localPosition, GetLocalPosition(closestCoord)))
            {
                closestCoord = neighborCoord;
            }
        }

        return closestCoord;
    }

    public override Vector3 GetLocalPosition(GridCoordinate gridCoordinate)
    {
        float rowOffset = gridCoordinate.Y % 2 == 0 ? 0 : _hexInnerRadius;
        float localX = gridCoordinate.X * _hexInnerRadius * 2 + rowOffset;
        float localY = gridCoordinate.Y * _hexOuterRadius * 1.5f;

        return new Vector3(localX, localY);
    }

    public override List<GridCoordinate> GetNeighborOffsets(GridCoordinate gridCoordinate)
    {
        return gridCoordinate.X % 2 == 0
            ?
            // Even neighbors
            new List<GridCoordinate>(
                new GridCoordinate[]
                {
                    new GridCoordinate(0, 1),
                    new GridCoordinate(-1, 0),
                    new GridCoordinate(-1, -1),
                    new GridCoordinate(0, -1),
                    new GridCoordinate(1, -1),
                    new GridCoordinate(1, 0),
                })
            :
            // Odd neighbors
            new List<GridCoordinate>(
                new GridCoordinate[]
                {
                    new GridCoordinate(0, +1),
                    new GridCoordinate(-1, 1),
                    new GridCoordinate(-1, 0),
                    new GridCoordinate(0, -1),
                    new GridCoordinate(+1, 0),
                    new GridCoordinate(+1, +1),
                });
    }
}
