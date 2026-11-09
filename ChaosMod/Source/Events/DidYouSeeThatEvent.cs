using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public sealed class DidYouSeeThatEvent: ChaosEvent
{
    static GameObject? SkeletonPrefab;
    public DidYouSeeThatEvent(): base("evt_did_you_see_that", "evt_did_you_see_that_desc")
    {
        ID = 40;
        isOnce = false;
        minTimer = 10;
        maxTimer = 10;
        options.hideTimer = true;

        SkeletonPrefab = ChaosMod.Instance.assets!.LoadAsset<GameObject>("Skeleton Running");
    }

    public override void Start()
    {
        base.Start();
        sk_cooldown = Random.Range(1f, 2f);
    }

    float sk_cooldown = 0f;
    float timer = 0f;
    float textTimer = -100f;
    public override void Update()
    {
        timer += Time.unscaledDeltaTime;
        textTimer += Time.unscaledDeltaTime;
        if (timer >= sk_cooldown) {
            timer = -200;
            textTimer = 0f;
            LmaoSkeleton?.SetActive(true);
            anim!.Play("Skeleton", -1, 0f);
        }

        if (textTimer >= 0.125f) {
            textTimer = -200f;
            nameKey = "evt_did_you_see_that_reveal";
            ForceUpdateName();
        }
    }

    public override void OnFinished()
    {
        base.OnFinished();
        LmaoSkeleton?.SetActive(false);
    }

    static GameObject? LmaoSkeleton;
    static Animator? anim;
    public override void OnStartGame(RectTransform rect)
    {
        LmaoSkeleton = Object.Instantiate(SkeletonPrefab)!;
        LmaoSkeleton.SetActive(false);
        LmaoSkeleton.transform.SetParent(rect, false);

        anim = LmaoSkeleton.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
    }

    public override void OnLeave()
    {
        Logging.Log("Leaving to menu");
        Object.Destroy(LmaoSkeleton);
        LmaoSkeleton = null!;
        anim = null!;
    }
}