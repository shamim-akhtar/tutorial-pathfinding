using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNodeView : MonoBehaviour
{
  // We two sprites for visualisation 
  // of this gridnode.
  [SerializeField]
  SpriteRenderer innerSprite;
  [SerializeField]
  SpriteRenderer outerSprite;

  // The property to access the GridNode.
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
