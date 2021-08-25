using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class RectGrid
{
  public int mX { get; private set; }
  public int mY { get; private set; }

  public class Cell : Node<Vector2Int>
  {
    public bool walkable = true;

    RectGrid mGrid;

    public Cell(Vector2Int index, RectGrid grid)
      : base(index)
    {
      mGrid = grid;
    }
    // get the neighbours for this cell.
    // here will will just throw the responsibility
    // to get the neighbours to the grid.
    public override List<Node<Vector2Int>> GetNeighbours()
    {
      return mGrid.GetNeighbourCells(this);
    }
  }

  public Cell[,] mCells { get; private set; }

  public RectGrid(int numX, int numY)
  {
    mX = numX;
    mY = numY;

    mCells = new Cell[mX, mY];

    for (int i = 0; i < mX; ++i)
    {
      for (int j = 0; j < mY; ++j)
      {
        mCells[i, j] = new Cell(new Vector2Int(i, j), this)
        {
          walkable = true
        };
      }
    }
  }

  // get neighbour cells for a given cell.
  public List<Node<Vector2Int>> GetNeighbourCells(Node<Vector2Int> loc)
  {
    List<Node<Vector2Int>> neighbours = new List<Node<Vector2Int>>();

    int x = loc.Value.x;
    int y = loc.Value.y;

    // Check up.
    if (y < mY - 1)
    {
      int i = x;
      int j = y + 1;

      if (mCells[i, j].walkable)
      {
        neighbours.Add(mCells[i, j]);
      }
    }
    // Check top-right
    if (y < mY - 1 && x < mX - 1)
    {
      int i = x + 1;
      int j = y + 1;

      if (mCells[i, j].walkable)
      {
        neighbours.Add(mCells[i, j]);
      }
    }
    // Check right
    if (x < mX - 1)
    {
      int i = x + 1;
      int j = y;

      if (mCells[i, j].walkable)
      {
        neighbours.Add(mCells[i, j]);
      }
    }
    // Check right-down
    if (x < mX - 1 && y > 0)
    {
      int i = x + 1;
      int j = y - 1;

      if (mCells[i, j].walkable)
      {
        neighbours.Add(mCells[i, j]);
      }
    }
    // Check down
    if (y > 0)
    {
      int i = x;
      int j = y - 1;

      if (mCells[i, j].walkable)
      {
        neighbours.Add(mCells[i, j]);
      }
    }
    // Check down-left
    if (y > 0 && x > 0)
    {
      int i = x - 1;
      int j = y - 1;

      if (mCells[i, j].walkable)
      {
        neighbours.Add(mCells[i, j]);
      }
    }
    // Check left
    if (x > 0)
    {
      int i = x - 1;
      int j = y;

      if (mCells[i, j].walkable)
      {
        neighbours.Add(mCells[i, j]);
      }
    }
    // Check left-top
    if (x > 0 && y < mY - 1)
    {
      int i = x - 1;
      int j = y + 1;

      if (mCells[i, j].walkable)
      {
        neighbours.Add(mCells[i, j]);
      }
    }

    return neighbours;
  }

  public static float GetManhattanCost(
    Vector2Int a,
    Vector2Int b)
  {
    return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
  }

  public static float GetEuclideanCost(
    Vector2Int a,
    Vector2Int b)
  {
    return GetCostBetweenTwoCells(a, b);
  }

  public static float GetCostBetweenTwoCells(
    Vector2Int a,
    Vector2Int b)
  {
    return Mathf.Sqrt(
            (a.x - b.x) * (a.x - b.x) +
            (a.y - b.y) * (a.y - b.y)
        );
  }
}