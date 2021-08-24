using UnityEngine;

public class RandomGraphNode : System.IEquatable<RandomGraphNode>
{
  public string Name { get; set; }
  public Vector2 Point { get; set; }

  #region Constructors
  public RandomGraphNode()
  {
  }

  public RandomGraphNode(string names, Vector2 point)
  {
    Name = names;
    Point = point;
  }

  public RandomGraphNode(string name, float x, float y)
  {
    Name = name;
    Point = new Vector2(x, y);
  }
  #endregion

  #region Functions related to Equal to hashcode
  public override bool Equals(object obj) =>
    this.Equals(obj as RandomGraphNode);

  public bool Equals(RandomGraphNode p)
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
  public static float Distance(RandomGraphNode a, RandomGraphNode b)
  {
    return (a.Point - b.Point).magnitude;
  }

  public static float GetManhattanCost(
    RandomGraphNode a,
    RandomGraphNode b)
  {
    return Mathf.Abs(a.Point.x - b.Point.x) +
      Mathf.Abs(a.Point.y - b.Point.y);
  }

  public static float GetEuclideanCost(
    RandomGraphNode a,
    RandomGraphNode b)
  {
    return GetCostBetweenTwoStops(a, b);
  }

  public static float GetCostBetweenTwoStops(
    RandomGraphNode a,
    RandomGraphNode b)
  {
    return (a.Point - b.Point).magnitude;
  }

  public static float GetAngleBetweenTwoStops(
    RandomGraphNode a,
    RandomGraphNode b)
  {
    float delta_x = b.Point.x - a.Point.x;
    float delta_y = b.Point.y - a.Point.y;
    float theta_radians = Mathf.Atan2(delta_y, delta_x);
    return theta_radians;
  }
  #endregion
}
