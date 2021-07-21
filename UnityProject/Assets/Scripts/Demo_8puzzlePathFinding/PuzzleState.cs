using System;

namespace Puzzle
{
  public class PuzzleState : IEquatable<PuzzleState>
  {
    // The integer array representing the internal state
    // of the puzzle.
    public int[] Arr
    {
      get;
      private set;
    }
    // The number of rows or columns of the puzzle.
    public int NumRowsOrCols
    {
      get;
    }

    // A private variable to store the empty 
    // tile index.
    private int mEmptyTileIndex;

    // Get the empty tile index
    public int GetEmptyTileIndex()
    {
      return mEmptyTileIndex;
    }

    // Constructor.
    public PuzzleState(int rows_or_cols)
    {
      NumRowsOrCols = rows_or_cols;
      Arr = new int[NumRowsOrCols * NumRowsOrCols];
      for (int i = 0; i < Arr.Length; ++i)
      {
        Arr[i] = i;
      }
      mEmptyTileIndex = Arr.Length - 1;
    }

    // Construct from an integer array of state.
    public PuzzleState(int[] arr)
    {
      NumRowsOrCols = (int)System.Math.Sqrt(arr.Length);

      Arr = new int[NumRowsOrCols * NumRowsOrCols];
      for (int i = 0; i < Arr.Length; ++i)
      {
        Arr[i] = arr[i];
        if (arr[i] == (Arr.Length - 1)) mEmptyTileIndex = i;
      }
    }

    // Construct from another state.
    public PuzzleState(PuzzleState other)
    {
      NumRowsOrCols = other.NumRowsOrCols;
      mEmptyTileIndex = other.mEmptyTileIndex;
      Arr = new int[NumRowsOrCols * NumRowsOrCols];
      other.Arr.CopyTo(Arr, 0);
    }

    // To check if two states are equal.
    public static bool Equals(PuzzleState a, PuzzleState b)
    {
      for (int i = 0; i < a.Arr.Length; i++)
      {
        if (a.Arr[i] != b.Arr[i]) return false;
      }
      return true;
    }

    public bool Equals(PuzzleState other)
    {
      if (other is null)
        return false;

      return Equals(this, other);
    }

    public override bool Equals(object obj) => Equals(obj as PuzzleState);
    public override int GetHashCode()
    {
      int hc = Arr.Length;
      foreach (int val in Arr)
      {
        hc = unchecked(hc * 314159 + val);
      }
      return hc;
    }

    public int FindEmptyTileIndex()
    {
      for (int i = 0; i < Arr.Length; i++)
        if (Arr[i] == Arr.Length - 1) return i;
      return Arr.Length;
    }

    public void SwapWithEmpty(int index)
    {
      int tmp = Arr[index];
      Arr[index] = Arr[mEmptyTileIndex];
      Arr[mEmptyTileIndex] = tmp;
      mEmptyTileIndex = index;
    }

    public int GethammingCost()
    {
      int cost = 0;
      for (int i = 0; i < Arr.Length; ++i)
      {
        if (Arr[i] == Arr.Length - 1) continue;
        if (Arr[i] != i + 1) cost += 1;
      }
      return cost;
    }

    public int GetManhattanCost()
    {
      int cost = 0;
      for (int i = 0; i < Arr.Length; ++i)
      {
        int v = Arr[i];
        if (v == Arr.Length - 1) continue;

        int gx = v % NumRowsOrCols;
        int gy = v / NumRowsOrCols;

        int x = i % NumRowsOrCols;
        int y = i / NumRowsOrCols;

        int mancost = System.Math.Abs(x - gx) + System.Math.Abs(y - gy);
        cost += mancost;
      }
      return cost;
    }
  };
}
