using System.Collections.Generic;
using UnityEngine;

// This is the main node type for a grid
// based map. We will derive this class from
// Pathfinding.Node generic class.
// For a 2D rectangular grid based map we 
// will simply use the Vector2Int class to 
// represent the location of the cell.
public class GridNode : PathFinding.Node<Vector2Int>
{
  // Is this cell walkable?
  public bool IsWalkable { get; set; }

  // Keep a reference to the grid so that 
  // we can find the neighbours.
  private GridMap gridMap;

  // construct the node with the grid and the location.
  public GridNode( Vector2Int value, GridMap gridMap)
    : base(value)
  {
    //this.gridMap = gridMap;

    // by default we set the cell to be walkable.
    IsWalkable = true;
    this.gridMap = gridMap;
  }

  // get the neighbours for this cell.
  // here will will just throw the responsibility
  // to get the neighbours to the grid.
  public override List<PathFinding.Node<Vector2Int>> GetNeighbours()
  {
    //return new List<PathFinding.Node<Vector2Int>>();
    return gridMap.GetNeighbourCells(this);
  }
}