using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Puzzle;
using GameAI.PathFinding;

public class PuzzleSolver : MonoBehaviour
{
    public PuzzleState_Viz mPuzzleStateViz;

    private PuzzleNode mCurrentState;
    private PuzzleNode mGoalState;

    private AStarPathFinder<PuzzleState> mAstarSolver = new AStarPathFinder<PuzzleState>();
    private PuzzleMap mPuzzle = new PuzzleMap(3);

    // Start is called before the first frame update
    void Start()
    {
        mCurrentState = new PuzzleNode(mPuzzle, new PuzzleState(3));
        mGoalState = new PuzzleNode(mPuzzle, new PuzzleState(3));

        //mAstarSolver.onChangeCurrentNode = OnChangeCurrentNode;
        mAstarSolver.NodeTraversalCost = PuzzleMap.GetCostBetweenTwoCells;
        mAstarSolver.HeuristicCost = PuzzleMap.GetManhattanCost;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //mCurrentState.RandomizeSolvable();
            mPuzzleStateViz.SetPuzzleState(mCurrentState.Value);
            mAstarSolver.Initialize(mCurrentState, mGoalState);

            Solve();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (mAstarSolver.Status == PathFinderStatus.RUNNING)
                mAstarSolver.Step();
            if(mAstarSolver.Status == PathFinderStatus.SUCCESS)
            {
                Debug.Log("Found solution. Displaying solution now");
                StartCoroutine(ShowSolution());
            }
            if (mAstarSolver.Status == PathFinderStatus.FAILURE)
            {
                Debug.Log("Failure");
            }
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            Randomize();
        }
    }

    IEnumerator Coroutine_Solve()
    {
        while (mAstarSolver.Status == PathFinderStatus.RUNNING)
        {
            mAstarSolver.Step();
            yield return null;
        }
        if (mAstarSolver.Status == PathFinderStatus.SUCCESS)
        {
            Debug.Log("Found solution. Displaying solution now");
            StartCoroutine(ShowSolution());
        }
        if (mAstarSolver.Status == PathFinderStatus.FAILURE)
        {
            Debug.Log("Failure");
        }
    }

    IEnumerator Coroutine_Randomize(int depth)
    {
        int i = 0;
        while (i < depth)
        {
            List<Node<PuzzleState>> neighbours = mPuzzle.GetNeighbours(mCurrentState);

            // get a random neignbour.
            int rn = Random.Range(0, neighbours.Count);
            mCurrentState.Value.SwapWithEmpty(neighbours[rn].Value.GetEmptyTileIndex());
            i++;
            mPuzzleStateViz.SetPuzzleState(mCurrentState.Value);
            yield return null;
        }
    }

    public void Randomize(int depth = 50)
    {
        StartCoroutine(Coroutine_Randomize(depth));
    }

    public void Solve()
    {
        StartCoroutine(Coroutine_Solve());
    }

    //void OnChangeCurrentNode(PathFinderNode<PuzzleState> node)
    //{
    //    mPuzzleStateViz.SetPuzzleState(node.Location);
    //}

    IEnumerator ShowSolution()
    {
        List<PuzzleState> reverseSolution = new List<PuzzleState>();
        PathFinder<PuzzleState>.PathFinderNode node = mAstarSolver.CurrentNode;
        while(node != null)
        {
            reverseSolution.Add(node.Location.Value);
            node = node.Parent;
        }

        if (reverseSolution.Count > 0)
        {
            mPuzzleStateViz.SetPuzzleState(reverseSolution[reverseSolution.Count - 1]);

            if (reverseSolution.Count > 2)
            {
                for (int i = reverseSolution.Count - 2; i >= 0; i -= 1)
                {
                    mPuzzleStateViz.SetPuzzleState(reverseSolution[i], 0.5f);
                    yield return new WaitForSeconds(1.0f);
                }
            }
        }
    }
}
