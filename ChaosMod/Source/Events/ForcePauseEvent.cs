using Lego.ChaosMod.Backend;

namespace Lego.ChaosMod.Events;
public class ForcePauseEvent: ChaosEvent // yea this is unused
{
    public ForcePauseEvent(): base("evt_force_pause", "evt_force_pause_desc")
    {
        isOnce = true;
    }

    public override void Start()
    {
        base.Start();
        MenuPageEsc.instance?.menuPage.PageStateSet(MenuPage.PageState.Active);
    }
}