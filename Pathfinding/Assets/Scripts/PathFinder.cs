using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PathFinding
{
  // An enumeration type to represent the various
  // states that the PathFinder can be at any given time.
  public enum PathFinderStatus
  {
    NOT_INITIALISED,
    SUCCESS,
    FAILURE,
    RUNNING,
  }

  // Class node. 
  // An abstract class that provides the base class
  // for any type of vertex that you may want to 
  // implement for your pathfinding.
  abstract public class Node<T>
  {
    public T Value { get; private set; }

    public Node(T value)
    {
      Value = value;
    }

    // Get the neighbours for this node.
    // Add derived class must implement this method.
    abstract public List<Node<T>> GetNeighbours();
  }

  public abstract class PathFinder<T>
  {
    #region Delegates for Cost Calculation.
    public delegate float CostFunction(T a, T b);
    public CostFunction HeuristicCost { get; set; }
    public CostFunction NodeTraversalCost { get; set; }
    #endregion

    #region PathFinderNode

    // The PathFinderNode class.
    // This class equates to a node on the tree generated
    // by the pathfinder in its search for the most optimal
    // path. Do not confuse this with the Node defined above.
    // This class encapsulates a Node and hold other
    // attributes needed for the pathfinding search.
    public class PathFinderNode : System.IComparable<PathFinderNode>
    {
      public PathFinderNode Parent { get; set; }
      public Node<T> Location { get; private set; }

      // The various costs.
      public float FCost { get; private set; }
      public float GCost { get; private set; }
      public float HCost { get; private set; }

      public PathFinderNode(Node<T> location,
        PathFinderNode parent,
        float gCost,
        float hCost)
      {
        Location = location;
        Parent = parent;
        HCost = hCost;
        SetGCost(gCost);
      }

      public void SetGCost(float c)
      {
        GCost = c;
        FCost = GCost + HCost;
      }

      public int CompareTo(PathFinderNode other)
      {
        if (other == null) return 1;
        return FCost.CompareTo(other.FCost);
      }
    }
    #endregion

    #region Properties

    // Add a property that holds the current status of the 
    // PathFinder. By default set it to NOT_INITIALISED
    public PathFinderStatus Status
    {
      get;
      private set;
    } = PathFinderStatus.NOT_INITIALISED;

    public Node<T> Start { get; private set; }
    public Node<T> Goal { get; private set; }

    // The property to access the current node 
    // that the pathfinder is now at.
    public PathFinderNode CurrentNode { get; private set; }

    #endregion

    #region Open and Closed Lists and Associated Functions.
    protected List<PathFinderNode> openList = 
      new List<PathFinderNode>();

    protected List<PathFinderNode> closedList =
      new List<PathFinderNode>();

    protected PathFinderNode GetLeastCostNode(
      List<PathFinderNode> myList)
    {
      int best_index = 0;
      float best_priority = myList[0].FCost;
      for(int i = 1; i < myList.Count; i++)
      {
        if(best_priority > myList[i].FCost)
        {
          best_priority = myList[i].FCost;
          best_index = i;
        }
      }
      PathFinderNode n = myList[best_index];
      return n;
    }

    protected int IsInList(List<PathFinderNode> myList, T cell)
    {
      for(int i = 0; i < myList.Count; i++)
      {
        if (EqualityComparer<T>.Default.Equals(myList[i].Location.Value, cell))
          return i;
      }
      return -1;
    }

    #endregion

    #region Delegates for Action Callbacks
    // We set some delegats to handle change to the internal
    // values during the pathfinding process.
    // These callbacks can be used to display visually
    // the changes to the cells and lists.
    public delegate void DelegatePathFinderNode(PathFinderNode node);
    public DelegatePathFinderNode onChangeCurrentNode;
    public DelegatePathFinderNode onAddToOpenList;
    public DelegatePathFinderNode onAddToCloasedList;
    public DelegatePathFinderNode onDestinationFound;

    public delegate void DelegateNoArguments();
    public DelegateNoArguments onStarted;
    public DelegateNoArguments onRunning;
    public DelegateNoArguments onFailure;
    public DelegateNoArguments onSuccess;
    #endregion

    #region Pathfinding Search Related Functions

    // Reset the internal variables for a new search.
    protected void Reset()
    {
      if(Status == PathFinderStatus.RUNNING)
      {
        // Cannot reset as a pathfinding is
        // currently in progress.
        return;
      }

      CurrentNode = null;
      openList.Clear();
      closedList.Clear();

      Status = PathFinderStatus.NOT_INITIALISED;
    }

    // Step until SUCCESS or FAILURE.
    // Take a search step. The user must call this
    // method until the Status returned is SUCCESS or FAILURE.
    public PathFinderStatus Step()
    {
      closedList.Add(CurrentNode);
      onAddToCloasedList?.Invoke(CurrentNode);

      if(openList.Count == 0)
      {
        // We have exhaused our search.
        Status = PathFinderStatus.FAILURE;
        onFailure?.Invoke();
        return Status;
      }

      // Get the least cost element from the openList.
      CurrentNode = GetLeastCostNode(openList);

      onChangeCurrentNode?.Invoke(CurrentNode);

      openList.Remove(CurrentNode);

      // Check if the node contains the goal cell.
      if(EqualityComparer<T>.Default.Equals(
        CurrentNode.Location.Value, Goal.Value))
      {
        Status = PathFinderStatus.SUCCESS;
        onDestinationFound?.Invoke(CurrentNode);
        onSuccess?.Invoke();
        return Status;
      }

      // Find the neignbours.
      List<Node<T>> neighbours = CurrentNode.Location.GetNeighbours();

      // Traverse each of these neighbours for 
      // possible expansion of the search.
      foreach(Node<T> cell in neighbours)
      {
        AlgorithmSpecificImplementation(cell);
      }

      Status = PathFinderStatus.RUNNING;
      onRunning?.Invoke();
      return Status;
    }

    abstract protected void AlgorithmSpecificImplementation(Node<T> cell);

    public bool Initialise(Node<T> start, Node<T> goal)
    {
      if(Status == PathFinderStatus.RUNNING)
      {
        // Pathfinding is currently in progress.
        return false;
      }

      Reset();

      Start = start;
      Goal = goal;

      float H = HeuristicCost(Start.Value, Goal.Value);

      PathFinderNode root = new PathFinderNode(Start, null, 0.0f, H);

      openList.Add(root);
      onAddToOpenList?.Invoke(root); 

      CurrentNode = root;

      onChangeCurrentNode?.Invoke(CurrentNode);
      onStarted?.Invoke();

      Status = PathFinderStatus.RUNNING;

      return true;
    }
    #endregion
  }

  #region Dijkstra's Algorithm

  // A cconcrete implementation of a Dijkstra PathFinder.
  public class DijkstraPathFinder<T> : PathFinder<T>
  {
    protected override void AlgorithmSpecificImplementation(Node<T> cell)
    {
      if(IsInList(closedList, cell.Value) == -1)
      {
        float G = CurrentNode.GCost + NodeTraversalCost(
          CurrentNode.Location.Value, cell.Value);

        // Heuristic cost for Dijkstra is 0.
        float H = 0.0f;

        int idOList = IsInList(openList, cell.Value);

        if(idOList == -1)
        {
          PathFinderNode n = new PathFinderNode(cell, CurrentNode, G, H);
          openList.Add(n);
          onAddToOpenList?.Invoke(n);
        }
        else
        {
          float oldG = openList[idOList].GCost;
          if(G < oldG)
          {
            openList[idOList].Parent = CurrentNode;
            openList[idOList].SetGCost(G);
            onAddToOpenList?.Invoke(openList[idOList]);
          }
        }
      }
    }
  }
  #endregion

  #region A* Algorithm
  public class AStarPathFinder<T> : PathFinder<T>
  {
    protected override void AlgorithmSpecificImplementation(Node<T> cell)
    {
      if(IsInList(closedList, cell.Value) == -1)
      {
        float G = CurrentNode.GCost + NodeTraversalCost(
          CurrentNode.Location.Value, cell.Value);
        float H = HeuristicCost(cell.Value, Goal.Value);

        int idOList = IsInList(openList, cell.Value);

        if(idOList == -1)
        {
          PathFinderNode n = new PathFinderNode(cell, CurrentNode, G, H);
          openList.Add(n);
          onAddToOpenList?.Invoke(n);
        }
        else
        {
          float oldG = openList[idOList].GCost;
          if(G < oldG)
          {
            openList[idOList].Parent = CurrentNode;
            openList[idOList].SetGCost(G);
            onAddToOpenList?.Invoke(openList[idOList]);
          }
        }
      }
    }
  }
  #endregion

  #region Greedy Best-First Search
  public class GreedyPathFinder<T> : PathFinder<T>
  {
    protected override void AlgorithmSpecificImplementation(Node<T> cell)
    {
      if (IsInList(closedList, cell.Value) == -1)
      {
        // G cost for Greedy search is 0.
        float G = 0;

        float H = HeuristicCost(cell.Value, Goal.Value);

        int idOList = IsInList(openList, cell.Value);

        if (idOList == -1)
        {
          PathFinderNode n = new PathFinderNode(cell, CurrentNode, G, H);
          openList.Add(n);
          onAddToOpenList?.Invoke(n);
        }
        else
        {
          float oldG = openList[idOList].GCost;
          if (G < oldG)
          {
            openList[idOList].Parent = CurrentNode;
            openList[idOList].SetGCost(G);
            onAddToOpenList?.Invoke(openList[idOList]);
          }
        }
      }
    }
  }
  #endregion
}
