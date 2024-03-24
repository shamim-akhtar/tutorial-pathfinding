using JetBrains.Annotations;
using PathFinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMap : MonoBehaviour
{
  [SerializeField]
  int numX;
  [SerializeField]
  int numY;

  [SerializeField]
  GameObject gridNodeViewPrefab;

  [SerializeField]
  bool allowDiagonalMovement = true;

  public Color COLOR_WALKABLE = new Color(0.4f, 0.4f, 0.8f, 1.0f);
  public Color COLOR_NONWALKABLE = Color.black;
  public Color COLOR_CURRENT_NODE = Color.cyan;
  public Color COLOR_ADD_TO_OPENLIST = Color.green;
  public Color COLOR_ADD_TO_CLOSEDLIST = Color.grey;
  public Color COLOR_PATH = Color.blue;

  public int NumX { get { return numX; } }
  public int NumY { get { return numY; } }

  [SerializeField]
  NPC npc;

  [SerializeField]
  Transform destination;

  float gridNodeWidth = 1.0f;
  float gridNodeHeight = 1.0f;

  public float GridNodeWidth { get { return gridNodeWidth; } }
  public float GridNodeHeight { get { return gridNodeHeight; } }

  private GridNodeView[,] gridNodeViews = null;

  // Find the dimensions of the prefab.
  private void GetGridNodeViewSize()
  {
    BoxCollider2D boxc = gridNodeViewPrefab.GetComponent<BoxCollider2D>();
    if(boxc != null )
    {
      gridNodeWidth = boxc.size.x;
      gridNodeHeight= boxc.size.y;
    }
  }

  // Start is called before the first frame update
  void Start()
  {
    gridNodeViews = new GridNodeView[NumX,NumY];
    for(int i = 0; i < NumX; i++)
    {
      for(int j = 0; j < NumY; j++)
      {
        GameObject obj = Instantiate(
          gridNodeViewPrefab,
          new Vector3(
            i * GridNodeWidth,
            j * GridNodeHeight,
            0.0f),
          Quaternion.identity);

        obj.name = "GridNode_" + i.ToString() + "_" + j.ToString();
        GridNodeView gnv = obj.GetComponent<GridNodeView>();
        gridNodeViews[i,j] = gnv;
        gnv.Node = new GridNode(new Vector2Int(i, j), this);

        obj.transform.SetParent(transform);
      }
    }

    SetCameraPosition();
    npc.Map = this;
    npc.SetStartNode(gridNodeViews[0, 0].Node);
  }

  void SetCameraPosition()
  {
    Camera.main.transform.position = new Vector3(
      ((numX - 1) * GridNodeWidth) / 2,
      ((numY - 1) * GridNodeHeight) / 2,
      -100.0f);

    float min_value = Mathf.Min((numX - 1) * GridNodeWidth, (numY - 1) * GridNodeHeight);
    Camera.main.orthographicSize = min_value * 0.75f;
  }

  public List<PathFinding.Node<Vector2Int>> GetNeighbours(PathFinding.Node<Vector2Int> loc)
  {
    List<PathFinding.Node<Vector2Int>> neighbours = new List<PathFinding.Node<Vector2Int>>();

    int x = loc.Value.x;
    int y = loc.Value.y;

    // Check Up.
    if(y < numY - 1)
    {
      int i = x;
      int j = y + 1;
      if (gridNodeViews[i,j].Node.IsWalkable)
      {
        neighbours.Add(gridNodeViews[i,j].Node);
      }
    }

    // Check Right
    if(x < numX - 1)
    {
      int i = x + 1;
      int j = y;
      if (gridNodeViews[i, j].Node.IsWalkable)
      {
        neighbours.Add(gridNodeViews[i, j].Node);
      }
    }

    // Check Down
    if(y > 0)
    {
      int i = x;
      int j = y - 1;
      if (gridNodeViews[i, j].Node.IsWalkable)
      {
        neighbours.Add(gridNodeViews[i, j].Node);
      }
    }

    // Check Left
    if(x > 0)
    {
      int i = x - 1;
      int j = y;

      if (gridNodeViews[i, j].Node.IsWalkable)
      {
        neighbours.Add(gridNodeViews[i, j].Node);
      }
    }

    if(allowDiagonalMovement)
    {
      // Check Top Right
      if(y < numY- 1 && x < numX - 1) 
      {
        int i = x + 1;
        int j = y + 1;

        if (gridNodeViews[i, j].Node.IsWalkable)
        {
          neighbours.Add(gridNodeViews[i, j].Node);
        }
      }
      //Check Right Down.
      if(x < numX - 1 && y > 0)
      {
        int i = x + 1;
        int j = y - 1;

        if (gridNodeViews[i, j].Node.IsWalkable)
        {
          neighbours.Add(gridNodeViews[i, j].Node);
        }
      }
      // Check Down Left
      if(y > 0 && x > 0)
      {
        int i = x - 1;
        int j = y - 1;

        if (gridNodeViews[i, j].Node.IsWalkable)
        {
          neighbours.Add(gridNodeViews[i, j].Node);
        }
      }

      // Check Left Top
      if(x > 0 && y < numY - 1)
      {
        int i = x - 1;
        int j = y + 1;

        if (gridNodeViews[i, j].Node.IsWalkable)
        {
          neighbours.Add(gridNodeViews[i, j].Node);
        }
      }
    }

    return neighbours;
  }

  public void RayCastAndToggleWalkable()
  {
    Vector2 rayPos = new Vector2(
      Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
      Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
    RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

    if(hit)
    {
      GameObject obj = hit.transform.gameObject;
      GridNodeView gnv = obj.GetComponent<GridNodeView>();
      ToggleWalkable(gnv);
    }
  }

  public void RayCastAndSetDestination()
  {
    Vector2 rayPos = new Vector2(
      Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
      Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
    RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

    if (hit)
    {
      GameObject obj = hit.transform.gameObject;
      GridNodeView gnv = obj.GetComponent<GridNodeView>();

      Vector3 pos = destination.position;
      pos.x = gnv.Node.Value.x * gridNodeWidth;
      pos.y = gnv.Node.Value.y * gridNodeHeight;
      destination.position = pos;

      npc.MoveTo(gnv.Node);
    }
  }

  public void ToggleWalkable(GridNodeView gnv)
  {
    if (gnv == null)
      return;

    int x = gnv.Node.Value.x;
    int y = gnv.Node.Value.y;

    gnv.Node.IsWalkable = !gnv.Node.IsWalkable;

    if(gnv.Node.IsWalkable)
    {
      gnv.SetInnerColor(COLOR_WALKABLE);
    }
    else
    {
      gnv.SetInnerColor(COLOR_NONWALKABLE);
    }
  }


  // Update is called once per frame
  void Update()
  {
    if(Input.GetMouseButtonDown(0))
    {
      RayCastAndToggleWalkable();
    }
    if (Input.GetMouseButtonDown(1))
    {
      RayCastAndSetDestination();
    }
  }

  public GridNode GetGridNode(int x, int y)
  {
    if(x >= 0 && x < numX && y >= 0 && y < numY)
    {
      return gridNodeViews[x, y].Node;
    }
    return null;
  }

  public GridNodeView GetGridNodeView(int x, int y)
  {
    if (x >= 0 && x < numX && y >= 0 && y < numY)
    {
      return gridNodeViews[x, y];
    }
    return null;
  }

  // The various cost functions.
  public static float GetManhattanCost(
    Vector2Int a,
    Vector2Int b)
  {
    return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
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

  public static float GetEuclideanCost(
    Vector2Int a,
    Vector2Int b)
  {
    return GetCostBetweenTwoCells(a, b);
  }

  public void OnChangeCurrentNode(PathFinding.PathFinder<Vector2Int>.PathFinderNode node)
  {
    int x = node.Location.Value.x;
    int y = node.Location.Value.y;
    GridNodeView gnv = gridNodeViews[x, y];
    gnv.SetInnerColor(COLOR_CURRENT_NODE);
  }

  public void OnAddToOpenList(PathFinding.PathFinder<Vector2Int>.PathFinderNode node)
  {
    int x = node.Location.Value.x;
    int y = node.Location.Value.y;
    GridNodeView gnv = gridNodeViews[x, y];
    gnv.SetInnerColor(COLOR_ADD_TO_OPENLIST);
  }

  public void OnAddToClosedList(PathFinding.PathFinder<Vector2Int>.PathFinderNode node)
  {
    int x = node.Location.Value.x;
    int y = node.Location.Value.y;
    GridNodeView gnv = gridNodeViews[x, y];
    gnv.SetInnerColor(COLOR_ADD_TO_CLOSEDLIST);
  }

  public void ResetGridNodeColours()
  {
    for(int i = 0; i < numX; ++i)
    {
      for(int j = 0; j < numY; ++j)
      {
        GridNodeView gnv = gridNodeViews[i, j];
        if(gnv.Node.IsWalkable)
        {
          gnv.SetInnerColor(COLOR_WALKABLE);
        }
        else
        {
          gnv.SetInnerColor(COLOR_NONWALKABLE);
        }
      }
    }
  }
}
