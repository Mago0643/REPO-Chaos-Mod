using Lego.ChaosMod.Backend;

public class HideHealthUIEvent : ChaosEvent
{
    public HideHealthUIEvent(): base("evt_hide_health", "evt_hide_health_desc")
    {
        minTimer = 20f;
        maxTimer = 35f;
        ID = 9;
    }

    public override void Update()
    {
        base.Update();

        HealthUI.instance.Hide();
    }

    public override void OnFinished()
    {
        base.OnFinished();

        HealthUI.instance.Show();
    }
}