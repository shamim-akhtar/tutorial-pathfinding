using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class RectGrid_Viz : MonoBehaviour
{
  // the max number of columns in the grid.
  public int mX;
  // the max number of rows in the grid
  public int mY;

  // The prefab for representing a grid cell. We will 
  // use the prefab to show/visualize the status of the cell
  // as we proceed with our pathfinding.
  [SerializeField]
  GameObject RectGridCell_Prefab;

  GameObject[,] mRectGridCellGameObjects;

  // the 2d array of Vecto2Int.
  // This stucture stores the 2d indices of the grid cells.
  protected Vector2Int[,] mIndices;

  // the 2d array of the RectGridCell.
  protected RectGridCell[,] mRectGridCells;

  public Color COLOR_WALKABLE = new Color(42 / 255.0f, 99 / 255.0f, 164 / 255.0f, 1.0f);
  public Color COLOR_NON_WALKABLE = new Color(0.0f, 0.0f, 0.0f, 1.0f);
  public Color COLOR_CURRENT_NODE = new Color(0.5f, 0.4f, 0.1f, 1.0f);
  public Color COLOR_ADD_TO_OPEN_LIST = new Color(0.2f, 0.7f, 0.5f, 1.0f);
  public Color COLOR_ADD_TO_CLOSED_LIST = new Color(0.5f, 0.5f, 0.5f, 1.0f);

  public Transform mDestination;
  public NPCMovement mNPCMovement;

  // Construct a grid with the max cols and rows.
  protected void Construct(int numX, int numY)
  {
    mX = numX;
    mY = numY;

    mIndices = new Vector2Int[mX, mY];
    mRectGridCellGameObjects = new GameObject[mX, mY];
    mRectGridCells = new RectGridCell[mX, mY];

    // create all the grid cells (Index data) with default values.
    // also create the grid cell game ibjects from the prefab.
    for (int i = 0; i < mX; ++i)
    {
      for (int j = 0; j < mY; ++j)
      {
        mIndices[i, j] = new Vector2Int(i, j);
        mRectGridCellGameObjects[i, j] = Instantiate(
          RectGridCell_Prefab,
          new Vector3(i, j, 0.0f),
          Quaternion.identity);

        // Set the parent for the grid cell to this transform.
        mRectGridCellGameObjects[i, j].transform.SetParent(transform);

        // set a name to the instantiated cell.
        mRectGridCellGameObjects[i, j].name = "cell_" + i + "_" + j;

        // create the RectGridCells
        mRectGridCells[i, j] = new RectGridCell(this, mIndices[i, j]);

        // set a reference to the RectGridCell_Viz
        RectGridCell_Viz rectGridCell_Viz =
          mRectGridCellGameObjects[i, j].GetComponent<RectGridCell_Viz>();
        if (rectGridCell_Viz != null)
        {
          rectGridCell_Viz.RectGridCell = mRectGridCells[i, j];
        }
      }
    }
  }

  void ResetCamera()
  {
    Camera.main.orthographicSize = mY / 2.0f + 1.0f;
    Camera.main.transform.position = new Vector3(mX / 2.0f - 0.5f, mY / 2.0f - 0.5f, -100.0f);
  }

  private void Start()
  {
    // Constryct the grid and the cell game objects.
    Construct(mX, mY);

    // Reset the camera to a proper size and position.
    ResetCamera();
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

      if (mRectGridCells[i, j].IsWalkable)
      {
        neighbours.Add(mRectGridCells[i, j]);
      }
    }
    // Check top-right
    if (y < mY - 1 && x < mX - 1)
    {
      int i = x + 1;
      int j = y + 1;

      if (mRectGridCells[i, j].IsWalkable)
      {
        neighbours.Add(mRectGridCells[i, j]);
      }
    }
    // Check right
    if (x < mX - 1)
    {
      int i = x + 1;
      int j = y;

      if (mRectGridCells[i, j].IsWalkable)
      {
        neighbours.Add(mRectGridCells[i, j]);
      }
    }
    // Check right-down
    if (x < mX - 1 && y > 0)
    {
      int i = x + 1;
      int j = y - 1;

      if (mRectGridCells[i, j].IsWalkable)
      {
        neighbours.Add(mRectGridCells[i, j]);
      }
    }
    // Check down
    if (y > 0)
    {
      int i = x;
      int j = y - 1;

      if (mRectGridCells[i, j].IsWalkable)
      {
        neighbours.Add(mRectGridCells[i, j]);
      }
    }
    // Check down-left
    if (y > 0 && x > 0)
    {
      int i = x - 1;
      int j = y - 1;

      if (mRectGridCells[i, j].IsWalkable)
      {
        neighbours.Add(mRectGridCells[i, j]);
      }
    }
    // Check left
    if (x > 0)
    {
      int i = x - 1;
      int j = y;

      Vector2Int v = mIndices[i, j];

      if (mRectGridCells[i, j].IsWalkable)
      {
        neighbours.Add(mRectGridCells[i, j]);
      }
    }
    // Check left-top
    if (x > 0 && y < mY - 1)
    {
      int i = x - 1;
      int j = y + 1;

      if (mRectGridCells[i, j].IsWalkable)
      {
        neighbours.Add(mRectGridCells[i, j]);
      }
    }

    return neighbours;
  }

  private void Update()
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
      RectGridCell_Viz sc = obj.GetComponent<RectGridCell_Viz>();
      ToggleWalkable(sc);
    }
  }
  public void ToggleWalkable(RectGridCell_Viz sc)
  {
    if (sc == null)
      return;

    int x = sc.RectGridCell.Value.x;
    int y = sc.RectGridCell.Value.y;

    sc.RectGridCell.IsWalkable = !sc.RectGridCell.IsWalkable;

    if (sc.RectGridCell.IsWalkable)
    {
      sc.SetInnerColor(COLOR_WALKABLE);
    }
    else
    {
      sc.SetInnerColor(COLOR_NON_WALKABLE);
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
      RectGridCell_Viz sc = obj.GetComponent<RectGridCell_Viz>();
      if (sc == null) return;

      Vector3 pos = mDestination.position;
      pos.x = sc.RectGridCell.Value.x;
      pos.y = sc.RectGridCell.Value.y;
      mDestination.position = pos;

      // Set the destination to the NPC.
      mNPCMovement.SetDestination(this, sc.RectGridCell);
    }
  }

  public RectGridCell GetRectGridCell(int x, int y)
  {
    if(x >= 0 && x < mX && y >=0 && y < mY)
    {
      return mRectGridCells[x, y];
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
    for(int i = 0; i < mX; i++)
    {
      for (int j = 0; j < mY; j++)
      {
        GameObject obj = mRectGridCellGameObjects[i, j];
        RectGridCell_Viz sc = obj.GetComponent<RectGridCell_Viz>();
        if(sc.RectGridCell.IsWalkable)
        {
          sc.SetInnerColor(COLOR_WALKABLE);
        }
        else
        {
          sc.SetInnerColor(COLOR_NON_WALKABLE);
        }
      }
    }
  }

  public void OnChangeCurrentNode(PathFinder<Vector2Int>.PathFinderNode node)
  {
    int x = node.Location.Value.x;
    int y = node.Location.Value.y;
    GameObject obj = mRectGridCellGameObjects[x, y];
    RectGridCell_Viz sc = obj.GetComponent<RectGridCell_Viz>();
    sc.SetInnerColor(COLOR_CURRENT_NODE);
  }
  public void OnAddToOpenList(PathFinder<Vector2Int>.PathFinderNode node)
  {
    int x = node.Location.Value.x;
    int y = node.Location.Value.y;
    GameObject obj = mRectGridCellGameObjects[x, y];
    RectGridCell_Viz sc = obj.GetComponent<RectGridCell_Viz>();
    sc.SetInnerColor(COLOR_ADD_TO_OPEN_LIST);
  }
  public void OnAddToClosedList(PathFinder<Vector2Int>.PathFinderNode node)
  {
    int x = node.Location.Value.x;
    int y = node.Location.Value.y;
    GameObject obj = mRectGridCellGameObjects[x, y];
    RectGridCell_Viz sc = obj.GetComponent<RectGridCell_Viz>();
    sc.SetInnerColor(COLOR_ADD_TO_CLOSED_LIST);
  }
}
