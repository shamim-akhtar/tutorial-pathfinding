using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Station : IEquatable<Station>
{
  public string Code { get; set; }
  public List<string> Names { get; set; } = new List<string>();
  public Vector2 Point { get; set; }
  public string Date { get; set; }

  public Station()
  {
  }

  public Station(List<string> names, Vector2 point)
  {
    Names = names.ToList();
    Point = point;
  }

  public Station(List<string> names, float x, float y)
  {
    Names = names.ToList();
    Point = new Vector2(x, y);
  }

  public override bool Equals(object obj) => 
    this.Equals(obj as Station);

  public bool Equals(Station p)
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
    return (Code == p.Code);
  }

  public override int GetHashCode() => 
    (Code, Point).GetHashCode();

  public static float Distance(Station a, Station b)
  {
    return (a.Point - b.Point).magnitude;
  }
}
