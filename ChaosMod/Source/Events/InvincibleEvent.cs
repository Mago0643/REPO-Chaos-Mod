using Lego.ChaosMod.Backend;

namespace Lego.ChaosMod.Events;
public class InvincibleEvent: ChaosEvent
{
    public InvincibleEvent(): base("evt_invincible", "evt_invincible_desc")
    {
        minTimer = 60f;
        maxTimer = 120f;
        ID = 36;
    }

    public override void Start()
    {
        foreach (PlayerAvatar plr in SemiFunc.PlayerGetAll())
        {
            plr.playerHealth.godMode = true;
        }
        base.Start();
    }

    public override void OnFinished()
    {
        foreach (PlayerAvatar plr in SemiFunc.PlayerGetAll())
        {
            plr.playerHealth.godMode = false;
        }
        base.OnFinished();
    }
}