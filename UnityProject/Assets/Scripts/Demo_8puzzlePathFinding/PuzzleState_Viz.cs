using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Puzzle;

public class PuzzleState_Viz : MonoBehaviour
{
    public GameObject[] mTiles;
    public Transform[] mTileLocations;
    // Start is called before the first frame update
    void Start()
    {
        //SetPuzzleState(PuzzleState.RandomSolvablePuzzle(3));
        PuzzleState state = new PuzzleState(3);
        //state.SwapWithEmpty(7);
        //state.RandomizeSolvable();
        SetPuzzleState(state);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPuzzleState(PuzzleState state)
    {
        for(int i = 0; i < state.Arr.Length; ++i)
        {
            mTiles[state.Arr[i]].transform.position = mTileLocations[i].position;
        }
    }

    public IEnumerator Coroutine_MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
        objectToMove.transform.position = end;
    }


    public void SetPuzzleState(PuzzleState state, float duration)
    {
        for (int i = 0; i < state.Arr.Length; ++i)
        {
            StartCoroutine(Coroutine_MoveOverSeconds(mTiles[state.Arr[i]], mTileLocations[i].position, duration));
            //mTiles[state.Arr[i]].transform.position = mTileLocations[i].position;
            //yield return null;
        }
    }
}
