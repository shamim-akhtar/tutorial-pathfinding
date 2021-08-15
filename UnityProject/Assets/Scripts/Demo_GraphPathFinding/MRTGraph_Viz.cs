using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class MRTGraph_Viz : MonoBehaviour
{

  MRTGraph MRT = new MRTGraph();
  [SerializeField]
  GameObject VertexPrefab;

  Dictionary<string, GameObject> mVerticesMap = new Dictionary<string, GameObject>();

  // Start is called before the first frame update
  void Start()
  {
    LoadCSV();

    // create the vertices of the graph.
    for(int i = 0; i < MRT.Count; ++i)
    {
      Vector3 pos = Vector3.zero;
      pos.x = MRT.Vertices[i].Value.Point.x;
      pos.y = MRT.Vertices[i].Value.Point.y;
      pos.z = 0.0f;

      GameObject obj = Instantiate(VertexPrefab, pos, Quaternion.identity);
      mVerticesMap[MRT.Vertices[i].Value.Code] = obj;
      obj.name = MRT.Vertices[i].Value.Names[0];
    }

    MRT.CalculateExtent();
    Camera.main.orthographicSize = MRT.Extent.width;
    Vector3 center = MRT.Extent.center;
    center.z = -100.0f;
    Camera.main.transform.position = center;
  }

  void LoadCSV()
  {
    TextAsset textFile = (TextAsset)Resources.Load("mrt_lrt");
    string fs = textFile.text;
    string[] fLines = Regex.Split(fs, "\n|\r|\r\n");

    for (int i = 0; i < fLines.Length; i++)
    {
      string valueLine = fLines[i];
      string[] values = Regex.Split(valueLine, ",");

      Station station = new Station();
      station.Names.Add(values[3]);
      station.Code = values[3];
      station.Point = new Vector2(float.Parse(values[4]), float.Parse(values[5]));
      MRT.AddVertex(station);
    }
  }

  // Update is called once per frame
  void Update()
  {

  }
}
