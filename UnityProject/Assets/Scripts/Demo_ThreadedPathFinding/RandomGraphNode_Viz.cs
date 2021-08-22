using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class RandomGraphNode_Viz : MonoBehaviour
{
  public Graph<RandomGraph.RandomGraphNode>.Vertex Vertex
  {
    get
    {
      return mVertex;
    }
  }
  private Graph<RandomGraph.RandomGraphNode>.Vertex mVertex;

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

      ConstantScreenLineWidth clw = 
        obj.AddComponent<ConstantScreenLineWidth>();
      mLines.Add(obj);

      lr.material = new Material(Shader.Find("Sprites/Default"));

      lr.startColor = Color.green;
      lr.endColor = Color.green;
    }
    return mLines[index].GetComponent<LineRenderer>();
  }

  public void SetVertex(Graph<RandomGraph.RandomGraphNode>.Vertex vertex)
  {
    mVertex = vertex;
    for (int i = 0; i < mVertex.Neighbours.Count; ++i)
    {
      Graph<RandomGraph.RandomGraphNode>.Vertex n = 
        mVertex.Neighbours[i] as Graph<RandomGraph.RandomGraphNode>.Vertex;

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
