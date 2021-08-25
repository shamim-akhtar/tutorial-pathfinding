using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IPathfindingUI
{
  string GetTitle();
  bool IsAnyPathFinderRunning();
  void SetInteractive(bool flag);
  void ResetLastDestination();
  void PathFindingStepForceComplete();
  void PathFindingStep();
  void RandomizeNPCs();
  void RegenerateMap();
  void SetPathFinderType(GameAI.PathFinding.PathFinderTypes type);
  void SetCostFunction(GameAI.PathFinding.CostFunctionType cf);
  void SetFCostText(Text textField);
  void SetHCostText(Text textField);
  void SetGCostText(Text textField);
  void SetNotificationText(Text textField);

  bool IsInteractive();
}
