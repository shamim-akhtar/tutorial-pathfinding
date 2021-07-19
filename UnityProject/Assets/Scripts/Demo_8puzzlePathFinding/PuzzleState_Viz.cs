using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Puzzle;

public class PuzzleState_Viz : MonoBehaviour
{
    [Tooltip("Associate the tiles into this array")]
    public GameObject[] mTiles;

    [Tooltip("Associate the location transforms into this array")]
    public Transform[] mTileLocations;

    void Start()
    {
        // Create a new PuzzleState and set it.
        PuzzleState state = new PuzzleState(3);
        SetPuzzleState(state);
    }

    public void SetPuzzleState(PuzzleState state)
    {
        for(int i = 0; i < state.Arr.Length; ++i)
        {
            mTiles[state.Arr[i]].transform.position = mTileLocations[i].position;
        }
    }

    public IEnumerator Coroutine_MoveOverSeconds(
        GameObject objectToMove,
        Vector3 end, 
        float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.position = Vector3.Lerp(
                startingPos, end, 
                (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
        objectToMove.transform.position = end;
    }


    public void SetPuzzleState(PuzzleState state, float duration)
    {
        for (int i = 0; i < state.Arr.Length; ++i)
        {
            StartCoroutine(Coroutine_MoveOverSeconds(
                mTiles[state.Arr[i]], 
                mTileLocations[i].position, 
                duration));
        }
    }
}
