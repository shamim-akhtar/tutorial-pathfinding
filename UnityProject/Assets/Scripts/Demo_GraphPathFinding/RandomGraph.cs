using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Stop : System.IEquatable<Stop>
{
  public string Name { get; set; }
  public Vector2 Point { get; set; }

  public Stop()
  {
  }

  public Stop(string names, Vector2 point)
  {
    Name = names;
    Point = point;
  }

  public Stop(string name, float x, float y)
  {
    Name = name;
    Point = new Vector2(x, y);
  }

  public override bool Equals(object obj) =>
    this.Equals(obj as Stop);

  public bool Equals(Stop p)
  {
    if (p is null)
    {
      return false;
    }

    // Optimization for a common success case.
    if (System.Object.ReferenceEquals(this, p))
    {
      return true;
    }

    // If run-time types are not exactly the same,
    // return false.
    if (this.GetType() != p.GetType())
    {
      return false;
    }

    // Return true if the fields match.
    // Note that the base class is not invoked 
    // because it is System.Object, which defines 
    // Equals as reference equality.
    return (Name == p.Name);
  }

  public override int GetHashCode() =>
    (Name, Point).GetHashCode();

  public static float Distance(Stop a, Stop b)
  {
    return (a.Point - b.Point).magnitude;
  }

  public static float GetManhattanCost(
    Stop a,
    Stop b)
  {
    return Mathf.Abs(a.Point.x - b.Point.x) +
      Mathf.Abs(a.Point.y - b.Point.y);
  }

  public static float GetEuclideanCost(
    Stop a,
    Stop b)
  {
    return GetCostBetweenTwoCells(a, b);
  }

  public static float GetCostBetweenTwoCells(
    Stop a,
    Stop b)
  {
    return (a.Point - b.Point).magnitude;
  }

  public static float GetAngleBetweenTwoCells(
    Stop a,
    Stop b)
  {
    float delta_x = b.Point.x - a.Point.x;
    float delta_y = b.Point.y - a.Point.y;
    float theta_radians = Mathf.Atan2(delta_y, delta_x);
    return theta_radians;
  }
}

public class RandomGraph : MonoBehaviour
{
  Graph<Stop> mBusStopGraph = new Graph<Stop>();
  private Rect mExtent = new Rect();

  [SerializeField]
  GameObject VertexPrefab;

  Dictionary<string, GameObject> mVerticesMap = new Dictionary<string, GameObject>();

  public void CreateRandomGraph()
  {
    int maxVertices = 20;

    // compute the maximum possible number of edges
    int maxPossibleEdges = maxVertices * (maxVertices - 1);

    // and randomly choose the number of edges less than
    // or equal to the maximum number of possible edges
    int maxEdges = 40;// Random.Range(0, maxPossibleEdges+1);

    // Creating an adjacency list
    // representation for the random graph
    List<List<int>> adjacencyList = new List<List<int>>(maxVertices);
    for (int i = 0; i < maxVertices; i++)
    {
      adjacencyList.Add(new List<int>());
    }

    // A for loop to randomly generate edges
    for (int i = 0; i < maxEdges; i++)
    {
      // Randomly select two vertices to
      // create an edge between them
      int v = Random.Range(0, maxVertices);
      int w = Random.Range(0, maxVertices);

      // Check if there is already an edge between v and w
      if ((v == w) || adjacencyList[v].Contains(w))
      {
        // Reduce the value of i
        // so that again v and w can be chosen
        // for the same edge count
        i = i - 1;
        continue;
      }

      // Add an edge between them if
      // not previously created
      adjacencyList[v].Add(w);
    }

    // convert the adjacency list to our graph.
    for(int i = 0; i < maxVertices; ++i)
    {
      mBusStopGraph.AddVertex(new Stop("stop_" + i, Random.Range(-100.0f, 100.0f), Random.Range(-100.0f, 100.0f)));
    }

    for(int i = 0; i < adjacencyList.Count; ++i)
    {
      for(int j = 0; j < adjacencyList[i].Count; ++j)
      {
        int to = adjacencyList[i][j];
        mBusStopGraph.AddDirectedEdge(
          mBusStopGraph.Vertices[i],
          mBusStopGraph.Vertices[to],
          Stop.GetCostBetweenTwoCells(mBusStopGraph.Vertices[i].Value, mBusStopGraph.Vertices[to].Value));
      }
    }
  }

  public void CalculateExtent()
  {
    float minX = Mathf.Infinity;
    float minY = Mathf.Infinity;
    float maxX = -Mathf.Infinity;
    float maxY = -Mathf.Infinity;
    for (int i = 0; i < mBusStopGraph.Vertices.Count; ++i)
    {
      Stop d = mBusStopGraph.Vertices[i].Value;
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

  void CreateRandomGraph1()
  {
    Random.InitState(10);
    //bool[,] flags = new bool[10, 10];

    int rows_cols =10;
    for (int i = 0; i < rows_cols; ++i)
    {
      for (int j = 0; j < rows_cols; ++j)
      {
        //flags[i, j] = false;
        float toss = Random.Range(0.0f, 1.0f);
        if (toss >= 0.75f)
        {
          mBusStopGraph.AddVertex(new Stop("stop_" + i + "_" + j, i, j));
          //flags[i, j] = true;
        }
      }
    }

    //mBusStopGraph.AddDirectedEdge(mBusStopGraph.Vertices[0], mBusStopGraph.Vertices[1],
    //  Stop.GetAngleBetweenTwoCells(mBusStopGraph.Vertices[0].Value, mBusStopGraph.Vertices[1].Value));

    // find the cost between all the vertices.
    List<List<float>> distances = new List<List<float>>(mBusStopGraph.Count);
    List<List<float>> angles = new List<List<float>>(mBusStopGraph.Count);
    for (int i = 0; i < mBusStopGraph.Count; ++i)
    {
      distances.Add(new List<float>());
      angles.Add(new List<float>());
      for (int j = 0; j < mBusStopGraph.Count; ++j)
      {
        distances[i].Add(Stop.Distance(
          mBusStopGraph.Vertices[i].Value, 
          mBusStopGraph.Vertices[j].Value));

        angles[i].Add(Stop.GetAngleBetweenTwoCells(
          mBusStopGraph.Vertices[i].Value, 
          mBusStopGraph.Vertices[j].Value));
      }

      var sorted = distances[i]
       .Select((x, k) => new KeyValuePair<float, int>(x, k))
       .OrderBy(x => x.Key)
       .ToList();

      List<float> B = sorted.Select(x => x.Key).ToList();
      List<int> idx = sorted.Select(x => x.Value).ToList();

      // connect the nearest 1 to 4 vertices.
      int index = Random.Range(2, 4);
      int id = 0;

      Dictionary<float, bool> angleFilled = new Dictionary<float, bool>();

      for (int j = 1; j < B.Count-1; ++j)
      {
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
    CreateRandomGraph1();

    for (int i = 0; i < mBusStopGraph.Vertices.Count; ++i)
    {

      Vector3 pos = Vector3.zero;
      pos.x = mBusStopGraph.Vertices[i].Value.Point.x;
      pos.y = mBusStopGraph.Vertices[i].Value.Point.y;
      pos.z = 0.0f;

      GameObject obj = Instantiate(VertexPrefab, pos, Quaternion.identity);
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
  }

  void Update()
  {

  }
}
