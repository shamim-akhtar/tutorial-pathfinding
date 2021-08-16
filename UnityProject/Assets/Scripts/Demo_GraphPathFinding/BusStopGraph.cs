using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameAI.PathFinding;

public class BusStopGraph : MonoBehaviour
{
  Graph<BusStop> mBusStopGraph = new Graph<BusStop>();
  private Rect mExtent = new Rect();

  [SerializeField]
  GameObject VertexPrefab;

  [SerializeField]
  NPC Npc;

  [SerializeField]
  Transform Destination;

  AStarPathFinder<BusStop> mPathFinder = 
    new AStarPathFinder<BusStop>();

  Graph<BusStop>.Vertex mGoal;
  Graph<BusStop>.Vertex mStart;

  Dictionary<string, GameObject> mVerticesMap =
    new Dictionary<string, GameObject>();

  LineRenderer mPathViz;

  public void CalculateExtent()
  {
    float minX = Mathf.Infinity;
    float minY = Mathf.Infinity;
    float maxX = -Mathf.Infinity;
    float maxY = -Mathf.Infinity;
    for (int i = 0; i < mBusStopGraph.Vertices.Count; ++i)
    {
      BusStop d = mBusStopGraph.Vertices[i].Value;
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

    int rows_cols =10;
    for (int i = 0; i < rows_cols; ++i)
    {
      for (int j = 0; j < rows_cols; ++j)
      {
        float toss = Random.Range(0.0f, 1.0f);
        if (toss >= 0.70f)
        {
          mBusStopGraph.AddVertex(
            new BusStop("stop_" + i + "_" + j, i, j));
        }
      }
    }

    // find the cost between all the vertices.
    List<List<float>> distances = 
      new List<List<float>>(mBusStopGraph.Count);

    List<List<float>> angles = 
      new List<List<float>>(mBusStopGraph.Count);

    for (int i = 0; i < mBusStopGraph.Count; ++i)
    {
      distances.Add(new List<float>());
      angles.Add(new List<float>());
      for (int j = 0; j < mBusStopGraph.Count; ++j)
      {
        distances[i].Add(BusStop.Distance(
          mBusStopGraph.Vertices[i].Value, 
          mBusStopGraph.Vertices[j].Value));

        angles[i].Add(BusStop.GetAngleBetweenTwoStops(
          mBusStopGraph.Vertices[i].Value, 
          mBusStopGraph.Vertices[j].Value));
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

          mBusStopGraph.AddDirectedEdge(
            mBusStopGraph.Vertices[i],
            mBusStopGraph.Vertices[idx[j]],
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

    for (int i = 0; i < mBusStopGraph.Vertices.Count; ++i)
    {

      Vector3 pos = Vector3.zero;
      pos.x = mBusStopGraph.Vertices[i].Value.Point.x;
      pos.y = mBusStopGraph.Vertices[i].Value.Point.y;
      pos.z = 0.0f;

      GameObject obj = Instantiate(
        VertexPrefab, 
        pos, 
        Quaternion.identity);

      obj.name = mBusStopGraph.Vertices[i].Value.Name;

      Vertex_Viz vertexViz = obj.AddComponent<Vertex_Viz>();
      vertexViz.SetVertex(mBusStopGraph.Vertices[i]);

      mVerticesMap[mBusStopGraph.Vertices[i].Value.Name] = obj;
    }

    CalculateExtent();
    Camera.main.orthographicSize = mExtent.width / 1.5f;
    Vector3 center = mExtent.center;
    center.z = -100.0f;
    Camera.main.transform.position = center;

    // randomly place our NPC to one of the vertices.
    int randIndex = Random.Range(0, mBusStopGraph.Count);
    Npc.transform.position = new Vector3(
      mBusStopGraph.Vertices[randIndex].Value.Point.x,
      mBusStopGraph.Vertices[randIndex].Value.Point.y,
      -1.0f);

    mStart = mBusStopGraph.Vertices[randIndex];

    mPathFinder.HeuristicCost = BusStop.GetManhattanCost;
    mPathFinder.NodeTraversalCost = BusStop.GetEuclideanCost;
    mPathFinder.onSuccess = OnPathFound;
    mPathFinder.onFailure = OnPathNotFound;

    // We create a line renderer to show the path.
    mPathViz = transform.gameObject.AddComponent<LineRenderer>();
    mPathViz.startWidth = 0.2f;
    mPathViz.endWidth = 0.2f;
    mPathViz.startColor = Color.magenta;
    mPathViz.endColor = Color.magenta;
  }

  void Update()
  {
    if (Input.GetMouseButtonDown(1))
    {
      RayCastAndSetDestination();
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
      Vertex_Viz sc = obj.GetComponent<Vertex_Viz>();
      if (sc == null) return;

      Vector3 pos = Destination.position;
      pos.x = sc.Vertex.Value.Point.x;
      pos.y = sc.Vertex.Value.Point.y;
      Destination.position = pos;

      mGoal = sc.Vertex;
      mPathFinder.Initialize(mStart, mGoal);
      StartCoroutine(Coroutine_FindPathSteps());
    }
  }

  IEnumerator Coroutine_FindPathSteps()
  {
    while (mPathFinder.Status == PathFinderStatus.RUNNING)
    {
      mPathFinder.Step();
      yield return null;
    }
  }

  public void OnPathFound()
  {
    PathFinder<BusStop>.PathFinderNode node = 
      mPathFinder.CurrentNode;
    List<BusStop> reverse_indices = new List<BusStop>();

    while (node != null)
    {
      reverse_indices.Add(node.Location.Value);
      node = node.Parent;
    }
    mPathViz.positionCount = reverse_indices.Count;
    for (int i = reverse_indices.Count - 1; i >= 0; i--)
    {
      Npc.AddWayPoint(new Vector2(
        reverse_indices[i].Point.x, 
        reverse_indices[i].Point.y));
      mPathViz.SetPosition(i, new Vector3(
        reverse_indices[i].Point.x,
        reverse_indices[i].Point.y,
        0.0f));
    }

    // We set the goal to be the start for next pathfinding
    mStart = mGoal;
  }

  void OnPathNotFound()
  {
    Debug.Log("Cannot find path to destination");
  }
}
