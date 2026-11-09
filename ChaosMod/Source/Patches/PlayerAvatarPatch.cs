using HarmonyLib;
using Lego.ChaosMod.Backend;
using Photon.Pun;

namespace Lego.ChaosMod.Patches;
[HarmonyPatch(typeof(PlayerAvatar))]
internal sealed class PlayerAvatarPatch {
    [HarmonyPatch("ReviveRPC"), HarmonyPostfix]
    static void Revive_Postfix(PlayerAvatar __instance, bool _revivedByTruck, PhotonMessageInfo _info = default)
    {
        if (ChaosController.instance != null) {
            if (!ChaosController.instance.spawnedPlayers.Contains(__instance)) {
                ChaosController.instance.spawnedPlayers.Add(__instance);
                ChaosController.instance.pendingPlayerSpawned = true;
            }
        }
    }

    [HarmonyPatch("PlayerDeathRPC"), HarmonyPostfix]
    static void Death_Postfix(PlayerAvatar __instance, int enemyIndex, PhotonMessageInfo _info = default)
    {
        if (ChaosController.instance != null) {
            if (!ChaosController.instance.deadPlayers.Contains(__instance)) {
                ChaosController.instance.deadPlayers.Add(__instance);
                ChaosController.instance.pendingPlayerDeath = true;
            }
        }
    }
}