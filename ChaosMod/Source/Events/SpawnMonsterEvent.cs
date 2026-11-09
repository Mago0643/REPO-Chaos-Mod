using Lego.ChaosMod.Backend;
using REPOLib.Modules;
using System.Collections.Generic;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class SpawnMonsterEvent: ChaosEvent
{
    internal static int selectedIdx = 0;
    internal static int[] counts = [];
    public SpawnMonsterEvent(int[] counts): base("evt_spawn_monsters", "evt_spawn_monsters_desc")
    {
        SpawnMonsterEvent.counts = counts;
        isOnce = true;
        ID = 1;
    }

    public override string GetName()
        => Language.GetText(nameKey).Replace("%f", counts[selectedIdx].ToString());

    public override void Start()
    {
        base.Start();
        selectedIdx = Random.Range(0, counts.Length);
        if (ChaosMod.IsDebug)
            Logging.Log("Spawn " + counts[selectedIdx] + " enemys");
        if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

        var points = Object.FindObjectsByType<LevelPoint>(0);
        foreach (var mod in ChaosEventManager.Events)
        {
            if (mod is SpawnMonsterEvent && !ChaosEventManager.Excludes.Contains(mod.Instance))
                ChaosEventManager.Excludes.Add(mod.Instance);
        }

        int choosen = 0;
        List<EnemySetup> enemies = [
            ..EnemyDirector.instance.enemiesDifficulty1,
            ..EnemyDirector.instance.enemiesDifficulty2,
            ..EnemyDirector.instance.enemiesDifficulty3
        ];

        foreach (EnemySetup monster in enemies)
        {
            if (choosen >= counts[selectedIdx]) break;
            if (Random.Range(0f, 1f) <= 0.6f) continue;

            var parents = Enemies.SpawnEnemy(monster, points[Random.Range(0, points.Length)].transform.position, Quaternion.identity);
            foreach (EnemyParent p in parents!) {
                if (!ChaosMod.Instance.spawnedEnemys.Contains(p))
                    ChaosMod.Instance.spawnedEnemys.Add(p);
            }
            Logging.Log($"Enemy spawned: {monster.name}");

            choosen += 1;
        }
    }
}