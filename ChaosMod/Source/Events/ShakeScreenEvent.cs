using Lego.ChaosMod.Backend;
using System.Collections;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class ShakeScreenEvent: ChaosEvent
{
    public ShakeScreenEvent(): base("evt_earthquake", "evt_earthquake_desc")
    {
        minTimer = 15f;
        maxTimer = 20f;
        ID = 15;
    }

    public override void Start()
    {
        base.Start();
        if (!ChaosMod.Instance.dizziness)
            return;

        // ChaosMod.Instance.StartCoroutine(AudioFadeIn());
        ChaosMod.Instance.RumbleAS!.volume = 1f;
        ChaosMod.Instance.RumbleAS.Play();
    }

    public override void Update()
    {
        base.Update();
        if (!ChaosMod.Instance.dizziness)
            return;

        GameDirector.instance.CameraShake.Shake(2f, .5f);
        GameDirector.instance.CameraImpact.Shake(2f, .5f);
    }

    public override void OnFinished()
    {
        base.OnFinished();
        if (!ChaosMod.Instance.dizziness)
            return;

        ChaosMod.Instance.StartCoroutine(AudioFadeOut());
    }

    IEnumerator AudioFadeIn()
    {
        AudioSource src = ChaosMod.Instance.RumbleAS;
        src.volume = 0f;
        src.Play();

        System.Func<float, float> ease = t => Mathf.Pow(t, 3.322f);
        float step = 0f;
        while (step < 1f)
        {
            src.volume = ease(step);
            step += Time.unscaledDeltaTime;
            yield return new WaitForEndOfFrame();
        }
        src.volume = 1f;
    }

    IEnumerator AudioFadeOut()
    {
        AudioSource src = ChaosMod.Instance.RumbleAS;
        src.volume = 1f;

        System.Func<float, float> ease = t => Mathf.Pow(-t, 3.322f) + 1f;
        float step = 0f;
        while (step < 1f)
        {
            src.volume = ease(step);
            step += Time.unscaledDeltaTime;
            yield return new WaitForEndOfFrame();
        }
        src.volume = 0f;
        src.Stop();
    }
}