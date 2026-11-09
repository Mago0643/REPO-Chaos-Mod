using Lego.ChaosMod.Backend;
using UnityEngine;
using UnityEngine.Rendering;

namespace Lego.ChaosMod.Events;
public class DisableLightingEvent: ChaosEvent
{
    public DisableLightingEvent(): base("evt_disable_all_lights", "evt_disable_all_lights_desc")
    {
        minTimer = 20f;
        maxTimer = 40f;
        ID = 25;
    }

    public AmbientMode ogAmbientMode;
    public Color ogAmbientLight;

    public override void Start()
    {
        base.Start();

        DisableLightingEvent light = (DisableLightingEvent)Instance;
        light.ogAmbientMode = RenderSettings.ambientMode;
        light.ogAmbientLight = RenderSettings.ambientLight;

        if (!ChaosEventManager.Excludes.Contains(Instance))
            ChaosEventManager.Excludes.Add(Instance);
    }

    public override void Update()
    {
        base.Update();

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = Color.white;
        RenderSettings.fog = false;
    }

    public override void OnFinished()
    {
        base.OnFinished();

        // bug: fog colors always being black
        DisableLightingEvent light = (DisableLightingEvent)Instance;
        RenderSettings.ambientMode = light.ogAmbientMode;
        RenderSettings.ambientLight = light.ogAmbientLight;
        RenderSettings.fog = true;

        if (ChaosEventManager.Excludes.Contains(Instance))
            ChaosEventManager.Excludes.Remove(Instance);
    }
}