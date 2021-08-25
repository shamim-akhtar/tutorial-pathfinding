using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class RectGrid_Viz : MonoBehaviour, IPathfindingUI
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
  RectGrid mGrid;

  public Color COLOR_WALKABLE = new Color(42 / 255.0f, 99 / 255.0f, 164 / 255.0f, 1.0f);
  public Color COLOR_NON_WALKABLE = new Color(0.0f, 0.0f, 0.0f, 1.0f);
  public Color COLOR_CURRENT_NODE = new Color(0.5f, 0.4f, 0.1f, 1.0f);
  public Color COLOR_ADD_TO_OPEN_LIST = new Color(0.2f, 0.7f, 0.5f, 1.0f);
  public Color COLOR_ADD_TO_CLOSED_LIST = new Color(0.5f, 0.5f, 0.5f, 1.0f);

  public Transform mDestination;
  public GameObject NpcPrefab;

  Text mTextFCost;
  Text mTextGCost;
  Text mTextHCost;
  Text mTextNotification;

  #region Pathfinding related variables
  public int NumNPC = 1;
  List<GameObject> mNPCs = new List<GameObject>();
  List<RectGrid.Cell> mNPCStartPositions =
    new List<RectGrid.Cell>();
  List<RectGrid.Cell> mNPCStartPositionsPrev =
    new List<RectGrid.Cell>();

  // The start vertex.
  RectGrid.Cell mGoal;

  //ThreadedPathFinderPool<Vector2Int> mThreadedPool = 
  //  new ThreadedPathFinderPool<Vector2Int>();

  Dictionary<PathFinderTypes, List<PathFinder<Vector2Int>>> mPathFinders =
    new Dictionary<PathFinderTypes, List<PathFinder<Vector2Int>>>();
  List<bool> mPathCalculated = new List<bool>();
  public PathFinderTypes mPathFinderType = PathFinderTypes.ASTAR;
  public bool mInteractive = false;

  List<LineRenderer> mPathViz = new List<LineRenderer>();

  private Dictionary<Vector2Int, RectGridCell_Viz> mNodeVertex_VizDic =
    new Dictionary<Vector2Int, RectGridCell_Viz>();
  #endregion

  // Construct a grid with the max cols and rows.
  protected void Construct(int numX, int numY)
  {
    mGrid = new RectGrid(mX, mY);

    mRectGridCellGameObjects = new GameObject[mX, mY];

    // create all the grid cells (Index data) with default values.
    // also create the grid cell game ibjects from the prefab.
    for (int i = 0; i < mX; ++i)
    {
      for (int j = 0; j < mY; ++j)
      {
        mRectGridCellGameObjects[i, j] = Instantiate(
          RectGridCell_Prefab,
          new Vector3(i, j, 0.0f),
          Quaternion.identity);

        // Set the parent for the grid cell to this transform.
        mRectGridCellGameObjects[i, j].transform.SetParent(transform);

        // set a name to the instantiated cell.
        mRectGridCellGameObjects[i, j].name = "cell_" + i + "_" + j;

        // set a reference to the RectGridCell_Viz
        RectGridCell_Viz rectGridCell_Viz =
          mRectGridCellGameObjects[i, j].GetComponent<RectGridCell_Viz>();
        if (rectGridCell_Viz != null)
        {
          rectGridCell_Viz.RectGridCell = mGrid.mCells[i, j];
        }
      }
    }
  }

  void ResetCamera()
  {
    Rect extent = new Rect(0, 0, mX, mY);

    // by default enable camera panning.
    CameraMovement2D camMovement = Camera.main.gameObject.GetComponent<CameraMovement2D>();
    if (camMovement)
    {
      camMovement.SetCamera(Camera.main);
      camMovement.RePositionCamera(extent);
    }
    else
    {
      Camera.main.orthographicSize = extent.height / 1.5f;
      Vector3 center = extent.center;
      center.z = -100.0f;
      Camera.main.transform.position = center;
    }
  }

  private void Start()
  {
    // Constryct the grid and the cell game objects.
    Construct(mX, mY);

    // Reset the camera to a proper size and position.
    ResetCamera();

    CreatePathFinders();

    for (int i = 0; i < NumNPC; ++i)
    {
      GameObject Npc = Instantiate(NpcPrefab);
      mNPCs.Add(Npc);

      // We create a line renderer to show the path.
      LineRenderer lr = Npc.AddComponent<LineRenderer>();
      mPathViz.Add(lr);
      lr.startWidth = 0.1f;
      lr.endWidth = 0.1f;
      lr.startColor = Color.magenta;
      lr.endColor = Color.magenta;
    }
    RandomizeNPCs();
    SetInteractive(mInteractive);
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

    if (Input.GetKeyDown(KeyCode.RightArrow))
    {
      if (mInteractive)
      {
        PathFindingStep();
      }
    }
    SyncThreads();
  }


  void SyncThreads()
  {
    //if (!mInteractive)
    //{
    //  for (int i = 0; i < NumNPC; ++i)
    //  {
    //    if (mThreadedPool.GetThreadedPathFinder(i).Done)
    //    {
    //      PathFinder<Vector2Int> pf = mThreadedPool.GetThreadedPathFinder(i).PathFinder;
    //      mThreadedPool.GetThreadedPathFinder(i).Done = false;

    //      if (pf.Status == PathFinderStatus.SUCCESS)
    //      {
    //        OnPathFound(i);
    //      }
    //      else if (pf.Status == PathFinderStatus.FAILURE)
    //      {
    //        OnPathNotFound(i);
    //      }
    //    }
    //  }
    //}
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

    sc.RectGridCell.walkable = !sc.RectGridCell.walkable;

    if (sc.RectGridCell.walkable)
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
    // disable picking if we hit the UI.
    if (EventSystem.current.IsPointerOverGameObject() || enabled == false)
    {
      return;
    }

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

      mGoal = sc.RectGridCell;

      FindPath();
    }
  }

  #region Pathfinding related functions


  void CreatePathFinders()
  {
    mPathFinders.Add(PathFinderTypes.ASTAR, new List<PathFinder<Vector2Int>>());
    mPathFinders.Add(PathFinderTypes.DJIKSTRA, new List<PathFinder<Vector2Int>>());
    mPathFinders.Add(PathFinderTypes.GREEDY_BEST_FIRST, new List<PathFinder<Vector2Int>>());
    for (int i = 0; i < NumNPC; ++i)
    {
      //// We create the different path finders
      //ThreadedPathFinder<Vector2Int> tpf = mThreadedPool.CreateThreadedAStarPathFinder();
      //tpf.PathFinder.HeuristicCost = RectGrid.GetManhattanCost;
      //tpf.PathFinder.NodeTraversalCost = RectGrid.GetEuclideanCost;

      AStarPathFinder<Vector2Int> pf1 = new AStarPathFinder<Vector2Int>();
      DijkstraPathFinder<Vector2Int> pf2 = new DijkstraPathFinder<Vector2Int>();
      GreedyPathFinder<Vector2Int> pf3 = new GreedyPathFinder<Vector2Int>();

      mPathFinders[PathFinderTypes.ASTAR].Add(pf1);
      mPathFinders[PathFinderTypes.DJIKSTRA].Add(pf2);
      mPathFinders[PathFinderTypes.GREEDY_BEST_FIRST].Add(pf3);

      pf1.HeuristicCost = RectGrid.GetManhattanCost;
      pf1.NodeTraversalCost = RectGrid.GetEuclideanCost;
      pf2.HeuristicCost = RectGrid.GetManhattanCost;
      pf2.NodeTraversalCost = RectGrid.GetEuclideanCost;
      pf3.HeuristicCost = RectGrid.GetManhattanCost;
      pf3.NodeTraversalCost = RectGrid.GetEuclideanCost;

      mPathCalculated.Add(false);
    }
  }

  public string GetTitle()
  {
    return "2D Grid";
  }

  public bool IsAnyPathFinderRunning()
  {
    for (int i = 0; i < mPathFinders[mPathFinderType].Count; ++i)
    {
      if (mPathFinders[mPathFinderType][i].Status == PathFinderStatus.RUNNING)
        return true;
    }
    return false;
  }

  public void SetInteractive(bool flag)
  {
    mInteractive = flag;
    if (mInteractive)
    {
      for (int i = 0; i < NumNPC; ++i)
      {
        mPathFinders[PathFinderTypes.ASTAR][i].onChangeCurrentNode = OnChangeCurrentNode;
        mPathFinders[PathFinderTypes.ASTAR][i].onAddToClosedList = OnAddToClosedList;
        mPathFinders[PathFinderTypes.ASTAR][i].onAddToOpenList = OnAddToOpenList;
        mPathFinders[PathFinderTypes.DJIKSTRA][i].onChangeCurrentNode = OnChangeCurrentNode;
        mPathFinders[PathFinderTypes.DJIKSTRA][i].onAddToClosedList = OnAddToClosedList;
        mPathFinders[PathFinderTypes.DJIKSTRA][i].onAddToOpenList = OnAddToOpenList;
        mPathFinders[PathFinderTypes.GREEDY_BEST_FIRST][i].onChangeCurrentNode = OnChangeCurrentNode;
        mPathFinders[PathFinderTypes.GREEDY_BEST_FIRST][i].onAddToClosedList = OnAddToClosedList;
        mPathFinders[PathFinderTypes.GREEDY_BEST_FIRST][i].onAddToOpenList = OnAddToOpenList;
      }
    }
    else
    {
      for (int i = 0; i < NumNPC; ++i)
      {
        mPathFinders[PathFinderTypes.ASTAR][i].onChangeCurrentNode = null;// OnChangeCurrentNode;
        mPathFinders[PathFinderTypes.ASTAR][i].onAddToClosedList = null;// OnAddToClosedList;
        mPathFinders[PathFinderTypes.ASTAR][i].onAddToOpenList = null;// OnAddToOpenList;
        mPathFinders[PathFinderTypes.DJIKSTRA][i].onChangeCurrentNode = null;// OnChangeCurrentNode;
        mPathFinders[PathFinderTypes.DJIKSTRA][i].onAddToClosedList = null;// OnAddToClosedList;
        mPathFinders[PathFinderTypes.DJIKSTRA][i].onAddToOpenList = null;// OnAddToOpenList;
        mPathFinders[PathFinderTypes.GREEDY_BEST_FIRST][i].onChangeCurrentNode = null;// OnChangeCurrentNode;
        mPathFinders[PathFinderTypes.GREEDY_BEST_FIRST][i].onAddToClosedList = null;// OnAddToClosedList;
        mPathFinders[PathFinderTypes.GREEDY_BEST_FIRST][i].onAddToOpenList = null;// OnAddToOpenList;
      }
    }
  }

  public void ResetLastDestination()
  {
    SetHCost(0.0f);
    SetGCost(0.0f);
    SetHCost(0.0f);
    for (int i = 0; i < mNPCStartPositionsPrev.Count; ++i)
    {
      mNPCStartPositions[i] = mNPCStartPositionsPrev[i];
      NPC npc = mNPCs[i].GetComponent<NPC>();
      if (npc)
      {
        npc.SetPosition(mNPCStartPositions[i].Value.x, mNPCStartPositions[i].Value.y);
      }
    }
    FindPath();
  }

  public void FindPath()
  {
    // clear old lines.
    for (int i = 0; i < NumNPC; ++i)
    {
      mPathViz[i].positionCount = 0;
      mPathCalculated[i] = false;
    }

    for(int i = 0; i < mX; ++i)
    {
      for(int j = 0; j < mY; ++j)
      {
        if (mGrid.mCells[i, j].walkable)
        {
          mRectGridCellGameObjects[i, j].GetComponent<RectGridCell_Viz>().SetInnerColor(COLOR_WALKABLE);
        }
        else
        {
          mRectGridCellGameObjects[i, j].GetComponent<RectGridCell_Viz>().SetInnerColor(COLOR_NON_WALKABLE);
        }
      }
    }

    if (!mInteractive)
    {
      for (int i = 0; i < mNPCs.Count; ++i)
      {
        //mThreadedPool.FindPath(i, mNPCStartPositions[i], mGoal);
        mPathFinders[mPathFinderType][i].Initialize(mNPCStartPositions[i], mGoal);
        StartCoroutine(Coroutine_FindPathSteps(i));
      }
    }
    else
    {
      for (int i = 0; i < mNPCs.Count; ++i)
      {
        mPathFinders[mPathFinderType][i].Initialize(mNPCStartPositions[i], mGoal);
      }
    }
  }
  IEnumerator Coroutine_FindPathSteps(int index)
  {
    PathFinder<Vector2Int> pathFinder = mPathFinders[mPathFinderType][index];
    while (pathFinder.Status == PathFinderStatus.RUNNING)
    {
      pathFinder.Step();
      yield return null;
    }

    if (pathFinder.Status == PathFinderStatus.SUCCESS)
    {
      OnPathFound(index);
    }
    else if (pathFinder.Status == PathFinderStatus.FAILURE)
    {
      OnPathNotFound(index);
    }
  }
  public void OnPathFound(int index)
  {
    PathFinder<Vector2Int>.PathFinderNode node = null;

    if (!mInteractive)
    {
      //ThreadedPathFinder<Vector2Int> tpf = mThreadedPool.GetThreadedPathFinder(index);
      //node = tpf.PathFinder.CurrentNode;
      node = mPathFinders[mPathFinderType][index].CurrentNode;
    }
    else
    {
      node = mPathFinders[mPathFinderType][index].CurrentNode;
    }

    SetFCost(node.Fcost);
    SetGCost(node.GCost);
    SetHCost(node.Hcost);

    List<Vector2Int> reverse_indices = new List<Vector2Int>();

    while (node != null)
    {
      reverse_indices.Add(node.Location.Value);
      node = node.Parent;
    }
    NPC Npc = mNPCs[index].GetComponent<NPC>();

    LineRenderer lr = mPathViz[index];
    lr.positionCount = reverse_indices.Count;
    for (int i = reverse_indices.Count - 1; i >= 0; i--)
    {
      Npc.AddWayPoint(new Vector2(
        reverse_indices[i].x,
        reverse_indices[i].y));

      lr.SetPosition(i, new Vector3(
        reverse_indices[i].x,
        reverse_indices[i].y,
        -2.0f));
    }
    // save these as the previous start positions.
    mNPCStartPositionsPrev[index] = mNPCStartPositions[index];
    mNPCStartPositions[index] = mGoal;
  }

  void OnPathNotFound(int i)
  {
    Debug.Log(i + " - Cannot find path to destination");
    if (mTextNotification)
    {
      mTextNotification.text = "Cannot find path to destination";
    }
  }

  public void PathFindingStepForceComplete()
  {
    if (mInteractive)
    {
      for (int i = 0; i < mPathFinders[mPathFinderType].Count; ++i)
      {
        if (mPathCalculated[i]) continue;
        StartCoroutine(Coroutine_FindPathSteps(i));
      }
    }
  }

  public void PathFindingStep()
  {
    if (mInteractive)
    {
      for (int i = 0; i < mPathFinders[mPathFinderType].Count; ++i)
      {
        if (mPathCalculated[i]) continue;

        int index = i;
        PathFinder<Vector2Int> pathFinder = mPathFinders[mPathFinderType][index];
        if (pathFinder.Status == PathFinderStatus.RUNNING)
        {
          pathFinder.Step();
        }

        if (pathFinder.Status == PathFinderStatus.SUCCESS)
        {
          OnPathFound(index);
          mPathCalculated[index] = true;
        }
        else if (pathFinder.Status == PathFinderStatus.FAILURE)
        {
          OnPathNotFound(index);
          mPathCalculated[index] = true;
        }
      }
    }
  }

  public void RandomizeNPCs()
  {
    mNPCStartPositions.Clear();
    mNPCStartPositionsPrev.Clear();
    for (int i = 0; i < NumNPC; ++i)
    {
      // randomly place our NPCs
      int x = Random.Range(0, mX);
      int y = Random.Range(0, mY);

      GameObject Npc = mNPCs[i];
      Npc.transform.position = new Vector3(x,y,-2.0f);

      mNPCStartPositions.Add(mGrid.mCells[x,y]);
      mNPCStartPositionsPrev.Add(mGrid.mCells[x, y]);
    }
  }

  public void RegenerateMap()
  {
    SceneManager.LoadScene("Combined_Demo_Grid");
  }

  public void SetPathFinderType(PathFinderTypes type)
  {
    mPathFinderType = type;
  }

  public void SetFCostText(Text textField)
  {
    mTextFCost = textField;
  }

  public void SetHCostText(Text textField)
  {
    mTextHCost = textField;
  }

  public void SetGCostText(Text textField)
  {
    mTextGCost = textField;
  }

  public void SetNotificationText(Text textField)
  {
    mTextNotification = textField;
  }
  public void SetFCost(float cost)
  {
    if(mTextFCost)
      mTextFCost.text = cost.ToString("F2");
  }

  public void SetHCost(float cost)
  {
    if (mTextHCost)
      mTextHCost.text = cost.ToString("F2");
  }

  public void SetGCost(float cost)
  {
    if (mTextGCost)
      mTextGCost.text = cost.ToString("F2");
  }

  #endregion

  public void ResetCellColours()
  {
    for (int i = 0; i < mX; i++)
    {
      for (int j = 0; j < mY; j++)
      {
        GameObject obj = mRectGridCellGameObjects[i, j];
        RectGridCell_Viz sc = obj.GetComponent<RectGridCell_Viz>();
        if (sc.RectGridCell.walkable)
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

    SetFCost(node.Fcost);
    SetHCost(node.Hcost);
    SetGCost(node.GCost);
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


  public bool IsInteractive()
  {
    return mInteractive;
  }

  public void SetCostFunction(CostFunctionType cf)
  {
    switch(cf)
    {
      case (CostFunctionType.MANHATTAN):
        {
          for (int i = 0; i < NumNPC; ++i)
          {
            //// We create the different path finders
            //ThreadedPathFinder<Vector2Int> tpf = mThreadedPool.GetThreadedPathFinder(i);
            //tpf.PathFinder.HeuristicCost = RectGrid.GetManhattanCost;
            //tpf.PathFinder.NodeTraversalCost = RectGrid.GetEuclideanCost;

            PathFinder<Vector2Int> pf1 = mPathFinders[PathFinderTypes.ASTAR][i];
            PathFinder<Vector2Int> pf2 = mPathFinders[PathFinderTypes.DJIKSTRA][i];
            PathFinder<Vector2Int> pf3 = mPathFinders[PathFinderTypes.GREEDY_BEST_FIRST][i];

            pf1.HeuristicCost = RectGrid.GetManhattanCost;
            pf1.NodeTraversalCost = RectGrid.GetEuclideanCost;
            pf2.HeuristicCost = RectGrid.GetManhattanCost;
            pf2.NodeTraversalCost = RectGrid.GetEuclideanCost;
            pf3.HeuristicCost = RectGrid.GetManhattanCost;
            pf3.NodeTraversalCost = RectGrid.GetEuclideanCost;

            mPathCalculated.Add(false);
          }
          break;
        }
      case (CostFunctionType.EUCLIDEN):
        {
          for (int i = 0; i < NumNPC; ++i)
          {
            //// We create the different path finders
            //ThreadedPathFinder<Vector2Int> tpf = mThreadedPool.GetThreadedPathFinder(i);
            //tpf.PathFinder.HeuristicCost = RectGrid.GetEuclideanCost;
            //tpf.PathFinder.NodeTraversalCost = RectGrid.GetEuclideanCost;

            PathFinder<Vector2Int> pf1 = mPathFinders[PathFinderTypes.ASTAR][i];
            PathFinder<Vector2Int> pf2 = mPathFinders[PathFinderTypes.DJIKSTRA][i];
            PathFinder<Vector2Int> pf3 = mPathFinders[PathFinderTypes.GREEDY_BEST_FIRST][i];

            pf1.HeuristicCost = RectGrid.GetEuclideanCost;
            pf1.NodeTraversalCost = RectGrid.GetEuclideanCost;
            pf2.HeuristicCost = RectGrid.GetEuclideanCost;
            pf2.NodeTraversalCost = RectGrid.GetEuclideanCost;
            pf3.HeuristicCost = RectGrid.GetEuclideanCost;
            pf3.NodeTraversalCost = RectGrid.GetEuclideanCost;

            mPathCalculated.Add(false);
          }
          break;
        }
    }
  }
}
