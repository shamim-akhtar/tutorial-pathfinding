using System.Collections.Generic;

namespace GameAI
{
    namespace PathFinding
    {
        // An enumeration type to represent the status of the 
        // pathfinder at any given time.
        public enum PathFinderStatus
        {
            NOT_INITIALIZED,
            SUCCESS,
            FAILURE,
            RUNNING,
        }

        // The Noce class. 
        // It is an abstract class that provides the base class
        // for any type of vertex that you want to implement in
        // your path finding problem.
        abstract public class Node<T>
        {
            // We store a reference to the T as Value.
            public T Value { get; private set; }

            // The constructor for the Node class.
            public Node(T value)
            {
                Value = value;
            }

            // Get the neighbours for this node. 
            // This is the most important function that 
            // your concrete vertex class should implement.
            abstract public List<Node<T>> GetNeighbours();
        }

        // The abstract PathFinder class that implements the core
        // pathfinding related codes.
        abstract public class PathFinder<T>
        {

            #region Delegates for cost calculation
            // Create a delegate that defines the signature
            // for calculating the cost between two 
            // Nodes (T which makes a Node)
            public delegate float CostFunction(T a, T b);
            public CostFunction HeuristicCost { get; set; }
            public CostFunction NodeTraversalCost { get; set; }
            #endregion

            #region PathFinderNode
            // The PathFinderNode class.
            // This class equates to a node in a the tree generated
            // by the pathfinder in its search for the most optimal
            // path. Do not confuse this with the Node class on top.
            // This class encapsulates a Node and hold other attributes
            // needed for the search traversal.
            // The pathfinder creates instances of this class at runtime
            // while doing the search.
            public class PathFinderNode
            {
                // The parent of this node.
                public PathFinderNode Parent { get; set; }

                // The Node that this PathFinderNode is pointing to.
                public Node<T> Location { get; private set; }

                // The various costs.
                public float Fcost { get; private set; }
                public float GCost { get; private set; }
                public float Hcost { get; private set; }

                // The constructor.
                // It takes in the Node, the parent, the gvost and the hcost.
                public PathFinderNode(Node<T> location, 
                    PathFinderNode parent, 
                    float gCost, 
                    float hCost)
                {
                    Location = location;
                    Parent = parent;
                    Hcost = hCost;
                    SetGCost(gCost);
                }

                // Set the gcost. 
                public void SetGCost(float c)
                {
                    GCost = c;
                    Fcost = GCost + Hcost;
                }
            }
            #endregion

            #region Properties

            // Add a property that holds the current status of the
            // pathfinder. By default it is set to NOT_INITIALIZED.
            // Also note that we have made the set to private to 
            // ensure that only this class can change and set
            // the status.
            public PathFinderStatus Status
            {
                get;
                private set;
            } = PathFinderStatus.NOT_INITIALIZED;

            // Add properties for the start and goal nodes.
            public Node<T> Start { get; private set; }
            public Node<T> Goal { get; private set; }

            // The property to access the CurrentNode that the
            // pathfinder is now at.
            public PathFinderNode CurrentNode { get; private set; }

            #endregion

            #region Open and Closed lists and associated functions
            // The open list for the path finder.
            protected List<PathFinderNode> mOpenList = new List<PathFinderNode>();

            // The closed list
            protected List<PathFinderNode> mClosedList = new List<PathFinderNode>();

            // A helper method to find the least cost node from a list
            protected PathFinderNode GetLeastCostNode(List<PathFinderNode> myList)
            {
                int best_index = 0;
                float best_priority = myList[0].Fcost;
                for (int i = 1; i < myList.Count; i++)
                {
                    if (best_priority > myList[i].Fcost)
                    {
                        best_priority = myList[i].Fcost;
                        best_index = i;
                    }
                }

                PathFinderNode n = myList[best_index];
                return n;
            }

            // A helper method to check if a value of T is in a list.
            // If it is then return the index of the item where the
            // value is. Otherwise return -1.
            protected int IsInList(List<PathFinderNode> myList, T cell)
            {
                for (int i = 0; i < myList.Count; ++i)
                {
                    if (EqualityComparer<T>.Default.Equals(myList[i].Location.Value, cell))
                        return i;
                }
                return -1;
            }

