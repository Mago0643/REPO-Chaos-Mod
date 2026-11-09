using Lego.ChaosMod.Backend;
using UnityEngine;

public class InfiniteJumpEvent: ChaosEvent
{
    public InfiniteJumpEvent(): base("evt_infinite_jump", "evt_infinite_jump_desc")
    {
        minTimer = 60f;
        maxTimer = 120f;
        ID = 37;
    }

    private float timer = 0f;
    public override void Update()
    {
        // 더 좋은 방법 추천받습니다
        // 1초에 100번 뛰는 사람은 없겠지???????????????????????????????????
        if (timer > 0f)
            timer -= Time.fixedDeltaTime;
        else {
            PlayerController.instance.JumpExtraCurrent = 100;
            timer = 1f;
        }
    }

    public override void OnFinished()
    {
        timer = 9e9f;
        PlayerController.instance.JumpExtraCurrent = PlayerController.instance.JumpExtra;

        base.OnFinished();
    }
}