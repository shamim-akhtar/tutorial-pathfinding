using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameAI.PathFinding;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class RandomGraph : MonoBehaviour
{
  Graph<RandomGraphNode> mRandomGraphNodes = new Graph<RandomGraphNode>();
  private Rect mExtent = new Rect();

  [SerializeField]
  GameObject VertexPrefab;

  [SerializeField]
  GameObject NpcPrefab;

  public int NumNPC = 50;

  [SerializeField]
  Transform Destination;

  Text StatusText;

  public int rows_cols = 20;
  public int rows_rows = 10;
  public float NodeSelectionProb = 0.6f;

  public bool UseThreads = false;

  private Text mTextFCost;
  private Text mTextGCost;
  private Text mTextHCost;

  public Color COLOR_DEFAULT = new Color(1.0f, 1.0f, 0.0f, 1.0f);
  public Color COLOR_OPEN_LIST = new Color(0.0f, 0.0f, 1.0f, 0.3f);
  public Color COLOR_CLOSED_LIST = new Color(0.0f, 0.0f, 0.0f, 0.3f);
  public Color COLOR_CURRENT_NODE = new Color(1.0f, 0.0f, 0.0f, 0.3f);

  List<GameObject> mNPCs = new List<GameObject>();
  List<Graph<RandomGraphNode>.Vertex> mNPCStartPositions = 
    new List<Graph<RandomGraphNode>.Vertex>();
  List<Graph<RandomGraphNode>.Vertex> mNPCStartPositionsPrev =
    new List<Graph<RandomGraphNode>.Vertex>();

  // The start vertex.
  Graph<RandomGraphNode>.Vertex mGoal;

  ThreadedPathFinderPool<RandomGraphNode> mThreadedPool = new ThreadedPathFinderPool<RandomGraphNode>();

  Dictionary<PathFinderTypes, List<PathFinder<RandomGraphNode>>> mPathFinders = 
    new Dictionary<PathFinderTypes,List<PathFinder<RandomGraphNode>>>();
  List<bool> mPathCalculated = new List<bool>();
  public PathFinderTypes mPathFinderType = PathFinderTypes.ASTAR;
  public bool mInteractive = false;

  List<LineRenderer> mPathViz = new List<LineRenderer>();

  private Dictionary<RandomGraphNode, RandomGraphNode_Viz> mNodeVertex_VizDic = new Dictionary<RandomGraphNode, RandomGraphNode_Viz>();

  public void CalculateExtent()
  {
    float minX = Mathf.Infinity;
    float minY = Mathf.Infinity;
    float maxX = -Mathf.Infinity;
    float maxY = -Mathf.Infinity;
    for (int i = 0; i < mRandomGraphNodes.Vertices.Count; ++i)
    {
      RandomGraphNode d = mRandomGraphNodes.Vertices[i].Value;
      Vector2 p = d.Point;

      if (minX > p.x) minX = p.x;
      if (minY > p.y) minY = p.y;
      if (maxX <= p.x) maxX = p.x;
      if (maxY <= p.y) maxY = p.y;
    }

    mExtent.xMin = minX;
    mExtent.xMax = maxX;
    mExtent.yMin = minY;
    mExtent.yMax = maxY;
  }

  void CreateRandomGraph()
  {
    //Random.InitState(10);
    for (int i = 0; i < rows_cols; ++i)
    {
      for (int j = 0; j < rows_rows; ++j)
      {
        float toss = Random.Range(0.0f, 1.0f);
        if (toss >= NodeSelectionProb)
        {
          mRandomGraphNodes.AddVertex(
            new RandomGraphNode("stop_" + i + "_" + j, i, j));
        }
      }
    }

    // find the cost between all the vertices.
    List<List<float>> distances = 
      new List<List<float>>(mRandomGraphNodes.Count);

    List<List<float>> angles = 
      new List<List<float>>(mRandomGraphNodes.Count);

    for (int i = 0; i < mRandomGraphNodes.Count; ++i)
    {
      distances.Add(new List<float>());
      angles.Add(new List<float>());
      for (int j = 0; j < mRandomGraphNodes.Count; ++j)
      {
        distances[i].Add(RandomGraphNode.Distance(
          mRandomGraphNodes.Vertices[i].Value, 
          mRandomGraphNodes.Vertices[j].Value));

        angles[i].Add(RandomGraphNode.GetAngleBetweenTwoStops(
          mRandomGraphNodes.Vertices[i].Value, 
          mRandomGraphNodes.Vertices[j].Value));
      }

      var sorted = distances[i]
       .Select((x, k) => new KeyValuePair<float, int>(x, k))
       .OrderBy(x => x.Key)
       .ToList();

      List<float> B = sorted.Select(x => x.Key).ToList();
      List<int> idx = sorted.Select(x => x.Value).ToList();

      // connect the nearest 2 to 4 vertices.
      int index = Random.Range(2, 6);
      int id = 0;

      Dictionary<float, bool> angleFilled = 
        new Dictionary<float, bool>();

      for (int j = 1; j < B.Count-1; ++j)
      {
        // we do not want to add collinear vertices.
        if (!angleFilled.ContainsKey(angles[i][idx[j]]))
        {
          angleFilled[angles[i][idx[j]]] = true;

          mRandomGraphNodes.AddDirectedEdge(
            mRandomGraphNodes.Vertices[i],
            mRandomGraphNodes.Vertices[idx[j]],
            B[j]);
          id++;
        }
        if (id == index)
          break;
      }
    }
  }

  void Start()
  {
    mPathFinders.Add(PathFinderTypes.ASTAR, new List<PathFinder<RandomGraphNode>>());
    mPathFinders.Add(PathFinderTypes.DJIKSTRA, new List<PathFinder<RandomGraphNode>>());
    mPathFinders.Add(PathFinderTypes.GREEDY_BEST_FIRST, new List<PathFinder<RandomGraphNode>>());

    CreateRandomGraph();

    for (int i = 0; i < mRandomGraphNodes.Vertices.Count; ++i)
    {

      Vector3 pos = Vector3.zero;
      pos.x = mRandomGraphNodes.Vertices[i].Value.Point.x;
      pos.y = mRandomGraphNodes.Vertices[i].Value.Point.y;
      pos.z = 0.0f;

      GameObject obj = Instantiate(
        VertexPrefab, 
        pos, 
        Quaternion.identity);

      obj.name = mRandomGraphNodes.Vertices[i].Value.Name;

      RandomGraphNode_Viz vertexViz = obj.GetComponent<RandomGraphNode_Viz>();
      vertexViz.SetVertex(mRandomGraphNodes.Vertices[i]);

      mNodeVertex_VizDic.Add(mRandomGraphNodes.Vertices[i].Value, vertexViz);
    }

    CalculateExtent();
    // by default enable camera panning.
    CameraMovement2D camMovement = Camera.main.gameObject.GetComponent<CameraMovement2D>();
    if (camMovement)
    {
      camMovement.SetCamera(Camera.main);
      camMovement.RePositionCamera(mExtent);
    }
    else
    {
      Camera.main.orthographicSize = mExtent.height / 1.5f;
      Vector3 center = mExtent.center;
      center.z = -100.0f;
      Camera.main.transform.position = center;
    }

    CreatePathFinders();

    for (int i = 0; i < NumNPC; ++i)
    {
      GameObject Npc = Instantiate(NpcPrefab);
      mNPCs.Add(Npc);

      // We create a line renderer to show the path.
      LineRenderer lr = Npc.AddComponent<LineRenderer>();
      mPathViz.Add(lr);
      lr.startWidth = 0.2f;
      lr.endWidth = 0.2f;
      lr.startColor = Color.magenta;
      lr.endColor = Color.magenta;
    }
    RandomizeNPCs();
    SetInteractive(mInteractive);
  }

  void CreatePathFinders()
  {
    for (int i = 0; i < NumNPC; ++i)
    {
      // We create the different path finders
      ThreadedPathFinder<RandomGraphNode> tpf = mThreadedPool.CreateThreadedAStarPathFinder();
      tpf.PathFinder.HeuristicCost = RandomGraphNode.GetManhattanCost;
      tpf.PathFinder.NodeTraversalCost = RandomGraphNode.GetEuclideanCost;

      AStarPathFinder<RandomGraphNode> pf1 = new AStarPathFinder<RandomGraphNode>();
      DijkstraPathFinder<RandomGraphNode> pf2 = new DijkstraPathFinder<RandomGraphNode>();
      GreedyPathFinder<RandomGraphNode> pf3 = new GreedyPathFinder<RandomGraphNode>();

      mPathFinders[PathFinderTypes.ASTAR].Add(pf1);
      mPathFinders[PathFinderTypes.DJIKSTRA].Add(pf2);
      mPathFinders[PathFinderTypes.GREEDY_BEST_FIRST].Add(pf3);

      pf1.HeuristicCost = RandomGraphNode.GetManhattanCost;
      pf1.NodeTraversalCost = RandomGraphNode.GetEuclideanCost;
      pf2.HeuristicCost = RandomGraphNode.GetManhattanCost;
      pf2.NodeTraversalCost = RandomGraphNode.GetEuclideanCost;
      pf3.HeuristicCost = RandomGraphNode.GetManhattanCost;
      pf3.NodeTraversalCost = RandomGraphNode.GetEuclideanCost;

      mPathCalculated.Add(false);
    }
  }

  public void RandomizeNPCs()
  {
    mNPCStartPositions.Clear();
    mNPCStartPositionsPrev.Clear();
    for (int i = 0; i < NumNPC; ++i)
    {
      // randomly place our NPCs
      int randIndex = Random.Range(0, mRandomGraphNodes.Count);

      GameObject Npc = mNPCs[i];
      Npc.transform.position = new Vector3(
        mRandomGraphNodes.Vertices[randIndex].Value.Point.x,
        mRandomGraphNodes.Vertices[randIndex].Value.Point.y,
        -2.0f);

      mNPCStartPositions.Add(mRandomGraphNodes.Vertices[randIndex]);
      mNPCStartPositionsPrev.Add(mRandomGraphNodes.Vertices[randIndex]);
    }
  }

  void Update()
  {
    if (Input.GetMouseButtonDown(1) ||
      Input.GetMouseButtonDown(0))
    {
      RayCastAndSetDestination();
    }

    if(Input.GetKeyDown(KeyCode.RightArrow))
    {
      if(mInteractive)
      {
        PathFindingStep();
      }
    }
    SyncThreads();
  }

  void SyncThreads()
  {
    if (UseThreads)
    {
      for (int i = 0; i < NumNPC; ++i)
      {
        if (mThreadedPool.GetThreadedPathFinder(i).Done)
        {
          PathFinder<RandomGraphNode> pf = mThreadedPool.GetThreadedPathFinder(i).PathFinder;
          mThreadedPool.GetThreadedPathFinder(i).Done = false;

          if (pf.Status == PathFinderStatus.SUCCESS)
          {
            OnPathFound(i);
          }
          else if (pf.Status == PathFinderStatus.FAILURE)
          {
            OnPathNotFound(i);
          }
        }
      }
    }
  }

  void RayCastAndSetDestination()
  {
    // disable picking if we hit the UI.
    if (EventSystem.current.IsPointerOverGameObject())
    {
      return;
    }

    Vector2 rayPos = new Vector2(
        Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
        Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

    RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

    //// by default enable camera panning.
    //CameraMovement2D camMovement = Camera.main.gameObject.GetComponent<CameraMovement2D>();
    //if(camMovement)
    //{
    //  CameraMovement2D.CameraPanning = true;
    //}

    if (hit)
    {

      //if (camMovement)
      //{
      //  CameraMovement2D.CameraPanning = false;
      //}
      // disable camera panning if
      GameObject obj = hit.transform.gameObject;
      RandomGraphNode_Viz sc = obj.GetComponent<RandomGraphNode_Viz>();
      if (sc == null) return;

      Vector3 pos = Destination.position;
      pos.x = sc.Vertex.Value.Point.x;
      pos.y = sc.Vertex.Value.Point.y;
      Destination.position = pos;
      Destination.gameObject.SetActive(true);

      mGoal = sc.Vertex;

      FindPath();
    }
  }

  public void FindPath()
  {
    // clear old lines.
    for (int i = 0; i < NumNPC; ++i)
    {
      mPathViz[i].positionCount = 0;
      mPathCalculated[i] = false;
    }
    foreach (KeyValuePair<RandomGraphNode, RandomGraphNode_Viz> entry in mNodeVertex_VizDic)
    {
      entry.Value.SetInnerColor(COLOR_DEFAULT);
    }

    if (UseThreads)
    {
      for (int i = 0; i < mNPCs.Count; ++i)
      {
        mThreadedPool.FindPath(i, mNPCStartPositions[i], mGoal);
      }
    }
    else
    {
      for (int i = 0; i < mNPCs.Count; ++i)
      {
        mPathFinders[mPathFinderType][i].Initialize(mNPCStartPositions[i], mGoal);
        if (!mInteractive)
        {
          // its not interactive so we start the coroutine.
          StartCoroutine(Coroutine_FindPathSteps(i));
        }
      }
    }
  }

  public void PathFindingStep()
  {
    if(mInteractive)
    {
      for(int i = 0; i < mPathFinders[mPathFinderType].Count; ++i)
      {
        if (mPathCalculated[i]) continue;

        int index = i;
        PathFinder<RandomGraphNode> pathFinder = mPathFinders[mPathFinderType][index];
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

  public void PathFindingStepForcComplete()
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

  IEnumerator Coroutine_FindPathSteps(int index)
  {
    PathFinder<RandomGraphNode> pathFinder = mPathFinders[mPathFinderType][index];
    while (pathFinder.Status == PathFinderStatus.RUNNING)
    {
      pathFinder.Step();
      yield return null;
    }

    if(pathFinder.Status == PathFinderStatus.SUCCESS)
    {
      OnPathFound(index);
    }
    else if(pathFinder.Status == PathFinderStatus.FAILURE)
    {
      OnPathNotFound(index);
    }
  }

  public void OnPathFound(int index)
  {
    if (StatusText)
    {
      StatusText.text = "Path found to destination";
    }
    PathFinder<RandomGraphNode>.PathFinderNode node = null;

    if (UseThreads)
    {
      ThreadedPathFinder<RandomGraphNode> tpf = mThreadedPool.GetThreadedPathFinder(index);
      node = tpf.PathFinder.CurrentNode;
    }
    else
    {
      node = mPathFinders[mPathFinderType][index].CurrentNode;
    }

    SetFCost(node.Fcost);
    SetGCost(node.GCost);
    SetHCost(node.Hcost);

    List<RandomGraphNode> reverse_indices = new List<RandomGraphNode>();

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
        reverse_indices[i].Point.x,
        reverse_indices[i].Point.y));

      lr.SetPosition(i, new Vector3(
        reverse_indices[i].Point.x,
        reverse_indices[i].Point.y,
        0.0f));
    }
    // save these as the previous start positions.
    mNPCStartPositionsPrev[index] = mNPCStartPositions[index];
    mNPCStartPositions[index] = mGoal;
  }

  void OnPathNotFound(int i)
  {
    Debug.Log(i + " - Cannot find path to destination");
    if(StatusText)
    {
      StatusText.text = "Cannot find path to destination";
    }
  }

  public void OnClickRegenerate()
  {
    SceneManager.LoadScene("Combined_Demo_Graph");
  }

  public void OnClickRandomizeNPCs()
  {
    RandomizeNPCs();
  }

  public void SetInteractive(bool flag)
  {
    mInteractive = flag;
    if(mInteractive)
    {
      //UseThreads = false;
      for(int i = 0; i < NumNPC; ++i)
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
      //UseThreads = true;
    }
  }

  public void OnChangeCurrentNode(PathFinder<RandomGraphNode>.PathFinderNode node)
  {
    Update_Vertex_Viz(node, COLOR_CURRENT_NODE);
    SetFCost(node.Fcost);
    SetGCost(node.GCost);
    SetHCost(node.Hcost);
  }

  public void OnAddToOpenList(PathFinder<RandomGraphNode>.PathFinderNode node)
  {
    Update_Vertex_Viz(node, COLOR_OPEN_LIST);
  }

  public void OnAddToClosedList(PathFinder<RandomGraphNode>.PathFinderNode node)
  {
    Update_Vertex_Viz(node, COLOR_CLOSED_LIST);
  }
  private void Update_Vertex_Viz(PathFinder<RandomGraphNode>.PathFinderNode node, Color color)
  {
    RandomGraphNode_Viz cellScript = mNodeVertex_VizDic[node.Location.Value];

    if (cellScript)
    {
      cellScript.SetInnerColor(color);
    }
  }

  public void SetFCost(float cost)
  {
    mTextFCost.text = cost.ToString("F2");
  }

  public void SetHCost(float cost)
  {
    mTextHCost.text = cost.ToString("F2");
  }

  public void SetGCost(float cost)
  {
    mTextGCost.text = cost.ToString("F2");
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

  public void SetCostFunction(CostFunctionType type)
  {
    if(type == CostFunctionType.EUCLIDEN)
    {
      for (int i = 0; i < NumNPC; ++i)
      {
        // We create the different path finders
        ThreadedPathFinder<RandomGraphNode> tpf = mThreadedPool.GetThreadedPathFinder(i);
        tpf.PathFinder.HeuristicCost = RandomGraphNode.GetEuclideanCost;
        tpf.PathFinder.NodeTraversalCost = RandomGraphNode.GetEuclideanCost;

        PathFinder<RandomGraphNode> pf1 = mPathFinders[PathFinderTypes.ASTAR][i];
        PathFinder<RandomGraphNode> pf2 = mPathFinders[PathFinderTypes.DJIKSTRA][i];
        PathFinder<RandomGraphNode> pf3 = mPathFinders[PathFinderTypes.GREEDY_BEST_FIRST][i];

        pf1.HeuristicCost = RandomGraphNode.GetEuclideanCost;
        pf1.NodeTraversalCost = RandomGraphNode.GetEuclideanCost;
        pf2.HeuristicCost = RandomGraphNode.GetEuclideanCost;
        pf2.NodeTraversalCost = RandomGraphNode.GetEuclideanCost;
        pf3.HeuristicCost = RandomGraphNode.GetEuclideanCost;
        pf3.NodeTraversalCost = RandomGraphNode.GetEuclideanCost;
      }
    }
    else if (type == CostFunctionType.MANHATTAN)
    {
      for (int i = 0; i < NumNPC; ++i)
      {
        // We create the different path finders
        ThreadedPathFinder<RandomGraphNode> tpf = mThreadedPool.GetThreadedPathFinder(i);
        tpf.PathFinder.HeuristicCost = RandomGraphNode.GetManhattanCost;
        tpf.PathFinder.NodeTraversalCost = RandomGraphNode.GetEuclideanCost;

        PathFinder<RandomGraphNode> pf1 = mPathFinders[PathFinderTypes.ASTAR][i];
        PathFinder<RandomGraphNode> pf2 = mPathFinders[PathFinderTypes.DJIKSTRA][i];
        PathFinder<RandomGraphNode> pf3 = mPathFinders[PathFinderTypes.GREEDY_BEST_FIRST][i];

        pf1.HeuristicCost = RandomGraphNode.GetManhattanCost;
        pf1.NodeTraversalCost = RandomGraphNode.GetEuclideanCost;
        pf2.HeuristicCost = RandomGraphNode.GetManhattanCost;
        pf2.NodeTraversalCost = RandomGraphNode.GetEuclideanCost;
        pf3.HeuristicCost = RandomGraphNode.GetManhattanCost;
        pf3.NodeTraversalCost = RandomGraphNode.GetEuclideanCost;
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
        npc.SetPosition(mNPCStartPositions[i].Value.Point.x, mNPCStartPositions[i].Value.Point.y);
      }
    }
    FindPath();
  }

  public void SetFCostText(Text txt)
  {
    mTextFCost = txt;
  }

  public void SetGCostText(Text txt)
  {
    mTextGCost = txt;
  }

  public void SetHCostText(Text txt)
  {
    mTextHCost = txt;
  }

  public void SetNotificationText(Text txt)
  {
    StatusText = txt;
  }

  public string GetTitle()
  {
    return "Graph";
  }
}
