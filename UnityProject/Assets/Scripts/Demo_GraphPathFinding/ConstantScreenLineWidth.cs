using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ConstantScreenLineWidth : MonoBehaviour
{
  public float mOriginalCameraSize = 20.0f;
  public float mLineWidth = 0.1f;

  LineRenderer mLineRenderer;
  private void Start()
  {
    mLineRenderer = GetComponent<LineRenderer>();
  }
  void LateUpdate()
  {
    float factor = Camera.main.orthographicSize / mOriginalCameraSize;// * mLineWidth;
    LineRenderer lr = mLineRenderer;
    lr.widthMultiplier = factor;
  }
}
