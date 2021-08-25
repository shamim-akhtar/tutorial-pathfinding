using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class NPC : MonoBehaviour
{
  public float Speed = 5.0f;
  public Queue<Vector2> mWayPoints = new Queue<Vector2>();

  void Start()
  {
    StartCoroutine(Coroutine_MoveTo());
  }

  public void AddWayPoint(float x, float y)
  {
    AddWayPoint(new Vector2(x, y));
  }

  public void AddWayPoint(Vector2 pt)
  {
    mWayPoints.Enqueue(pt);
  }

  public void SetPosition(float x, float y)
  {
    mWayPoints.Clear();
    transform.position = new Vector3(x, y, transform.position.z);
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
