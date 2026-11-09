using Lego.ChaosMod.Backend;

namespace Lego.ChaosMod.Events;
public class InfiniteStaminaEvent: ChaosEvent
{
    public InfiniteStaminaEvent(): base("evt_infinite_stamina", "evt_infinite_stamina_desc")
    {
        minTimer = 60f;
        maxTimer = 120f;
        ID = 35;
    }

    public override void Update()
    {
        PlayerController.instance.EnergyCurrent = PlayerController.instance.EnergyStart;
    }
}