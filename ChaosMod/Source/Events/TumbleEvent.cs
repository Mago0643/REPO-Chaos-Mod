using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class TumbleEvent: ChaosEvent
{
    public TumbleEvent(): base("evt_tumble", "evt_tumble_desc")
    {
        isOnce = true;
        ID = 2;
    }

    public override void Start()
    {
        base.Start();

        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            foreach (PlayerTumble tumble in Object.FindObjectsByType<PlayerTumble>(0))
            {
                tumble.TumbleSet(true, false);
                tumble.TumbleOverrideTime(Random.Range(2f, 3f));
            }
        }
    }
}