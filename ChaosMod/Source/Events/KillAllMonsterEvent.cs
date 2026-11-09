using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class KillAllMonsterEvent: ChaosEvent
{
    public KillAllMonsterEvent(): base("evt_kill_all_monsters", "evt_kill_all_monsters_desc")
    {
        isOnce = true;
        ID = 4;
    }

    public override void Start()
    {
        base.Start();
        foreach (var enemy in Object.FindObjectsByType<EnemyHealth>(0))
        {
            enemy.Hurt(0x7FFFFFFF, Vector3.zero);
        }
    }
}