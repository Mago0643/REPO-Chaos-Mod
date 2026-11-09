using Lego.ChaosMod.Backend;

namespace Lego.ChaosMod.Events;
public class NothingEvent: ChaosEvent
{
    public NothingEvent(): base("evt_none", "evt_none_desc")
    {
        isOnce = true;
    }
}