using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridIslandOperations<TGridObject>
{
    private Grid<TGridObject> _grid;

    public GridIslandOperations(Grid<TGridObject> grid)
    {
        _grid = grid;
    }

    public void GetIsland(GridCoordinate gridCoordinate, HashSet<GridCoordinate> island,
        Func<GridCoordinate, bool> islandCondition)
    {
        if (island.Contains(gridCoordinate) || !islandCondition(gridCoordinate))
        {
            return;
        }

        foreach (GridCoordinate offset in _grid.GetNeighborOffsets(gridCoordinate))
        {
            GridCoordinate neighborCoordinate = gridCoordinate + offset;

            if (!_grid.TryGet(gridCoordinate, out TGridObject gridObject) || gridObject == null)
            {
                continue;
            }

            GetIsland(neighborCoordinate, island, islandCondition);
        }
    }
}