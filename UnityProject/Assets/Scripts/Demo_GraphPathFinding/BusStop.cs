using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusStop : System.IEquatable<BusStop>
{
  /// <summary>
  /// We will have only two variables for our BusStop. 
  /// One will be the location, and the other will be 
  /// the name of the BusStop. Note that these two variables 
  /// are only for demonstration of our bus stops. 
  /// 
  /// If you require other variables, then you can add 
  /// them as you wish. Our location variable can be 
  /// latitude and longitude or plain simple cartesian coordinates.
  /// </summary>
  public string Name { get; set; }
  public Vector2 Point { get; set; }

  #region Constructors
  public BusStop()
  {
  }

  public BusStop(string names, Vector2 point)
  {
    Name = names;
    Point = point;
  }

  public BusStop(string name, float x, float y)
  {
    Name = name;
    Point = new Vector2(x, y);
  }
  #endregion

  #region Functions related to Equal to hashcode
  public override bool Equals(object obj) =>
    this.Equals(obj as BusStop);

  public bool Equals(BusStop p)
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
  #endregion

  #region The cost functions and other utility functions
  public static float Distance(BusStop a, BusStop b)
  {
    return (a.Point - b.Point).magnitude;
  }

  public static float GetManhattanCost(
    BusStop a,
    BusStop b)
  {
    return Mathf.Abs(a.Point.x - b.Point.x) +
      Mathf.Abs(a.Point.y - b.Point.y);
  }

  public static float GetEuclideanCost(
    BusStop a,
    BusStop b)
  {
    return GetCostBetweenTwoStops(a, b);
  }

  public static float GetCostBetweenTwoStops(
    BusStop a,
    BusStop b)
  {
    return (a.Point - b.Point).magnitude;
  }

  public static float GetAngleBetweenTwoStops(
    BusStop a,
    BusStop b)
  {
    float delta_x = b.Point.x - a.Point.x;
    float delta_y = b.Point.y - a.Point.y;
    float theta_radians = Mathf.Atan2(delta_y, delta_x);
    return theta_radians;
  }
  #endregion
}