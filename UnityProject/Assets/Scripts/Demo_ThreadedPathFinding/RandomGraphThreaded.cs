using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameAI.PathFinding;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class RandomGraphThreaded : MonoBehaviour
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

  [SerializeField]
  Text StatusText;

  public int rows_cols = 20;
  public int rows_rows = 10;
  public float NodeSelectionProb = 0.6f;

  List<GameObject> mNPCs = new List<GameObject>();
  // The goal vertex
  List<Graph<RandomGraphNode>.Vertex> mNPCStartPositions = 
    new List<Graph<RandomGraphNode>.Vertex>();

  // The start vertex.
  Graph<RandomGraphNode>.Vertex mGoal;

  ThreadedPathFinderPool<RandomGraphNode> mThreadedPool = new ThreadedPathFinderPool<RandomGraphNode>();
  List<LineRenderer> mPathViz = new List<LineRenderer>();

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

      RandomGraphNode_Viz vertexViz = obj.AddComponent<RandomGraphNode_Viz>();
      vertexViz.SetVertex(mRandomGraphNodes.Vertices[i]);
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

    for (int i = 0; i < NumNPC; ++i)
    {
      GameObject Npc = Instantiate(NpcPrefab);
      mNPCs.Add(Npc);
      ThreadedPathFinder<RandomGraphNode> tpf = mThreadedPool.CreateThreadedAStarPathFinder();
      tpf.PathFinder.HeuristicCost = RandomGraphNode.GetManhattanCost;
      tpf.PathFinder.NodeTraversalCost = RandomGraphNode.GetEuclideanCost;

      // We create a line renderer to show the path.
      LineRenderer lr = Npc.AddComponent<LineRenderer>();
      mPathViz.Add(lr);
      lr.startWidth = 0.2f;
      lr.endWidth = 0.2f;
      lr.startColor = Color.magenta;
      lr.endColor = Color.magenta;
    }
    RandomizeNPCs();
  }

  public void RandomizeNPCs()
  {
    mNPCStartPositions.Clear();
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
    }
  }

  void Update()
  {
    if (Input.GetMouseButtonDown(1) ||
      Input.GetMouseButtonDown(0))
    {
      RayCastAndSetDestination();
    }

    for(int i = 0; i < NumNPC; ++i)
    {
      if(mThreadedPool.GetThreadedPathFinder(i).Done)
      {
        PathFinder<RandomGraphNode> pf = mThreadedPool.GetThreadedPathFinder(i).PathFinder;
        mThreadedPool.GetThreadedPathFinder(i).Done = false;

        if(pf.Status == PathFinderStatus.SUCCESS)
        {
          OnPathFound(i);
        }
        else if(pf.Status == PathFinderStatus.FAILURE)
        {
          OnPathNotFound(i);
        }
      }
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

    // by default enable camera panning.
    CameraMovement2D camMovement = Camera.main.gameObject.GetComponent<CameraMovement2D>();
    if(camMovement)
    {
      CameraMovement2D.CameraPanning = true;
    }

    if (hit)
    {
      // clear old lines.
      for (int i = 0; i < NumNPC; ++i)
      {
        mPathViz[i].positionCount = 0;
      }

      if (camMovement)
      {
        CameraMovement2D.CameraPanning = false;
      }
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

      // create threaded pool.
      for(int i = 0; i < mNPCs.Count; ++i)
      {
        mThreadedPool.FindPath(i, mNPCStartPositions[i], mGoal);
      }
    }
  }

  IEnumerator Coroutine_FindPathSteps(PathFinder<RandomGraphNode> pathFinder)
  {
    while (pathFinder.Status == PathFinderStatus.RUNNING)
    {
      pathFinder.Step();
      yield return null;
    }
  }

  public void OnPathFound(int index)
  {
    ThreadedPathFinder<RandomGraphNode> tpf = mThreadedPool.GetThreadedPathFinder(index);
    if (StatusText)
    {
      StatusText.text = "Path found to destination";
    }
    PathFinder<RandomGraphNode>.PathFinderNode node =
      tpf.PathFinder.CurrentNode;
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
    SceneManager.LoadScene("Demo_ThreadedPathFinding");
  }

  public void OnClickRandomizeNPCs()
  {
    RandomizeNPCs();
  }
}
