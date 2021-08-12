using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class NPCMovement : MonoBehaviour
{
  public float Speed = 1.0f;
  public Queue<Vector2> mWayPoints = new Queue<Vector2>();

  PathFinder<Vector2Int> mPathFinder = new AStarPathFinder<Vector2Int>();

  // Start is called before the first frame update
  void Start()
  {
    mPathFinder.onSuccess = OnSuccessPathFinding;
    mPathFinder.onFailure = OnFailurePathFinding;
    mPathFinder.HeuristicCost = RectGrid_Viz.GetManhattanCost;
    mPathFinder.NodeTraversalCost = RectGrid_Viz.GetEuclideanCost;
    StartCoroutine(Coroutine_MoveTo());
  }

  public void AddWayPoint(Vector2 pt)
  {
    mWayPoints.Enqueue(pt);
  }

  public void SetDestination(
    RectGrid_Viz map, 
    RectGridCell destination)
  {
    //// we do not have pathfinding yet, so
    //// we just add the destination as a waypoint.
    //AddWayPoint(destination.Value);

    // Now we have a pathfinder.
    if (mPathFinder.Status == PathFinderStatus.RUNNING)
    {
      Debug.Log("Pathfinder already running. Cannot set destination now");
      return;
    }

    // remove all waypoints from the queue.
    mWayPoints.Clear();

    // new start location is previous destination.
    RectGridCell start = map.GetRectGridCell(
      (int)transform.position.x, 
      (int)transform.position.y);

    if (start == null) return;

    //mPathFinder.onAddToClosedList = map.OnAddToClosedList;
    //mPathFinder.onAddToOpenList = map.OnAddToOpenList;
    //mPathFinder.onChangeCurrentNode = map.OnChangeCurrentNode;
    map.ResetCellColours();

    mPathFinder.Initialize(start, destination);
    StartCoroutine(Coroutine_FindPathSteps());
  }

  IEnumerator Coroutine_FindPathSteps()
  {
    while(mPathFinder.Status == PathFinderStatus.RUNNING)
    {
      mPathFinder.Step();
      yield return null;
    }
  }

  void OnSuccessPathFinding()
  {
    PathFinder<Vector2Int>.PathFinderNode node = mPathFinder.CurrentNode;
    List<Vector2Int> reverse_indices = new List<Vector2Int>();
    while(node != null)
    {
      reverse_indices.Add(node.Location.Value);
      node = node.Parent;
    }
    for(int i = reverse_indices.Count -1; i >= 0; i--)
    {
      AddWayPoint(new Vector2(reverse_indices[i].x, reverse_indices[i].y));
    }
  }

  void OnFailurePathFinding()
  {
    Debug.Log("Error: Cannot find path");
  }

  public IEnumerator Coroutine_MoveTo()
  {
    while (true)
    {
      while (mWayPoints.Count > 0)
      {
        yield return StartCoroutine(
          Coroutine_MoveToPoint(
            mWayPoints.Dequeue(), 
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
}