            #endregion

            #region Delegates for action callbacks.
            // Some callbacks to handle on changes to the internal values.
            // these callbacks can be used by the game to display visually the
            // changes to the cells and lists.
            public delegate void DelegatePathFinderNode(PathFinderNode node);
            public DelegatePathFinderNode onChangeCurrentNode;
            public DelegatePathFinderNode onAddToOpenList;
            public DelegatePathFinderNode onAddToClosedList;
            public DelegatePathFinderNode onDestinationFound;

            public delegate void DelegateNoArgument();
            public DelegateNoArgument onStarted;
            public DelegateNoArgument onRunning;
            public DelegateNoArgument onFailure;
            public DelegateNoArgument onSuccess;
            #endregion

            #region Actual path finding search functions
            // Stage 1. Initialize the serach.
            // Initialize a new search.
            // Note that a search can only be initialized if 
            // the path finder is not already running.
            public bool Initialize(Node<T> start, Node<T> goal)
            {
                if (Status == PathFinderStatus.RUNNING)
                {
                    // Path finding is already in progress.
                    return false;
                }

                // Reset the variables.
                Reset();

                // Set the start and the goal nodes for this search.
                Start = start;
                Goal = goal;

                // Calculate the H cost for the start.
                float H = HeuristicCost(Start.Value, Goal.Value);

                // Create a root node with its parent as null.
                PathFinderNode root = new PathFinderNode(Start, null, 0f, H);

                // add this root node to our open list.
                mOpenList.Add(root);

                // set the current node to root node.
                CurrentNode = root;

                // Invoke the deletages to inform the caller if the delegates are not null.
                onChangeCurrentNode?.Invoke(CurrentNode);
                onStarted?.Invoke();

                // set the status of the pathfinder to RUNNING.
                Status = PathFinderStatus.RUNNING;

                return true;
            }
            
            // Stage 2: Step until success or failure
            // Take a search step. The user must continue to call this method 
            // until the Status is either SUCCESS or FAILURE.
            public PathFinderStatus Step()
            {
                // Add the current node to the closed list.
                mClosedList.Add(CurrentNode);

                // Call the delegate to inform any subscribers.
                onAddToClosedList?.Invoke(CurrentNode);

                if (mOpenList.Count == 0)
                {
                    // we have exhausted our search. No solution is found.
                    Status = PathFinderStatus.FAILURE;
                    onFailure?.Invoke();
                    return Status;
                }

                // Get the least cost element from the open list. 
                // This becomes our new current node.
                CurrentNode = GetLeastCostNode(mOpenList);

                // Call the delegate to inform any subscribers.
                onChangeCurrentNode?.Invoke(CurrentNode);

                // Remove the node from the open list.
                mOpenList.Remove(CurrentNode);

                // Check if the node contains the Goal cell.
                if (EqualityComparer<T>.Default.Equals(
                    CurrentNode.Location.Value, Goal.Value))
                {
                    Status = PathFinderStatus.SUCCESS;
                    onDestinationFound?.Invoke(CurrentNode);
                    onSuccess?.Invoke();
                    return Status;
                }

                // Find the neighbours.
                List<Node<T>> neighbours = CurrentNode.Location.GetNeighbours();

                // Traverse each of these neighbours for possible expansion.
                foreach (Node<T> cell in neighbours)
                {
                    AlgorithmSpecificImplementation(cell);
                }

                Status = PathFinderStatus.RUNNING;
                onRunning?.Invoke();
                return Status;
            }

            abstract protected void AlgorithmSpecificImplementation(Node<T> cell);

            // Reset the internal variables for a new search.
            protected void Reset()
            {
                if (Status == PathFinderStatus.RUNNING)
                {
                    // Cannot reset path finder. Path finding in progress.
                    return;
                }

                CurrentNode = null;

                mOpenList.Clear();
                mClosedList.Clear();

                Status = PathFinderStatus.NOT_INITIALIZED;
            }

            #endregion
        }

