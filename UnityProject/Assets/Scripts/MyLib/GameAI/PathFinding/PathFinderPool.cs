using System.Collections;
using System.Collections.Generic;
using GameAI.PathFinding;
using Unity.Jobs;
using UnityEngine;
using System.Threading;

public class ThreadedPathFinder<T>
{
  PathFinder<T> mPathFinder;
  public PathFinder<T> PathFinder
  {
    get
    {
      return mPathFinder;
    }
  }

  public int Index { get; private set; }
  public bool Done { get; set; } = false;

  public ThreadedPathFinder(
    PathFinder<T> pf, 
    int index)
  {
    mPathFinder = pf;
    Index = index;
  }

  public void Execute()
  {
    while(mPathFinder.Status == 
      PathFinderStatus.RUNNING)
    {
      mPathFinder.Step();
    }
    Done = true;
  }
}

public class ThreadedPathFinderPool<T>
{
  private List<ThreadedPathFinder<T>> mPathFinders = 
    new List<ThreadedPathFinder<T>>();

  public ThreadedPathFinder<T> 
    CreateThreadedAStarPathFinder()
  {
    ThreadedPathFinder<T> tpf = 
      new ThreadedPathFinder<T>(
        new AStarPathFinder<T>(), 
        mPathFinders.Count);
    mPathFinders.Add(tpf);
    return tpf;
  }

  public ThreadedPathFinder<T>
    CreateThreadedDijkstraPathFinder()
  {
    ThreadedPathFinder<T> tpf =
      new ThreadedPathFinder<T>(
        new DijkstraPathFinder<T>(),
        mPathFinders.Count);
    mPathFinders.Add(tpf);
    return tpf;
  }

  public ThreadedPathFinder<T>
    CreateThreadedGreedyPathFinder()
  {
    ThreadedPathFinder<T> tpf =
      new ThreadedPathFinder<T>(
        new GreedyPathFinder<T>(),
        mPathFinders.Count);
    mPathFinders.Add(tpf);
    return tpf;
  }

  public ThreadedPathFinder<T> 
    GetThreadedPathFinder(int index)
  {
    return mPathFinders[index];
  }

  public void FindPath(
    ThreadedPathFinder<T> tpf,
    Node<T> start,
    Node<T> goal)
  {
    tpf.PathFinder.Initialize(start, goal);
    ThreadStart starter = tpf.Execute;
    Thread thread = new Thread(starter);
    thread.IsBackground = true;
    thread.Start();
    //thread.Join();
  }

  public void FindPath(
    int index,
    Node<T> start,
    Node<T> goal)
  {
    if (index < 0 || index >= mPathFinders.Count)
      return;
    FindPath(mPathFinders[index], start, goal);
  }
}