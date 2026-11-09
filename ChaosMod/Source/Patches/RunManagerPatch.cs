using HarmonyLib;
using Lego.ChaosMod.Backend;
using System.Collections;

namespace Lego.ChaosMod.Patches;
[HarmonyPatch(typeof(RunManager))]
internal class RunManagerPatch
{
    [HarmonyPatch("ChangeLevel"), HarmonyPostfix]
    static void ChangeLevel_Postfix(bool _completedLevel, bool _levelFailed, RunManager.ChangeLevelType _changeLevelType = RunManager.ChangeLevelType.Normal)
    {
        ChaosMod.Instance?.OnLevelChange?.Invoke(_changeLevelType);
    }

    [HarmonyPatch("LeaveToMainMenu"), HarmonyPostfix]
    static void LeaveToMainMenu_Postfix()
    {
        if (RunManager.instance == null || RunManager.instance.levelCurrent == null || RunManager.instance.levelCurrent != RunManager.instance.levelMainMenu) return;
        ChaosMod.Instance?.OnLevelChange?.Invoke(RunManager.ChangeLevelType.MainMenu);
    }
}