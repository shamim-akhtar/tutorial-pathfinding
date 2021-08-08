using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.PathFinding;

public class RectGridCell_Viz : MonoBehaviour
{
  [SerializeField]
  SpriteRenderer InnerSprite;
  [SerializeField]
  SpriteRenderer OuterSprite;

  public RectGridCell RectGridCell;
  //public TextMesh FCostText;
  //public TextMesh HCostText;
  //public TextMesh GCostText;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }

  public void SetInnerColor(Color col)
  {
    InnerSprite.color = col;
  }

  public void SetOuterColor(Color col)
  {
    OuterSprite.color = col;
  }

  //public void SetFCost(float cost)
  //{
  //    FCostText.text = cost.ToString("F2");
  //}

  //public void SetHCost(float cost)
  //{
  //    HCostText.text = cost.ToString("F0");
  //}

  //public void SetGCost(float cost)
  //{
  //    GCostText.text = cost.ToString("F2");
  //}

  //public void ClearTexts()
  //{
  //    GCostText.text = "";
  //    HCostText.text = "";
  //    FCostText.text = "";
  //}

  //public void SetFCostColor(Color color)
  //{
  //    FCostText.color = color;
  //}

  //public void SetGCostColor(Color color)
  //{
  //    GCostText.color = color;
  //}

  //public void SetHCostColor(Color color)
  //{
  //    HCostText.color = color;
  //}
}