        #region AstarPathFinder
        // The AstarPathFinder.
        public class AStarPathFinder<T> : PathFinder<T>
        {
            protected override void AlgorithmSpecificImplementation(Node<T> cell)
            {
                // first of all check if the node is already in the closedlist.
                // if so then we do not need to continue search for this node.
                if (IsInList(mClosedList, cell.Value) == -1)
                {
                    // The cell does not exist in the closed list.

                    // Calculate the cost of the node from its parent.
                    // Remember G is the cost from the start till now.
                    // So to get G we will get the G cost of the currentNode
                    // and add the cost from currentNode to this cell.
                    // We can actually implement a function to calculate the cost 
                    // between two adjacent cells. 

                    float G = CurrentNode.GCost + NodeTraversalCost(
                        CurrentNode.Location.Value, cell.Value);

                    float H = HeuristicCost(cell.Value, Goal.Value);

                    // Check if the cell is already there in the open list.
                    int idOList = IsInList(mOpenList, cell.Value);
                    if (idOList == -1)
                    {
                        // The cell does not exist in the open list.
                        // We will add the cell to the open list.

                        PathFinderNode n = new PathFinderNode(cell, CurrentNode, G, H);
                        mOpenList.Add(n);
                        onAddToOpenList?.Invoke(n);
                    }
                    else
                    {
                        // if the cell exists in the openlist then check if the G cost 
                        // is less than the one already in the list.
                        float oldG = mOpenList[idOList].GCost;
                        if (G < oldG)
                        {
                            // change the parent and update the cost to the new G
                            mOpenList[idOList].Parent = CurrentNode;
                            mOpenList[idOList].SetGCost(G);
                            onAddToOpenList?.Invoke(mOpenList[idOList]);
                        }
                    }
                }
            }
        }
        #endregion

        #region DijkstraPathFinder
        public class DijkstraPathFinder<T> : PathFinder<T>
        {
            protected override void AlgorithmSpecificImplementation(Node<T> cell)
            {
                if (IsInList(mClosedList, cell.Value) == -1)
                {
                    float G = CurrentNode.GCost + NodeTraversalCost(
                        CurrentNode.Location.Value, cell.Value);

                    //Dijkstra doesn't include the Heuristic cost
                    float H = 0.0f;

                    // Check if the cell is already there in the open list.
                    int idOList = IsInList(mOpenList, cell.Value);
                    if (idOList == -1)
                    {
                        // The cell does not exist in the open list.
                        // We will add the cell to the open list.

                        PathFinderNode n = new PathFinderNode(cell, CurrentNode, G, H);
                        mOpenList.Add(n);
                        onAddToOpenList?.Invoke(n);
                    }
                    else
                    {
                        // if the cell exists in the openlist then check if the G cost is less than the 
                        // one already in the list.
                        float oldG = mOpenList[idOList].GCost;
                        if (G < oldG)
                        {
                            // change the parent and update the cost to the new G
                            mOpenList[idOList].Parent = CurrentNode;
                            mOpenList[idOList].SetGCost(G);
                            onAddToOpenList?.Invoke(mOpenList[idOList]);
                        }
                    }
                }
            }
        }
        #endregion

        #region GreedyPathFinder
        public class GreedyPathFinder<T> : PathFinder<T>
        {
            protected override void AlgorithmSpecificImplementation(Node<T> cell)
            {
                if (IsInList(mClosedList, cell.Value) == -1)
                {
                    //Greedy best-first does doesn't include the G cost
                    float G = 0.0f;
                    float H = HeuristicCost(cell.Value, Goal.Value);

                    // Check if the cell is already there in the open list.
                    int idOList = IsInList(mOpenList, cell.Value);
                    if (idOList == -1)
                    {
                        // The cell does not exist in the open list.
                        // We will add the cell to the open list.

                        PathFinderNode n = new PathFinderNode(cell, CurrentNode, G, H);
                        mOpenList.Add(n);
                        onAddToOpenList?.Invoke(n);
                    }
                    else
                    {
                        // if the cell exists in the openlist then check if the G cost is less than the 
                        // one already in the list.
                        float oldG = mOpenList[idOList].GCost;
                        if (G < oldG)
                        {
                            // change the parent and update the cost to the new G
                            mOpenList[idOList].Parent = CurrentNode;
                            mOpenList[idOList].SetGCost(G);
                            onAddToOpenList?.Invoke(mOpenList[idOList]);
                        }
                    }
                }
            }
        }
        #endregion
    }
}