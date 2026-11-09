using Lego.ChaosMod.Backend;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class ClubMoodEvent: ChaosEvent // this thing annoys some people i think?
{
    public static float BPM = 140.000f;
    float bps = 0f;
    AudioSource src;
    public ClubMoodEvent(): base("evt_club_mood", "evt_club_mood_desc")
    {
        ID = 29;
        float tempBPS = 60f / BPM;

        while (minTimer < 60f)
        {
            minTimer += tempBPS;
        }
        maxTimer = minTimer;
        while (maxTimer < 90f)
        {
            maxTimer += tempBPS;
        }
    }

    Color ogLight = Color.clear;
    private List<PlayerAvatar> players;
    private PhysGrabObject[] items;
    public override void Start()
    {
        base.Start();

        foreach (var evt in ChaosEventManager.Events)
        {
            if (evt is ClubMoodEvent)
                ChaosEventManager.Excludes.Add(evt);
        }

        // ChaosMod.Instance.spawnedEnemys = GameObject.FindObjectsByType<EnemyParent>(0).ToHashSet();

        src = ChaosMod.Instance.ClubSource!;
        src.volume = 0f;
        src.Play();

        bps = 60f / BPM;

        ogLight = RenderSettings.ambientLight;

        items = Object.FindObjectsByType<PhysGrabObject>(0);

        players = SemiFunc.PlayerGetAll();
        ChaosMod.Instance.StartCoroutine(MusicFadeIn());
    }

    IEnumerator MusicFadeIn()
    {
        float t = 0f;
        while (t < 1f)
        {
            src.volume = t;

            t += Time.deltaTime * 2f;
            yield return null;
        }

        src.volume = 1f;
    }

    int lastBeat = -1;
    Color[] colors = [
        Color.red, new Color(1,1,0), Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta
    ];
    Color curColor = Color.clear;
    Color lastColor = Color.blue;
    public override void Update()
    {
        float beat = src.time / bps;
        if (Mathf.FloorToInt(beat) != lastBeat)
        {
            lastBeat = Mathf.FloorToInt(beat);

            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                foreach (var enemy in ChaosMod.Instance.spawnedEnemys)
                {
                    if (enemy == null) return;
                    
                    var rb = enemy.GetComponent<EnemyRigidbody>();
                    if (rb == null) continue;
                    rb.enemy.GetComponent<EnemyStateStunned>().Set(timerSelf + 5f);

                    var realRB = rb.GetComponent<Rigidbody>();
                    realRB.AddForce(Vector3.up * 15f, ForceMode.Impulse);
                }


                foreach (var valuable in items)
                {
                    if (valuable == null) continue;
                    valuable.OverrideIndestructible(timerSelf + 3f);
                    bool fuckit = true;
                    if (valuable.TryGetComponent<PhysGrabObjectImpactDetector>(out var impact))
                        fuckit = !(impact.inCart || valuable.grabbed);
                    if (valuable.name.Contains("Cart"))
                        fuckit = false;

                    if ((bool)valuable.rb && fuckit)
                        valuable.rb.AddForce(Vector3.up * (3.5f * valuable.massOriginal), ForceMode.Impulse);
                }
            }

            curColor = colors[Random.Range(0, colors.Length)];
            while (curColor == lastColor)
            {
                curColor = colors[Random.Range(0, colors.Length)];
            }
            lastColor = curColor;
        }
        RenderSettings.ambientLight = curColor;

        float headTilt = Mathf.Sin(Mathf.PI * 2f * beat) * 25f;

        foreach (var plr in players)
        {
            if (plr == null) continue;
            // local
            if (SemiFunc.PlayerAvatarLocal() == plr) // idk this works, never test it with others
            {
                plr.playerExpression.OverrideExpressionSet(4, 100f);
                PlayerExpressionsUI.instance.playerExpression.OverrideExpressionSet(4, 100f);
                PlayerExpressionsUI.instance.playerAvatarVisuals.HeadTiltOverride(headTilt * 0.5f);
            } else {
                plr.playerAvatarVisuals.HeadTiltOverride(headTilt);
            }
        }

        float beatX = Util.BeatMath(beat, BPM, 0f);
        for (int i = 0; i < ChaosController.instance.events.Count; i++)
        {
            TextLerp text = ChaosController.instance.events[i].evtNameText;
            if (text == null) continue;
            text.posOffset = new Vector2(beatX * (i % 2 == 0 ? 1f : -1f), 0f);
        }
    }

    IEnumerator MusicFadeOut()
    {
        AudioSource src = ChaosMod.Instance.ClubSource!;
        float t = 0f;
        while (t < 1f)
        {
            src.volume = 1f-t;

            t += Time.deltaTime * 2f;
            yield return null;
        }

        src.volume = 0f;
        src.Stop();
    }

    public override void OnFinished()
    {
        foreach (var evt in ChaosEventManager.Events)
        {
            if (evt is ClubMoodEvent)
                ChaosEventManager.Excludes.Remove(evt);
        }
        ChaosMod.Instance.StartCoroutine(MusicFadeOut());
        RenderSettings.ambientLight = ogLight;
        for (int i = 0; i < ChaosController.instance.events.Count; i++)
        {
            TextLerp text = ChaosController.instance.events[i].evtNameText;
            if (!(bool)text) continue;
            text.posOffset = Vector2.zero;
        }
        base.OnFinished();
    }
}