using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CameraMovement2D : MonoBehaviour
{
  public float CameraSizeMin = 1.0f;
  public static bool CameraPanning { get; set; } = true;
  public Slider mSliderZoom;

  private Vector3 mDragPos;
  private Vector3 mOriginalPosition;

  private float mCameraSizeMax;
  private float mZoomFactor = 0.0f;
  private Camera mCamera;
  private bool mDragging = false;

  void Start()
  {
    SetCamera(Camera.main);
  }

  public void SetCamera(Camera camera)
  {
    mCamera = camera;
    mCameraSizeMax = mCamera.orthographicSize;
    mOriginalPosition = mCamera.transform.position;
  }

  public void RePositionCamera(Rect extent)
  {
    Camera.main.orthographicSize = extent.height / 1.25f;
    Vector3 center = extent.center;
    center.z = -100.0f;
    Camera.main.transform.position = center;

    mCameraSizeMax = mCamera.orthographicSize;
    mOriginalPosition = mCamera.transform.position;
  }

  public void RePositionCamera(float width, float height)
  {
    Rect extent = new Rect(0.0f, 0.0f, width, height);
    RePositionCamera(extent);
  }

  void Update()
  {
    // Camera panning is disabled when a tile is selected.
    if (!CameraPanning)
    {
      mDragging = false;
      return;
    }

    // We also check if the pointer is not on UI item
    // or is disabled.
    if (EventSystem.current.IsPointerOverGameObject() || enabled == false)
    {
      //mDragging = false;
      return;
    }

    // Save the position in worldspace.
    if (Input.GetMouseButtonDown(0))
    {
      mDragPos = mCamera.ScreenToWorldPoint(Input.mousePosition);
      mDragging = true;
    }

    if (Input.GetMouseButton(0) && mDragging)
    {
      Vector3 diff = mDragPos - mCamera.ScreenToWorldPoint(Input.mousePosition);
      diff.z = 0.0f;
      mCamera.transform.position += diff;
    }
    if (Input.GetMouseButtonUp(0))
    {
      mDragging = false;
    }
  }

  public void ResetCameraView()
  {
    mCamera.transform.position = mOriginalPosition;
    mCamera.orthographicSize = mCameraSizeMax;
    mZoomFactor = 0.0f;
    if (mSliderZoom)
    {
      mSliderZoom.value = 0.0f;
    }
  }

  public void OnSliderValueChanged()
  {
    Zoom(mSliderZoom.value);
  }

  public void Zoom(float value)
  {
    mZoomFactor = value;
    mZoomFactor = Mathf.Clamp01(mZoomFactor);

    mCamera.orthographicSize = mCameraSizeMax -
        mZoomFactor * (mCameraSizeMax - CameraSizeMin);
  }

  public void ZoomIn()
  {
    Zoom(mZoomFactor + 0.01f);
  }

  public void ZoomOut()
  {
    Zoom(mZoomFactor - 0.01f);
  }
}
