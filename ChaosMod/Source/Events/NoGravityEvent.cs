using Lego.ChaosMod.Backend;

namespace Lego.ChaosMod.Events;
public class NoGravityEvent: ChaosEvent
{
    public NoGravityEvent(): base("evt_moon_gravity", "evt_moon_gravity_desc")
    {
        minTimer = 60f;
        maxTimer = 120f;
        ID = 27;
    }

    public override void Update()
    {
        base.Update();

        PlayerController.instance.AntiGravity(0.1f);
    }
}