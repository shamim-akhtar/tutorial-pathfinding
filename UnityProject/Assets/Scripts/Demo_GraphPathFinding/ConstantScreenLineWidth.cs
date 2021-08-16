using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ConstantScreenLineWidth : MonoBehaviour
{
  public float mOriginalCameraSize = 10.0f;

  LineRenderer mLineRenderer;
  private void Start()
  {
    mLineRenderer = GetComponent<LineRenderer>();
  }

  void LateUpdate()
  {
    float factor = Camera.main.orthographicSize / mOriginalCameraSize;

    LineRenderer lr = mLineRenderer;
    lr.widthMultiplier = factor;
  }
}
