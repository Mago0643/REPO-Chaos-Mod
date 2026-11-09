using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class VoicePitchEvent: ChaosEvent
{
    internal static float[] pitches = [];
    internal static int selectedIdx = 0;

    public VoicePitchEvent(float[] pitches): base("목소리 음조절", "목소리의 음을 조절합니다.")
    {
        minTimer = 30f;
        maxTimer = 60f;

        options.multiplayerOnly = true;
        VoicePitchEvent.pitches = pitches;
        ID = 20;
    }

    public override void Start()
    {
        base.Start();
        selectedIdx = Random.Range(0, pitches.Length);
    }

    public float timeShit = 0f;
    public override void Update()
    {
        base.Update();

        if (timeShit > 0f)
            timeShit -= Time.unscaledDeltaTime;
        else
        {
            timeShit = .5f;
            foreach (var pa in SemiFunc.PlayerGetAll())
                pa.voiceChat.OverridePitch(pitches[selectedIdx], 1f, 1f, 0.5f);
        }

    }
}