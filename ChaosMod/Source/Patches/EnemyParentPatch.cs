using HarmonyLib;
using Lego.ChaosMod.Backend;

namespace Lego.ChaosMod.Patches;
[HarmonyPatch(typeof(EnemyParent))]
internal sealed class EnemyParentPatch
{
    [HarmonyPatch("DespawnRPC"), HarmonyPrefix]
    static void Despawn_Prefix(EnemyParent __instance)
    {
        if (ChaosMod.Instance != null && ChaosMod.Instance.spawnedEnemys.Contains(__instance)) {
            ChaosMod.Instance.spawnedEnemys.Remove(__instance);
            if (ChaosMod.IsDebug)
                Logging.Log("Enemy Despawned");
        }

        if (ChaosController.instance != null) {
            Enemy e = __instance.Enemy;
            
            bool hasHealth = e.HasHealth;
            EnemyHealth eHealth = e.Health;
            int curHealth = eHealth.healthCurrent;

            if (ChaosMod.IsDebug) {
                Logging.Log($"Enemy Has Health: {hasHealth}, Health: {curHealth}");
            }

            if (!hasHealth || curHealth >= 0) {
                ChaosController.instance.deadEnemies.Add(__instance);
                ChaosController.instance.pendingEnemyDeath = true;
                return;
            }

            // just despawned
            ChaosController.instance.despawnedEnemies.Add(__instance);
            ChaosController.instance.pendingEnemyDespawned = true;
        }
    }

    [HarmonyPatch("SpawnRPC"), HarmonyPostfix]
    static void Spawn_Postfix(EnemyParent __instance)
    {
        if (ChaosMod.Instance != null && !ChaosMod.Instance.spawnedEnemys.Contains(__instance)) {
            ChaosMod.Instance.spawnedEnemys.Add(__instance);
            if (ChaosMod.IsDebug)
                Logging.Log("Enemy Spawned");
        }

        if (ChaosController.instance != null) {
            ChaosController.instance.respawnedEnemies.Add(__instance);
            ChaosController.instance.pendingEnemyRespawned = true;
        }
    }
}