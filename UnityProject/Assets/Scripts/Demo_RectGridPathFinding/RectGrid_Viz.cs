using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectGrid_Viz : MonoBehaviour
{
  // the max number of columns in the grid.
  public int mX;
  // the max number of rows in the grid
  public int mY;

  // The prefab for representing a grid cell. We will 
  // use the prefab to show/visualize the status of the cell
  // as we proceed with our pathfinding.
  [SerializeField]
  GameObject RectGridCell_Prefab;

  GameObject[,] mRectGridCellGameObjects;

  // the 2d array of Vecto2Int.
  // This stucture stores the 2d indices of the grid cells.
  protected Vector2Int[,] mIndices;

  // Construct a grid with the max cols and rows.
  protected void Construct(int numX, int numY)
  {
    mX = numX;
    mY = numY;

    mIndices = new Vector2Int[mX, mY];
    mRectGridCellGameObjects = new GameObject[mX, mY];

    // create all the grid cells (Index data) with default values.
    // also create the grid cell game ibjects from the prefab.
    for (int i = 0; i < mX; ++i)
    {
      for (int j = 0; j < mY; ++j)
      {
        mIndices[i, j] = new Vector2Int(i, j);
        mRectGridCellGameObjects[i, j] = Instantiate(
          RectGridCell_Prefab,
          new Vector3(i, j, 0.0f),
          Quaternion.identity);

        // Set the parent for the grid cell to this transform.
        mRectGridCellGameObjects[i, j].transform.SetParent(transform);

        // set a name to the instantiated cell.
        mRectGridCellGameObjects[i, j].name = "cell_" + i + "_" + j;
      }
    }
  }

  void ResetCamera()
  {
    Camera.main.orthographicSize = mY / 2.0f + 1.0f;
    Camera.main.transform.position = new Vector3(mX / 2.0f - 0.5f, mY / 2.0f - 0.5f, -100.0f);
  }

  private void Start()
  {
    // Constryct the grid and the cell game objects.
    Construct(mX, mY);

    // Reset the camera to a proper size and position.
    ResetCamera();
  }
}
