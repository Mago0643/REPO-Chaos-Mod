using Lego.ChaosMod.Backend;
using System.Collections.Generic;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class OilFloorEvent: ChaosEvent // this is unused
{
    public OilFloorEvent(): base("evt_oil_floor", "evt_oil_floor_desc")
    {
        minTimer = 40f;
        maxTimer = 120f;
        ID = 32;
    }

    private List<PlayerAvatar> players = new List<PlayerAvatar>();
    public override void Start()
    {
        players = SemiFunc.PlayerGetAll();

        base.Start();
    }

    public override void Update()
    {
        foreach (PlayerAvatar plr in players) {
            Logging.Log(plr.GetComponent<Rigidbody>().drag);
        }
    }
}