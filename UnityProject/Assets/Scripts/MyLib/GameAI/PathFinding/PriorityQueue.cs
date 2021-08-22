using System;
using System.Collections.Generic;

namespace PriorityQueue
{
  public static class MyExtensions
  {
    #region Public extension methods for List
    // Use
    public static void Enqueue<T>(
      this List<T> _pq, 
      T item) 
      where T : IComparable<T>
    {
      _pq.Add(item);
      BubbleUp(_pq);
    }

    public static T Dequeue<T>(
      this List<T> _pq) 
      where T : IComparable<T>
    {
      var highestPrioritizedItem = _pq[0];

      MoveLastItemToTheTop(_pq);
      SinkDown(_pq);

      return highestPrioritizedItem;
    }
    #endregion

    #region Private helper methods
    private static void BubbleUp<T>(
      List<T> _pq) 
      where T : IComparable<T>
    {
      var childIndex = _pq.Count - 1;
      while (childIndex > 0)
      {
        var parentIndex = (childIndex - 1) / 2;
        if (_pq[childIndex].CompareTo(
          _pq[parentIndex]) >= 0)
        {
          break;
        }
        Swap(_pq, childIndex, parentIndex);
        childIndex = parentIndex;
      }
    }

    private static void Swap<T>(
      List<T> _pq, 
      int index1, 
      int index2) 
      where T : IComparable<T>
    {
      var tmp = _pq[index1];
      _pq[index1] = _pq[index2];
      _pq[index2] = tmp;
    }

    private static void MoveLastItemToTheTop<T>(
      List<T> _pq) 
      where T : IComparable<T>
    {
      var lastIndex = _pq.Count - 1;
      _pq[0] = _pq[lastIndex];
      _pq.RemoveAt(lastIndex);
    }

    private static void SinkDown<T>(
      List<T> _pq) 
      where T : IComparable<T>
    {
      var lastIndex = _pq.Count - 1;
      var parentIndex = 0;

      while (true)
      {
        var firstChildIndex = parentIndex * 2 + 1;
        if (firstChildIndex > lastIndex)
        {
          break;
        }
        var secondChildIndex = firstChildIndex + 1;
        if (secondChildIndex <= lastIndex && 
          _pq[secondChildIndex].CompareTo(
            _pq[firstChildIndex]) < 0)
        {
          firstChildIndex = secondChildIndex;
        }
        if (_pq[parentIndex].CompareTo(_pq[firstChildIndex]) < 0)
        {
          break;
        }
        Swap(_pq, parentIndex, firstChildIndex);
        parentIndex = firstChildIndex;
      }
    }
    #endregion
  }
}