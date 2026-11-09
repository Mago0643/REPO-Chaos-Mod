using Lego.ChaosMod.Backend;

namespace Lego.ChaosMod.Events;
public class QuakeFOVEvent: ChaosEvent
{
    public QuakeFOVEvent(): base("evt_quake_fov", "evt_quake_fov_desc")
    {
        minTimer = 20f;
        maxTimer = 30f;
        ID = 5;
    }

    public override void Update()
    {
        base.Update();
        CameraZoom.Instance.OverrideZoomSet(ChaosMod.Instance.dizziness ? 150f : 90f, .5f, 0.5f, 1f, ChaosMod.Instance.gameObject, 10);
    }
}