using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChaosMod
{
    [HarmonyPatch(typeof(EnemyParent))]
    internal class EnemyParentPatch
    {
        [HarmonyPatch("DespawnRPC"), HarmonyPostfix]
        static void Despawn_Postfix(EnemyParent __instance)
        {
            if (ChaosMod.Instance != null && ChaosMod.Instance.spawnedEnemys.Contains(__instance)) {
                ChaosMod.Instance.spawnedEnemys.Remove(__instance);
                if (ChaosMod.IsDebug)
                    ChaosMod.Logger.LogInfo("Enemy Despawned");
            }
        }

        [HarmonyPatch("SpawnRPC"), HarmonyPostfix]
        static void Spawn_Postfix(EnemyParent __instance)
        {
            if (ChaosMod.Instance != null && !ChaosMod.Instance.spawnedEnemys.Contains(__instance)) {
                ChaosMod.Instance.spawnedEnemys.Add(__instance);
                if (ChaosMod.IsDebug)
                    ChaosMod.Logger.LogInfo("Enemy Spawned");
            }
        }

        
    }
}
