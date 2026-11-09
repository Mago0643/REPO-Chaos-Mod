using Lego.ChaosMod.Backend;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class DoomsdayEvent: ChaosEvent
{
    public DoomsdayEvent(): base("evt_doomsday", "evt_doomsday_desc")
    {
        minTimer = 30f;
        maxTimer = 60f;
    }

    private static List<PhysGrabObject> items = new();

    public override void Start()
    {
        base.Start();

        items = Object.FindObjectsByType<PhysGrabObject>(0).ToList();
        // do not make cart and tumbled players go crazy move because it makes the game unplayable
        items.RemoveAll(x => x.transform.name.Contains("Cart") || x.transform.name.Contains("Tumble"));
        // ChaosMod.Instance.spawnedEnemys = GameObject.FindObjectsByType<EnemyParent>(0).ToHashSet();
    }

    public override void Update()
    {
        if (SemiFunc.IsMasterClientOrSingleplayer()) {
            foreach (var enemy in ChaosMod.Instance.spawnedEnemys)
            {
                var rb = enemy.GetComponent<EnemyRigidbody>();
                if (rb == null) continue;
                rb.enemy.GetComponent<EnemyStateStunned>().Set(timerSelf + 5f);

                var realRB = rb.GetComponent<Rigidbody>();
                if (realRB.linearVelocity.magnitude <= 50f)
                    realRB.AddForce(realRB.transform.up * 10f, ForceMode.Impulse);
                if (realRB.angularVelocity.magnitude <= 25f)
                    realRB.AddTorque(realRB.transform.forward * Random.Range(0.5f, 2f) * 10f, ForceMode.Impulse);
            }


            foreach (var valuable in items)
            {
                if (valuable == null) continue;
                // TODO: Do not affect if items are in the extraction or cart
                valuable.OverrideIndestructible(timerSelf + 3f);
                bool fuckit = true;
                if (valuable.TryGetComponent<PhysGrabObjectImpactDetector>(out var impact))
                    fuckit = !(impact.inCart || valuable.grabbed);

                if ((bool)valuable.rb && fuckit)
                {
                    if (valuable.rb.linearVelocity.magnitude <= 25f)
                        valuable.rb.AddForce(valuable.transform.up * (2f * valuable.massOriginal), ForceMode.Impulse);
                    if (valuable.rb.angularVelocity.magnitude <= 25f)
                        valuable.rb.AddTorque(valuable.transform.forward * (2f * Random.Range(0.5f, 2f) * valuable.massOriginal), ForceMode.Impulse);
                }
            }
        }

        if (!ChaosMod.Instance.dizziness)
            return;

        GameDirector.instance.CameraShake.Shake(2f, .5f);
        GameDirector.instance.CameraImpact.Shake(2f, .5f);
    }
}