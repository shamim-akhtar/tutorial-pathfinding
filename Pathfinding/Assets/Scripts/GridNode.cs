using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNode : PathFinding.Node<Vector2Int>
{
  public bool IsWalkable { get; set; }

  private GridMap gridMap;

  public GridNode(Vector2Int value, GridMap gridMap) 
    : base(value)
  {
    IsWalkable = true;
    this.gridMap = gridMap;
  }

  public override 
    List<PathFinding.Node<Vector2Int>> GetNeighbours()
  {
    // Return an empty list for now.
    // Later we will call gridMap's GetNeighbours
    // function.
    //return new List<PathFinding.Node<Vector2Int>>();
    return gridMap.GetNeighbours(this);
  }
}
