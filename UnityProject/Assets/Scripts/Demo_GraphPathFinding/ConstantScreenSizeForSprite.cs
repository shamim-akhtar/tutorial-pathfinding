using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantScreenSizeForSprite : MonoBehaviour
{
  public float mOriginalCameraSize = 10.0f;
  public Vector3 OrigScale = Vector3.one;

  void LateUpdate()
  {
    if (Camera.main.orthographicSize > 0.1f)
    {
      transform.localScale = 
        Camera.main.orthographicSize / 
        mOriginalCameraSize * OrigScale;
    }
  }
}
