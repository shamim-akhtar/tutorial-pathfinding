using PathFinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
  public float Speed = 1.0f;
  public Queue<Vector2> wayPoints = new Queue<Vector2>();

  PathFinder<Vector2Int> pathFinder = new AStarPathFinder<Vector2Int>();
  public GridMap Map { get; set; }

  // Start is called before the first frame update
  void Start()
  {
    pathFinder.onSuccess = OnSuccessPathFinding;
    pathFinder.onFailure = OnFailurePathFinding;
    pathFinder.HeuristicCost = GridMap.GetManhattanCost;
    pathFinder.NodeTraversalCost = GridMap.GetEuclideanCost;
    StartCoroutine(Coroutine_MoveTo());
  }

  public void AddWayPoint(
    GridNode node)
  {
    wayPoints.Enqueue(new Vector2(
      node.Value.x * Map.GridNodeWidth,
      node.Value.y * Map.GridNodeHeight));
    GridNodeView gnv = Map.GetGridNodeView(node.Value.x, node.Value.y);
    gnv.SetInnerColor(Color.cyan);
  }

  public void SetDestination(
    GridNode destination)
  {

    // Now we have a pathfinder.
    if (pathFinder.Status == PathFinderStatus.RUNNING)
    {
      Debug.Log("Pathfinder already running. Cannot set destination now");
      return;
    }

    // remove all waypoints from the queue.
    wayPoints.Clear();

    // new start location is previous destination.
    GridNode start = Map.GetGridNode(
      (int)(transform.position.x / Map.GridNodeWidth),
      (int)(transform.position.y / Map.GridNodeHeight));

    if (start == null) return;

    pathFinder.onAddToClosedList = Map.OnAddToClosedList;
    pathFinder.onAddToOpenList = Map.OnAddToOpenList;
    pathFinder.onChangeCurrentNode = Map.OnChangeCurrentNode;
    Map.ResetCellColours();

    pathFinder.Initialise(start, destination);
    StartCoroutine(Coroutine_FindPathSteps());
  }

  IEnumerator Coroutine_FindPathSteps()
  {
    while (pathFinder.Status == PathFinderStatus.RUNNING)
    {
      pathFinder.Step();
      yield return new WaitForSeconds(0.1f);
    }
  }

  public void SetStart(
    GridNode start)
  {
    wayPoints.Clear();
    AddWayPoint(start);
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
            Speed));
      }
      yield return null;
    }
  }

  // coroutine to move smoothly
  private IEnumerator Coroutine_MoveOverSeconds(
    GameObject objectToMove,
    Vector3 end,
    float seconds)
  {
    float elapsedTime = 0;
    Vector3 startingPos = objectToMove.transform.position;
    while (elapsedTime < seconds)
    {
      objectToMove.transform.position =
        Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
      elapsedTime += Time.deltaTime;

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
        transform.gameObject,
        endP,
        duration));
  }

  void OnSuccessPathFinding()
  {
    PathFinder<Vector2Int>.PathFinderNode node = pathFinder.CurrentNode;
    List<Vector2Int> reverse_indices = new List<Vector2Int>();
    while (node != null)
    {
      reverse_indices.Add(node.Location.Value);
      node = node.Parent;
    }
    for (int i = reverse_indices.Count - 1; i >= 0; i--)
    {
      AddWayPoint(Map.GetGridNode(reverse_indices[i].x, reverse_indices[i].y));
    }
  }

  void OnFailurePathFinding()
  {
    Debug.Log("Error: Cannot find path");
  }
}
