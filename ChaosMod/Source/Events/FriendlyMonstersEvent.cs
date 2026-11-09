using Lego.ChaosMod.Backend;
using System.Collections.Generic;

namespace Lego.ChaosMod.Events;
public class FriendlyMonstersEvent: ChaosEvent
{
    public FriendlyMonstersEvent(): base("evt_friendly_monsters", "evt_friendly_monsters_desc")
    {
        minTimer = 60f;
        maxTimer = 120f;
        ID = 18;
    }

    static List<string> playerSteamIDs = new();
    public override void Start()
    {
        base.Start();

        foreach (var semibot in SemiFunc.PlayerGetAll()) {
            if (!EnemyDirector.instance.debugNoVision.Contains(semibot.steamID)) {
                EnemyDirector.instance.debugNoVision.Add(semibot.steamID);
            } else playerSteamIDs.Add(semibot.steamID);
        }
    }

    public override void OnFinished()
    {
        base.OnFinished();
        
        foreach (var semibot in SemiFunc.PlayerGetAll()) {
            if (EnemyDirector.instance.debugNoVision.Contains(semibot.steamID) && playerSteamIDs.Contains(semibot.steamID)) {
                EnemyDirector.instance.debugNoVision.Remove(semibot.steamID);
            }
        }
        playerSteamIDs.Clear();
    }
}