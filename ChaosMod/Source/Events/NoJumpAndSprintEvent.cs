using Lego.ChaosMod.Backend;

namespace Lego.ChaosMod.Events;
public class NoJumpAndSprintEvent: ChaosEvent // this is so evil
{
    public NoJumpAndSprintEvent(): base("evt_no_jump_sprint", "evt_no_jump_sprint_desc")
    {
        minTimer = 20f;
        maxTimer = 40f;
        ID = 19;
    }

    public override void Update()
    {
        base.Update();

        PlayerController.instance.EnergyCurrent = 0f;
        PlayerController.instance.JumpExtraCurrent = 0;
        PlayerController.instance.OverrideJumpCooldown(timerSelf);
    }

    public override void OnFinished()
    {
        base.OnFinished();

        PlayerController.instance.EnergyCurrent = PlayerController.instance.EnergyStart;
        PlayerController.instance.OverrideJumpCooldown(0f);
        PlayerController.instance.JumpExtraCurrent = PlayerController.instance.JumpExtra;
    }
}