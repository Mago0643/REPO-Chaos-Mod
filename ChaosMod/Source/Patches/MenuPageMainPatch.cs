using HarmonyLib;
using Lego.ChaosMod.Backend;

namespace Lego.ChaosMod.Patches;
[HarmonyPatch(typeof(MenuPageMain))]
internal sealed class MenuPageMainPatch
{
    [HarmonyPatch("Start"), HarmonyPostfix]
    static void Start_Postfix()
    {
        if (MenuManager.instance == null || MenuManager.instance.menuPages == null || MenuManager.instance.menuPages.Count == 0) {
            Logging.Error("MenuManager's instance is null! Cannot add config button.");
            return;
        }

        ChaosConfigManager.Register();
    }
}