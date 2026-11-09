using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class DisablePostProcessingEvent: ChaosEvent
{
    public DisablePostProcessingEvent(): base("evt_disable_post_processing", "evt_disable_post_processing_desc")
    {
        minTimer = 30f;
        maxTimer = 60f;
        ID = 11;
    }

    public static GameObject? post;
    public override void Start()
    {
        base.Start();
        if (post == null) post = Object.FindAnyObjectByType<PostProcessing>().transform.parent.gameObject;
        post.SetActive(false);

    }

    public override void OnFinished()
    {
        base.OnFinished();
        post!.SetActive(true);
    }
}