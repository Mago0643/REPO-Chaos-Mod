using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class AddHealthEvent: ChaosEvent
{
    internal static int[] amounts = [];
    internal static int selected = 0;
    public AddHealthEvent(int[] amounts): base("evt_health_positive", "evt_health_desc")
    {
        AddHealthEvent.amounts = amounts;
        nameKey = "Give Health";
        isOnce = true;
        options.oncePerLevel = true;
        ID = 8;
    }

    public override string GetName()
        => amounts[selected] >= 0 ? "evt_add_health_positive" : "evt_add_health_negative";

    public override void Start()
    {
        base.Start();

        selected = Random.Range(0, amounts.Length);
        int amount = amounts[selected];
        ForceUpdateName();

        if (amount == 0) return;

        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            foreach (var plr in Object.FindObjectsByType<PlayerHealth>(0))
            {
                bool isMinus = Mathf.Sign(amount) < 0;
                if (isMinus)
                    plr.Hurt(Mathf.Abs(amount), true);
                else
                    plr.Heal(Mathf.Abs(amount));
            }
        }
    }
}