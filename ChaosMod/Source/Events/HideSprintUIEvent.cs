using Lego.ChaosMod.Backend;

namespace Lego.ChaosMod.Events;
public class HideSprintUIEvent : ChaosEvent
{
    public HideSprintUIEvent() : base("evt_hide_sprint", "evt_hide_sprint_desc")
    {
        minTimer = 20f;
        maxTimer = 35f;
        ID = 10;
    }

    public override void Update()
    {
        base.Update();

        EnergyUI.instance.Hide();
    }

    public override void OnFinished()
    {
        base.OnFinished();

        EnergyUI.instance.Show();
    }
}