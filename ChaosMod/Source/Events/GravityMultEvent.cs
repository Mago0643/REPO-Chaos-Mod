using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class GravityMultEvent: ChaosEvent
{
    internal static float[] mults = [];
    internal static int selectedIdx = 0;
    public GravityMultEvent(float[] mults): base("evt_gravity_mult", "evt_gravity_mult_desc")
    {
        GravityMultEvent.mults = mults;
        minTimer = 30f;
        maxTimer = 60f;
        ID = 30;
    }

    public override string GetName()
        => Language.GetText(nameKey).Replace("%f", mults[selectedIdx].ToString());

    public override void Start()
    {
        base.Start();
        selectedIdx = Random.Range(0, mults.Length);
        Physics.gravity = new Vector3(0f, -9.81f * mults[selectedIdx], 0f);
    }

    public override void OnFinished()
    {
        Physics.gravity = new Vector3(0f, -9.81f, 0f);
        base.OnFinished();
    }
}