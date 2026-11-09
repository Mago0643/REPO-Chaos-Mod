using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class FuckGravityEvent: ChaosEvent
{
    public FuckGravityEvent(): base("evt_fuck_gravity", "evt_fuck_gravity_desc")
    {
        minTimer = 30f;
        maxTimer = 40f;
        ID = 39;
    }

    float gravity = -9.81f;

    float rand()
    {
        return (Random.Range(0, 2) - 0.5f) * 0.3f;
    }

    public override void Update()
    {
        gravity += rand();
        Physics.gravity = new Vector3(0f, gravity, 0f);
        base.Update();
    }

    public override void OnFinished()
    {
        Physics.gravity = new Vector3(0f, -9.81f, 0f);
        base.OnFinished();
    }
}