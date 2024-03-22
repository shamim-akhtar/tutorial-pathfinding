using PathFinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMap : MonoBehaviour
{
  [SerializeField]
  int numX = 10;
  [SerializeField]
  int numY = 10;
  [SerializeField]
  GameObject gridNodeViewPrefab;

  [SerializeField]
  Transform destination;
  [SerializeField]
  bool allowDiagonalMovement = true;

  public Color COLOR_WALKABLE = new Color(100/255.0f, 100/255.0f, 200/255.0f);
  public Color COLOR_NON_WALKABLE = Color.black;
  public Color COLOR_CURRENT_NODE = Color.yellow;
  public Color COLOR_ADD_TO_OPEN_LIST = Color.green;
  public Color COLOR_ADD_TO_CLOSED_LIST = Color.grey;

  float gridNodeWidth = 1;
  float gridNodeHeight = 1;

  public float GridNodeWidth { get { return gridNodeWidth; } }
  public float GridNodeHeight { get { return gridNodeWidth; } }

  GridNodeView[,] gridNodeViews = null;

  public int NumX { get { return numX;} }
  public int NumY { get { return numY;} }

  [SerializeField]
  NPC npc;

  private void GetRoomSize()
  {
    SpriteRenderer[] spriteRenderers =
      gridNodeViewPrefab.GetComponentsInChildren<SpriteRenderer>();

    Vector3 minBounds = Vector3.positiveInfinity;
    Vector3 maxBounds = Vector3.negativeInfinity;

    foreach (SpriteRenderer ren in spriteRenderers)
    {
      minBounds = Vector3.Min(
        minBounds,
        ren.bounds.min);

      maxBounds = Vector3.Max(
        maxBounds,
        ren.bounds.max);
    }

    gridNodeWidth = maxBounds.x - minBounds.x;
    gridNodeHeight = maxBounds.y - minBounds.y;
  }
  // Start is called before the first frame update
  void Start()
  {
    npc.Map = this;

    gridNodeViews = new GridNodeView[numX, numY];
    for (int i = 0; i < numX; ++i)
    {
      for(int j = 0; j < numY; ++j)
      {
        GameObject obj = Instantiate(gridNodeViewPrefab,
          new Vector3(i * gridNodeWidth, j * gridNodeHeight, 0.0f),
          Quaternion.identity);

        obj.name = "GridNode_" + i.ToString() + "_" + j.ToString();
        GridNodeView gridNodeView = obj.GetComponent<GridNodeView>();
        gridNodeViews[i, j] = gridNodeView;
        gridNodeView.Node = new GridNode(new Vector2Int(i, j), this);

        obj.transform.SetParent(gameObject.transform);
      }
    }
    SetCameraPosition();
    SetAllNodesWalkable();
    npc.SetStart(gridNodeViews[0, 0].Node);
  }

  private void SetAllNodesWalkable()
  {
    for (int i = 0; i < numX; ++i)
    {
      for (int j = 0; j < numY; ++j)
      {
        gridNodeViews[i, j].SetInnerColor(COLOR_WALKABLE);
      }
    }
  }

  private void SetCameraPosition()
  {
    Camera.main.transform.position = new Vector3(
      ((numX - 1) * gridNodeWidth) / 2,
      ((numY - 1) * gridNodeHeight) / 2,
      -100.0f);

    float min_value = Mathf.Min((numX-1) * gridNodeWidth, (numY-1) * gridNodeHeight);
    Camera.main.orthographicSize = min_value * 0.75f;
  }
  
  // get neighbour cells for a given cell.
  public List<Node<Vector2Int>> GetNeighbourCells(Node<Vector2Int> loc)
  {
    List<Node<Vector2Int>> neighbours = new List<Node<Vector2Int>>();

    int x = loc.Value.x;
    int y = loc.Value.y;

    // Check up.
    if (y < numY - 1)
    {
      int i = x;
      int j = y + 1;

      if (gridNodeViews[i, j].Node.IsWalkable)
      {
        neighbours.Add(gridNodeViews[i, j].Node);
      }
    }
    // Check right
    if (x < numX - 1)
    {
      int i = x + 1;
      int j = y;

      if (gridNodeViews[i, j].Node.IsWalkable)
      {
        neighbours.Add(gridNodeViews[i, j].Node);
      }
    }
    // Check down
    if (y > 0)
    {
      int i = x;
      int j = y - 1;

      if (gridNodeViews[i, j].Node.IsWalkable)
      {
        neighbours.Add(gridNodeViews[i, j].Node);
      }
    }
    // Check left
    if (x > 0)
    {
      int i = x - 1;
      int j = y;

      if (gridNodeViews[i, j].Node.IsWalkable)
      {
        neighbours.Add(gridNodeViews[i, j].Node);
      }
    }
    if (allowDiagonalMovement)
    {
      // Check top-right
      if (y < numY - 1 && x < numX - 1)
      {
        int i = x + 1;
        int j = y + 1;

        if (gridNodeViews[i, j].Node.IsWalkable)
        {
          neighbours.Add(gridNodeViews[i, j].Node);
        }
      }
      // Check right-down
      if (x < numX - 1 && y > 0)
      {
        int i = x + 1;
        int j = y - 1;

        if (gridNodeViews[i, j].Node.IsWalkable)
        {
          neighbours.Add(gridNodeViews[i, j].Node);
        }
      }
      // Check down-left
      if (y > 0 && x > 0)
      {
        int i = x - 1;
        int j = y - 1;

        if (gridNodeViews[i, j].Node.IsWalkable)
        {
          neighbours.Add(gridNodeViews[i, j].Node);
        }
      }
      // Check left-top
      if (x > 0 && y < numY - 1)
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


  // toggling of walkable/non-walkable cells.
  public void RayCastAndToggleWalkable()
  {
    Vector2 rayPos = new Vector2(
        Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
        Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
    RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

    if (hit)
    {
      GameObject obj = hit.transform.gameObject;
      GridNodeView gnv = obj.GetComponent<GridNodeView>();
      ToggleWalkable(gnv);
    }
  }
  public void ToggleWalkable(GridNodeView gnv)
  {
    if (gnv == null)
      return;

    int x = gnv.Node.Value.x;
    int y = gnv.Node.Value.y;

    gnv.Node.IsWalkable = !gnv.Node.IsWalkable;

    if (gnv.Node.IsWalkable)
    {
      gnv.SetInnerColor(COLOR_WALKABLE);
    }
    else
    {
      gnv.SetInnerColor(COLOR_NON_WALKABLE);
    }
  }

  void RayCastAndSetDestination()
  {
    Vector2 rayPos = new Vector2(
        Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
        Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
    RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

    if (hit)
    {
      GameObject obj = hit.transform.gameObject;
      GridNodeView gnv = obj.GetComponent<GridNodeView>();
      if (gnv == null) return;

      Vector3 pos = destination.position;
      pos.x = gnv.Node.Value.x;
      pos.y = gnv.Node.Value.y;
      destination.position = pos;

      // Set the destination to the NPC.
      npc.SetDestination(gnv.Node);
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetMouseButtonDown(0))
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
    if (x >= 0 && x < numX && y >= 0 && y < numY)
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

  public void ResetCellColours()
  {
    for (int i = 0; i < numX; i++)
    {
      for (int j = 0; j < numY; j++)
      {
        GridNodeView gnv = gridNodeViews[i, j];
        if (gnv.Node.IsWalkable)
        {
          gnv.SetInnerColor(COLOR_WALKABLE);
        }
        else
        {
          gnv.SetInnerColor(COLOR_NON_WALKABLE);
        }
      }
    }
  }

  public void OnChangeCurrentNode(PathFinder<Vector2Int>.PathFinderNode node)
  {
    int x = node.Location.Value.x;
    int y = node.Location.Value.y;
    GridNodeView gnv = gridNodeViews[x, y];
    gnv.SetInnerColor(COLOR_CURRENT_NODE);
  }
  public void OnAddToOpenList(PathFinder<Vector2Int>.PathFinderNode node)
  {
    int x = node.Location.Value.x;
    int y = node.Location.Value.y;
    GridNodeView gnv = gridNodeViews[x, y];
    gnv.SetInnerColor(COLOR_ADD_TO_OPEN_LIST);
  }
  public void OnAddToClosedList(PathFinder<Vector2Int>.PathFinderNode node)
  {
    int x = node.Location.Value.x;
    int y = node.Location.Value.y;
    GridNodeView gnv = gridNodeViews[x, y];
    gnv.SetInnerColor(COLOR_ADD_TO_CLOSED_LIST);
  }
}
