using PathFinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
  public float speed = 2.0f;
  public Queue<Vector2> wayPoints = new Queue<Vector2>();

  PathFinder<Vector2Int> pathFinder = new AStarPathFinder<Vector2Int>();

  public GridMap Map { get; set; }

  private IEnumerator Coroutine_MoveOverSeconds(
    GameObject objectToMove,
    Vector3 end,
    float seconds)
  {
    float elaspedTime = 0.0f;
    Vector3 startingPos = objectToMove.transform.position;

    while (elaspedTime < seconds)
    {
      objectToMove.transform.position =
        Vector3.Lerp(startingPos, end, elaspedTime / seconds);
      elaspedTime += Time.deltaTime;

      yield return new WaitForEndOfFrame();
    }
    objectToMove.transform.position = end;
  }

  IEnumerator Coroutine_MoveToPoint(Vector2 p, float speed)
  {
    Vector3 endP = new Vector3(p.x, p.y, transform.position.z);
    float duration = (transform.position - endP).magnitude / speed;

    yield return StartCoroutine(
      Coroutine_MoveOverSeconds(
        transform.gameObject, endP, duration));
  }

  public IEnumerator Coroutine_MoveTo()
  {
    while (true)
    {
      while (wayPoints.Count > 0)
      {
        yield return StartCoroutine(
          Coroutine_MoveToPoint(
            wayPoints.Dequeue(),
            speed));
      }
      yield return null;
    }
  }

  private void AddWayPoint(GridNode node)
  {
    wayPoints.Enqueue(new Vector2(
      node.Value.x * Map.GridNodeWidth,
      node.Value.y * Map.GridNodeHeight));

    // We set a color to show the path.
    GridNodeView gnv = Map.GetGridNodeView(node.Value.x, node.Value.y);
    gnv.SetInnerColor(Map.COLOR_PATH);
  }

  public void SetStartNode(GridNode node)
  {
    wayPoints.Clear();
    transform.position = new Vector3(
      node.Value.x * Map.GridNodeWidth,
      node.Value.y * Map.GridNodeHeight,
      transform.position.z);
  }

  public void MoveTo(GridNode destination)
  {
    if(pathFinder.Status == PathFinderStatus.RUNNING)
    {
      Debug.Log("PathFinder is running. Cannot start a new pathfinding now");
      return;
    }

    GridNode start = Map.GetGridNode(
      (int)(transform.position.x / Map.GridNodeWidth),
      (int)(transform.position.y / Map.GridNodeHeight));

    SetStartNode(start);

    pathFinder.onAddToCloasedList = Map.OnAddToClosedList;
    pathFinder.onAddToOpenList = Map.OnAddToOpenList;
    pathFinder.onChangeCurrentNode= Map.OnChangeCurrentNode;

    // We will need to reset the colours from previous
    // pathfinding search (if any).
    Map.ResetGridNodeColours();

    //AddWayPoint(destination);

    pathFinder.Initialise(start, destination);
    StartCoroutine(Coroutine_FindPathStep());
  }

  IEnumerator Coroutine_FindPathStep()
  {
    while(pathFinder.Status == PathFinderStatus.RUNNING)
    {
      pathFinder.Step();
      // We purposely make it slower so that we can
      // visualise the search.
      yield return new WaitForSeconds(0.1f);
    }
  }

  private void Start()
  {
    pathFinder.onSuccess = OnSuccessPathFinding;
    pathFinder.onFailure = OnFailurePathFinding;
    pathFinder.HeuristicCost = GridMap.GetManhattanCost;
    pathFinder.NodeTraversalCost = GridMap.GetEuclideanCost;
    StartCoroutine(Coroutine_MoveTo());
  }

  void OnSuccessPathFinding()
  {
    PathFinder<Vector2Int>.PathFinderNode node = pathFinder.CurrentNode;
    List<Vector2Int> reverse_indices = new List<Vector2Int>();
    while(node != null)
    {
      reverse_indices.Add(node.Location.Value);
      node = node.Parent;
    }

    for(int i = reverse_indices.Count - 1; i >= 0; i--)
    {
      AddWayPoint(Map.GetGridNode(reverse_indices[i].x, reverse_indices[i].y));
    }
  }

  void OnFailurePathFinding()
  {
    Debug.Log("Cannot find path. No valid path exists!");
  }
}
