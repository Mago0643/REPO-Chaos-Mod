using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;

public class SpinEternallyEvent: ChaosEvent
{
    public SpinEternallyEvent(): base("evt_spin", "evt_spin_desc")
    {
        minTimer = 20f;
        maxTimer = 40f;
        ID = 17;

        // TODO: finish this
        onEnemyRespawned += (EnemyParent e) => {
            e.Enemy.Stunned = true;
        };
    }

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();

        // bayblades
        foreach (var enemy in ChaosMod.Instance.spawnedEnemys)
        {
            if (enemy == null) continue;

            var rb = enemy.GetComponent<EnemyRigidbody>();
            if (rb == null) continue;

            var realRB = rb.GetComponent<Rigidbody>();
            if (realRB.angularVelocity.magnitude <= 1500f)
                realRB.AddTorque(rb.transform.forward * 500f, ForceMode.Impulse);
        }
    }
}