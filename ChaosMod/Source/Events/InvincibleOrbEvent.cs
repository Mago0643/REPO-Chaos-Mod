using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;

public class InvincibleOrbEvent: ChaosEvent
{
    public InvincibleOrbEvent() : base("evt_titanume_orb", "evt_titanume_orb_desc")
    {
        minTimer = 40f;
        maxTimer = 70f;
        ID = 21;
    }

    public override void Update()
    {
        base.Update();

        foreach (var orb in Object.FindObjectsByType<EnemyValuable>(0))
        {
            orb.GetComponent<PhysGrabObjectImpactDetector>().destroyDisable = true;
        }
    }
}