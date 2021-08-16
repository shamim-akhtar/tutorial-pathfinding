using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex_Viz : MonoBehaviour
{
  public Graph<BusStop>.Vertex Vertex { get { return mVertex; } }
  private Graph<BusStop>.Vertex mVertex;

  List<GameObject> mLines = new List<GameObject>();

  private LineRenderer GetOrCreateLine(int index)
  {
    if (index >= mLines.Count)
    {
      GameObject obj = new GameObject();
      obj.name = "line_" + index.ToString();
      obj.transform.SetParent(transform);
      obj.transform.position = new Vector3(0.0f, 0.0f, -1.0f);
      LineRenderer lr = obj.AddComponent<LineRenderer>();

      ConstantScreenLineWidth clw = obj.AddComponent<ConstantScreenLineWidth>();
      mLines.Add(obj);

      lr.material = new Material(Shader.Find("Sprites/Default"));

      lr.startColor = Color.green;
      lr.endColor = Color.green;
    }
    return mLines[index].GetComponent<LineRenderer>();
  }

  public void SetVertex_Perc(Graph<BusStop>.Vertex vertex)
  {
    mVertex = vertex;
    for(int i = 0; i < mVertex.Neighbours.Count; ++i)
    {
      Graph<BusStop>.Vertex n = (Graph<BusStop>.Vertex)mVertex.Neighbours[i];

      Vector3 a = new Vector3(mVertex.Value.Point.x, mVertex.Value.Point.y, -1.0f);
      Vector3 b = new Vector3(n.Value.Point.x, n.Value.Point.y, -1.0f);

      // find the direction.
      Vector3 dir = (b - a);
      float distance = dir.magnitude;
      dir.Normalize();

      // draw the first 95% of the line in white and then the last 5% in black.
      Vector3 c = a + dir * distance * 0.15f;
      Vector3 d = a + dir * distance * 0.85f;

      LineRenderer lr = GetOrCreateLine(i);

      float PercentHead = 0.2f;
      lr.widthCurve = new AnimationCurve(
            new Keyframe(0, 0.2f),
            new Keyframe(0.999f - PercentHead, 0.2f),  // neck of arrow
            new Keyframe(1 - PercentHead, 1.0f),  // max width of arrow head
            new Keyframe(1, 0f));  // tip of arrow
      lr.positionCount = 4;
      lr.SetPositions(
        new Vector3[]
        {
          c
          , Vector3.Lerp(c, d, 0.999f - PercentHead)
          , Vector3.Lerp(c, d, 1 - PercentHead)
          , d
        });
    }
  }

  public void SetVertex(Graph<BusStop>.Vertex vertex)
  {
    mVertex = vertex;
    for (int i = 0; i < mVertex.Neighbours.Count; ++i)
    {
      Graph<BusStop>.Vertex n = mVertex.Neighbours[i] as Graph<BusStop>.Vertex;

      Vector3 a = new Vector3(
        mVertex.Value.Point.x, 
        mVertex.Value.Point.y, 
        -1.0f);
      Vector3 b = new Vector3(
        n.Value.Point.x, 
        n.Value.Point.y, 
        -1.0f);

      // find the direction.
      Vector3 dir = (b - a);
      float distance = dir.magnitude;
      dir.Normalize();

      // instead of percentage use fixed lengths
      // and arrow heads so that they dont scale.
      Vector3 c = a + dir * 0.22f;
      Vector3 d = b - dir * 0.2f;
      Vector3 e = b - dir * 0.31f;
      Vector3 f = b - dir * 0.3f;

      float p1 = (e - c).magnitude / (d - c).magnitude;
      float p2 = (f - c).magnitude / (d - c).magnitude;

      LineRenderer lr = GetOrCreateLine(i);

      lr.widthCurve = new AnimationCurve(
            new Keyframe(0, 0.05f),
            new Keyframe(p1, 0.05f), // neck of arrow
            new Keyframe(p2, 0.25f), // max width of arrow head
            new Keyframe(1, 0f));   // tip of arrow
      lr.positionCount = 4;
      lr.SetPositions(
        new Vector3[]
        {
          c,
          e,
          f,
          d
        });
    }
  }
}
