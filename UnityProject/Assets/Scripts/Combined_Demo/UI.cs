using Lean.Gui;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
  public Text TitleText;
  public LeanSwitch mSwitchAlgo;
  public Text mAlgoText;
  public LeanToggle mToggleCostFunction;
  public Text mCostFunctionText;
  enum CostFunctionType
  {
    MANHATTAN,
    EUCLIDEN,
  }
  CostFunctionType mCostFunctionType = CostFunctionType.MANHATTAN;

  public LeanToggle mToggleMode;
  public Text mToggleModeText;
  enum ModeType
  {
    EDITOR,
    PLAYER,
  }
  ModeType mModeType = ModeType.PLAYER;

  public LeanToggle mToggleInteractive;
  public Text mToggleInteractiveText;
  enum InteractiveType
  {
    INTERACTIVE,
    NON_INTERACTIVE,
  }
  InteractiveType mInteractiveType = InteractiveType.NON_INTERACTIVE;

  public Button mBtnReset;
  public Button mBtnPlay;
  public Button mBtnNext;
  public Button mBtn8Puzzle;
  public Button mBtnGrid;
  public Button mBtnGraph;
  public Button mBtnRandomizeNPC;

  public Text mTextFCost;
  public Text mTextGCost;
  public Text mTextHCost;
  public GameObject mNotification;
  public Text mTextNotification;

  private int mPathFindingAlgo = 0; // Astar, 1 = Djikstra and 2 = Greedy best-first

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }

  public void SetTitle(string title)
  {
    TitleText.text = title;
  }

  public void SetToggleCostFunction()
  {
    //if (mPathFinder_Viz.mPathFinder == null)
    //  return;
    //if (mPathFinder_Viz.mPathFinder != null && mPathFinder_Viz.mPathFinder.Status == PathFinderStatus.RUNNING)
    //{
    //  // disable selection when running.
    //  if (mToggleCostFunction.On)
    //    mToggleCostFunction.TurnOff();
    //  else
    //    mToggleCostFunction.TurnOn();
    //  return;
    //}

    if (mCostFunctionType == CostFunctionType.MANHATTAN)
    {
      mCostFunctionType = CostFunctionType.EUCLIDEN;
      mCostFunctionText.text = "Euclidean Cost";
    }
    else
    {
      mCostFunctionType = CostFunctionType.MANHATTAN;
      mCostFunctionText.text = "Manhattan Cost";
    }
    //SetCostFunction(mCostFunctionType);
  }

  public void SetToggleMode()
  {
    if (mModeType == ModeType.EDITOR)
    {
      mModeType = ModeType.PLAYER;
      mToggleModeText.text = "Play Mode";
    }
    else
    {
      mModeType = ModeType.EDITOR;
      mToggleModeText.text = "Edit Mode";
    }
  }


  public void SetToggleInteractive()
  {
    if (mInteractiveType == InteractiveType.NON_INTERACTIVE)
    {
      mInteractiveType = InteractiveType.INTERACTIVE;
      mToggleInteractiveText.text = "Interactive";
      mBtnPlay.gameObject.SetActive(true);
      mBtnNext.gameObject.SetActive(true);
    }
    else
    {
      mInteractiveType = InteractiveType.NON_INTERACTIVE;
      mToggleInteractiveText.text = "Non Interactive";
      mBtnPlay.gameObject.SetActive(false);
      mBtnNext.gameObject.SetActive(false);
    }
  }

  public void OnClickBtn_Reset()
  {
    mTextFCost.text = "";
    mTextGCost.text = "";
    mTextHCost.text = "";

  }

  public void OnClickBtn_Play()
  {

  }

  public void OnClickBtn_Next()
  {

  }

  public void OnClickBtn_8Puzzle()
  {

  }

  public void OnClickBtn_Grid()
  {

  }

  public void OnClickBtn_Graph()
  {

  }

  public void OnClickBtn_RandomizeNPC()
  {

  }

  public void OnSelectAlgorithm()
  {
    mPathFindingAlgo = mSwitchAlgo.State;
    if (mPathFindingAlgo == 0)
    {
      mAlgoText.text = "Astar";
      mAlgoText.alignment = TextAnchor.MiddleLeft;
    }
    if (mPathFindingAlgo == 1)
    {
      mAlgoText.text = "Dijkstra";
      mAlgoText.alignment = TextAnchor.MiddleCenter;
    }
    if (mPathFindingAlgo == 2)
    {
      mAlgoText.text = "Greedy Best-First";
      mAlgoText.alignment = TextAnchor.MiddleRight;
    }
    //mPathFinder_Viz.SetPathFindingAlgorithm((PathFindingAlgorithm)mPathFindingAlgo);
    //SetCostFunction(mCostFunctionType);
    //mPathFinder_Viz.mPathFinder.onFailure += OnPathFindingCompleted;
    //mPathFinder_Viz.mPathFinder.onSuccess += OnPathFindingCompleted;
    //mPathFinder_Viz.mPathFinder.onStarted += OnPathFindingStarted;
    //mPathFinder_Viz.mPathFinder.onChangeCurrentNode += OnChangeCurrentNode;
  }

  public void SetFCost(float cost)
  {
      mTextFCost.text = cost.ToString("F2");
  }

  public void SetHCost(float cost)
  {
    mTextHCost.text = cost.ToString("F2");
  }

  public void SetGCost(float cost)
  {
    mTextGCost.text = cost.ToString("F2");
  }

  public void ShowNotification(string text, float duration = 5.0f)
  {
    mNotification.SetActive(true);
    mTextNotification.text = text;
    StartCoroutine(Coroutine_ActiveDuration(mNotification, duration));
  }

  IEnumerator Coroutine_ActiveDuration(GameObject obj, float duration)
  {
    yield return new WaitForSeconds(duration);
    obj.SetActive(false);
  }
}
