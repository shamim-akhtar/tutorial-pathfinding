using Lean.Gui;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Graph : MonoBehaviour
{
  public RandomGraph mDemo;

  public Text TitleText;
  public LeanSwitch mSwitchAlgo;
  public Text mAlgoText;
  public LeanToggle mToggleCostFunction;
  public Text mCostFunctionText;
  public enum CostFunctionType
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

  //public Text mTextFCost;
  //public Text mTextGCost;
  //public Text mTextHCost;
  public GameObject mNotification;
  public Text mTextNotification;

  private int mPathFindingAlgo = 0; // Astar, 1 = Djikstra and 2 = Greedy best-first

  // Start is called before the first frame update
  void Start()
  {
    SetTitle("Pathdinding Playground - Graphs");
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
    if (mDemo.IsAnyPathFinderRunning())
    {
      ShowNotification("Pathfinder running. Cannot change cost function.\nif you are using interactive mode then click the Play button to force complete pathfinding.");
      // disable selection when running.
      if (mToggleCostFunction.On)
        mToggleCostFunction.TurnOff();
      else
        mToggleCostFunction.TurnOn();
      return;
    }
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
    if(mDemo.IsAnyPathFinderRunning())
    {
      ShowNotification("Pathfining in progress. Cannot toggle interactive mode.\nif you are using interactive mode then click the Play button to force complete pathfinding.");
      // disable selection when running.
      if (mToggleInteractive.On)
        mToggleInteractive.TurnOff();
      else
        mToggleInteractive.TurnOn();
      return;
    }

    if (mInteractiveType == InteractiveType.NON_INTERACTIVE)
    {
      mInteractiveType = InteractiveType.INTERACTIVE;
      mToggleInteractiveText.text = "Interactive";
      mBtnPlay.gameObject.SetActive(true);
      mBtnNext.gameObject.SetActive(true);

      mDemo.SetInteractive(true);
    }
    else
    {
      mInteractiveType = InteractiveType.NON_INTERACTIVE;
      mToggleInteractiveText.text = "Non Interactive";
      mBtnPlay.gameObject.SetActive(false);
      mBtnNext.gameObject.SetActive(false);
      mDemo.SetInteractive(false);
    }
  }

  public void OnClickBtn_Reset()
  {
    if (mDemo.IsAnyPathFinderRunning())
    {
      ShowNotification("Cannot reset. Pathfining in progress.\nif you are using interactive mode then click the Play button to force complete pathfinding.");
      return;
    }
    mDemo.ResetLastDestination();
  }

  public void OnClickBtn_Play()
  {
    mDemo.PathFindingStepForcComplete();
  }

  public void OnClickBtn_Next()
  {
    mDemo.PathFindingStep();
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
    mDemo.RandomizeNPCs();
  }

  public void OnClickBtn_RandomizeGraph()
  {
    mDemo.OnClickRegenerate();
  }

  public void OnSelectAlgorithm()
  {
    if(mDemo.IsAnyPathFinderRunning())
    {
      mSwitchAlgo.State = (int)mPathFindingAlgo;
      ShowNotification("Pathfinder running. Cannot change algorithm.\nif you are using interactive mode then click the Play button to force complete pathfinding.");
      return;
    }

    if(!mDemo.mInteractive)
    {
      mSwitchAlgo.State = (int)mPathFindingAlgo;
      ShowNotification("Toggle to interactive mode to switch pathfinder algorithm. Non interactive mode uses the AStar pathfinder using threads.");
      return;
    }
    mPathFindingAlgo = mSwitchAlgo.State;
    if (mPathFindingAlgo == 0)
    {
      mAlgoText.text = "Astar";
      mAlgoText.alignment = TextAnchor.MiddleLeft;
      mDemo.mPathFinderType = GameAI.PathFinding.PathFinderTypes.ASTAR;
    }
    if (mPathFindingAlgo == 1)
    {
      mAlgoText.text = "Dijkstra";
      mAlgoText.alignment = TextAnchor.MiddleCenter;
      mDemo.mPathFinderType = GameAI.PathFinding.PathFinderTypes.DJIKSTRA;
    }
    if (mPathFindingAlgo == 2)
    {
      mAlgoText.text = "Greedy Best-First";
      mAlgoText.alignment = TextAnchor.MiddleRight;
      mDemo.mPathFinderType = GameAI.PathFinding.PathFinderTypes.GREEDY_BEST_FIRST;
    }
  }

  //public void SetFCost(float cost)
  //{
  //    mTextFCost.text = cost.ToString("F2");
  //}

  //public void SetHCost(float cost)
  //{
  //  mTextHCost.text = cost.ToString("F2");
  //}

  //public void SetGCost(float cost)
  //{
  //  mTextGCost.text = cost.ToString("F2");
  //}

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
