using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNodeView : MonoBehaviour
{
  [SerializeField]
  SpriteRenderer innerSprite;
  [SerializeField]
  SpriteRenderer outerSprite;

  public GridNode Node { get; set; }

  public void SetInnerColor(Color col)
  {
    innerSprite.color = col;
  }
  public void SetOuterColor(Color col)
  {
    outerSprite.color = col;
  }
}
