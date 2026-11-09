using Lego.ChaosMod.Backend;

namespace Lego.ChaosMod.Events;
public class ReviveAllPlayersEvent: ChaosEvent
{
    public ReviveAllPlayersEvent(): base("evt_revive_all", "evt_revive_all_desc")
    {
        ID = 23;
        isOnce = true;
    }

    public override void Start()
    {
        base.Start();
        if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

        foreach (var plr in SemiFunc.PlayerGetAll())
        {
            if (Util.GetInternalVar<bool>(plr, "deadSet"))
            {
                plr.Revive();
                // total health 2, but i don't think this works
                plr.playerHealth.Heal(1);
            }
        }
    }
}