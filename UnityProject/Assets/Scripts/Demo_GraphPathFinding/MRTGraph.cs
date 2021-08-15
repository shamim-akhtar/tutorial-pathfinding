using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oware;

public class MRTGraph : Graph<Station>
{
  static LatLngUTMConverter s_LatLonConverter = new LatLngUTMConverter("WGS 84");

  static Station CreateStation(
    string nameEng,
    string nameChi,
    string nameTam,
    string date,
    string code,
    float x,
    float y)
  {
    Station station = new Station();
    station.Names.Add(nameEng);
    station.Names.Add(nameChi);
    station.Names.Add(nameTam);
    station.Date = date;
    station.Code = code;

    //LatLngUTMConverter.UTMResult res = s_LatLonConverter.convertLatLngToUtm(lon, lat);
    //float e = (float)res.Easting;
    //float n = (float)res.Northing;
    //station.Point = new Vector2(e, n);
    station.Point = new Vector2(x, y);

    return station;
  }

  public Rect Extent { get { return mExtent; } }
  private Rect mExtent;

  //public TextAsset CSVFile;

  public MRTGraph()
  {
    // we will create the graph here manually.
    // Ideally we should be able to load and
    // save the graph from a file.
    //AddVertex(CreateStation("Jurong East", "裕廊东", "ஜூரோங் கிழக்கு", "10 March 1990", "NS1", 103.7421f, 1.3334f));
    //AddVertex(CreateStation("Bukit Batok", "武吉巴督", "புக்கிட் பாத்தோக்", "10 March 1990", "NS2", 103.7496f, 1.3491f));
    //AddVertex(CreateStation("Bukit Gombak", "武吉巴督", "புக்கிட் பாத்தோக்", "10 March 1990", "NS3", 103.7518f, 1.3589f));
    //AddVertex(CreateStation("Choa Chu Kang", "武吉巴督", "புக்கிட் பாத்தோக்", "10 March 1990", "NS4", 103.7443f, 1.3854f));
    //LoadCSV();
  }

  public void CalculateExtent()
  {
    float minX = Mathf.Infinity;
    float minY = Mathf.Infinity;
    float maxX = -Mathf.Infinity;
    float maxY = -Mathf.Infinity;
    for (int i = 0; i < Vertices.Count; ++i)
    {
      Station d = Vertices[i].Value;
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

  public static float GetManhattanCost(
    Station a, 
    Station b)
  {
    return Mathf.Abs(a.Point.x - b.Point.x) +
      Mathf.Abs(a.Point.y - b.Point.y);
  }

  public static float GetEuclideanCost(
    Station a, 
    Station b)
  {
    return GetCostBetweenTwoCells(a, b);
  }

  public static float GetCostBetweenTwoCells(
    Station a, 
    Station b)
  {
    return (a.Point - b.Point).magnitude;
  }
}
