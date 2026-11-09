using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class GameSpeedEvent: ChaosEvent
{
    internal static float[] mults = [];
    internal static int selectedIdx = 0;
    public GameSpeedEvent(float[] mults): base("evt_game_speed", "evt_game_speed_desc")
    {
        minTimer = 20f;
        maxTimer = 25f;
        GameSpeedEvent.mults = mults;
        ID = 3;
    }

    public override string GetName()
    {
        return Language.GetText(nameKey).Replace("%f", mults[selectedIdx].ToString());
    }

    public override void Start()
    {
        base.Start();
        selectedIdx = Random.Range(0, mults.Length);
        Time.timeScale = mults[selectedIdx];
    }

    public override void OnFinished()
    {
        base.OnFinished();
        Time.timeScale = 1f;
    }
}