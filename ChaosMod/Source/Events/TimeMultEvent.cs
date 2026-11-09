using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class TimeMultEvent: ChaosEvent
{
    internal static float[] mults = [];
    internal static int selectedIdx = 0;
    public TimeMultEvent(float[] mults): base("evt_timer_mult", "evt_timer_mult_desc")
    {
        TimeMultEvent.mults = mults;
        minTimer = 30f;
        maxTimer = 60f;
        ID = 31;
    }

    public override string GetName()
    {
        return Language.GetText(nameKey).Replace("%f", mults[selectedIdx].ToString());
    }

    public override void Start()
    {
        base.Start();
        selectedIdx = Random.Range(0, mults.Length);
        ChaosMod.Instance.controller!.timeScale = mults[selectedIdx];
    }

    public override void OnFinished()
    {
        ChaosMod.Instance.controller!.timeScale = 1f;
        base.OnFinished();
    }
}