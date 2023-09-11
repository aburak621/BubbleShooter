using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid<TGridObject>
{
    private int _width;
    private int _height;
    private List<List<TGridObject>> _gridObjectList;

    protected float _cellSize;

    public Grid(int width, int height, float cellSize = 1.0f)
    {
        _width = width;
        _height = height;
        _cellSize = cellSize;
        _gridObjectList = new List<List<TGridObject>>();

        InitializeGrid();
    }

    public void InitializeGrid()
    {
        _gridObjectList.Clear();

        for (int i = 0; i < _width; i++)
        {
            _gridObjectList.Add(new List<TGridObject>());
            for (int j = 0; j < _height; j++)
            {
                _gridObjectList[i].Add(default);
            }
        }
    }

    public virtual Vector3 GetLocalPosition(GridCoordinate gridCoordinate)
    {
        return new Vector3(gridCoordinate.X * _cellSize, gridCoordinate.Y * _cellSize);
    }

    public virtual GridCoordinate GetGridCoordinate(Vector3 localPosition)
    {
        return new GridCoordinate(
            Mathf.RoundToInt(localPosition.x / _cellSize),
            Mathf.RoundToInt(localPosition.y / _cellSize)
        );
    }

    public List<TGridObject> GetNeighbors(GridCoordinate gridCoordinate)
    {
        List<TGridObject> neighbors = new List<TGridObject>();
        foreach (GridCoordinate coordinate in GetNeighborCoordinates(gridCoordinate))
        {
            if (TryGet(coordinate, out TGridObject neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }
    
    public List<GridCoordinate> GetNeighborCoordinates(GridCoordinate gridCoordinate)
    {
        List<GridCoordinate> neighborsCoords = new List<GridCoordinate>();
        foreach (GridCoordinate neighborOffset in GetNeighborOffsets(gridCoordinate))
        {
            neighborsCoords.Add(gridCoordinate + neighborOffset);
        }
        
        return neighborsCoords;
    }

    public virtual List<GridCoordinate> GetNeighborOffsets(GridCoordinate gridCoordinate)
    {
        return new List<GridCoordinate>(
            new GridCoordinate[]
            {
                new GridCoordinate(0, 1),
                new GridCoordinate(1, 0),
                new GridCoordinate(0, -1),
                new GridCoordinate(-1, 0),
            }
        );
    }

    public bool TrySet(GridCoordinate gridCoordinate, TGridObject gridObject)
    {
        if (IsValidGridCoordinate(gridCoordinate))
        {
            return false;
        }

        _gridObjectList[gridCoordinate.X][gridCoordinate.Y] = gridObject;
        return true;
    }

    public bool TryGet(GridCoordinate gridCoordinate, out TGridObject gridObject)
    {
        bool validCoordinate = IsValidGridCoordinate(gridCoordinate);
        gridObject = validCoordinate ? _gridObjectList[gridCoordinate.X][gridCoordinate.Y] : default;
        return validCoordinate;
    }

    private bool IsValidGridCoordinate(GridCoordinate gridCoordinate)
    {
        return gridCoordinate.X >= 0 &&
               gridCoordinate.Y >= 0 &&
               gridCoordinate.X < _width &&
               gridCoordinate.Y < _height;
    }

    public int GetWidth()
    {
        return _width;
    }

    public int GetHeight()
    {
        return _height;
    }

    public void SetWidth(int width)
    {
        _width = width;
    }

    public void SetHeight(int height)
    {
        _height = height;
    }
}

public struct GridCoordinate
{
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public int X;
    public int Y;

    public GridCoordinate(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static GridCoordinate operator +(GridCoordinate left, GridCoordinate right)
    {
        return new GridCoordinate(left.X + right.X, left.Y + right.Y);
    }

    public static GridCoordinate operator -(GridCoordinate left, GridCoordinate right)
    {
        return new GridCoordinate(left.X - right.X, left.Y - right.Y);
    }

    public static bool operator ==(GridCoordinate left, GridCoordinate right)
    {
        return left.X == right.X && left.Y == right.Y;
    }

    public static bool operator !=(GridCoordinate left, GridCoordinate right)
    {
        return !(left == right);
    }

    public bool Equals(GridCoordinate other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object obj)
    {
        return obj is GridCoordinate other && Equals(other);
    }

    public override string ToString()
    {
        return $"X: {X}, Y: {Y}";
    }
}
