using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class RandomUpgradeEvent: ChaosEvent
{
    public RandomUpgradeEvent(): base("evt_random_upgrade", "evt_random_upgrade_desc")
    {
        isOnce = true;
        ID = 33;
    }

    public override void Start()
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
        foreach (PlayerAvatar semibot in SemiFunc.PlayerGetAll())
        {
            UpgradePlayer(semibot);
        }

        base.Start();
    }

    void UpgradePlayer(PlayerAvatar plr)
    {
        // TODO: Maybe think about upgrade mods??
        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            string steamID = SemiFunc.PlayerGetSteamID(plr);
            string upgrade = "Map Player Count";
            var upgradeFunc = PunManager.instance.UpgradeMapPlayerCount;
            switch (Random.Range(0, 12))
            {
                case 0:
                    upgradeFunc = PunManager.instance.UpgradeDeathHeadBattery;
                    upgrade = "Death Head Battery";
                    break;
                case 1:
                    upgradeFunc = PunManager.instance.UpgradePlayerCrouchRest;
                    upgrade = "Crouch Rest";
                    break;
                case 2:
                    upgradeFunc = PunManager.instance.UpgradePlayerEnergy;
                    upgrade = "Energy";
                    break;
                case 3:
                    upgradeFunc = PunManager.instance.UpgradePlayerExtraJump;
                    upgrade = "Extra Jump";
                    break;
                case 4:
                    upgradeFunc = PunManager.instance.UpgradePlayerGrabRange;
                    upgrade = "Grab Range";
                    break;
                case 5:
                    upgradeFunc = PunManager.instance.UpgradePlayerGrabStrength;
                    upgrade = "Grab Strength";
                    break;
                case 6:
                    upgradeFunc = PunManager.instance.UpgradePlayerHealth;
                    upgrade = "Health";
                    break;
                case 7:
                    upgradeFunc = PunManager.instance.UpgradePlayerSprintSpeed;
                    upgrade = "Sprint Speed";
                    break;
                //case 8: // i think this is unused? or in the future update?? or this is LORE???????????
                //        // not adding it cuz felt not so.
                //    upgradeFunc = PunManager.instance.UpgradePlayerThrowStrength;
                //    upgrade = "Throw Strength";
                //    break;
                case 8:
                    // map upgrade is useless after getting one.
                    // reroll again

                    // BUG: they changed how this works? i think finding with steamID doesn't work.
                    if (StatsManager.instance.playerUpgradeMapPlayerCount[steamID] > 0)
                        UpgradePlayer(plr);
                    break;
                case 9:
                    upgradeFunc = PunManager.instance.UpgradePlayerTumbleClimb;
                    upgrade = "Tumble Climb";
                    break;
                case 10:
                    upgradeFunc = PunManager.instance.UpgradePlayerTumbleLaunch;
                    upgrade = "Tumble Launch";
                    break;
                case 11:
                    upgradeFunc = PunManager.instance.UpgradePlayerTumbleWings;
                    upgrade = "Tumble Wings";
                    break;
            }
            upgradeFunc(steamID, 1); // only 1 upgrade cuz it makes game pretty boring
            Logging.Log(SemiFunc.PlayerGetName(plr) + " got \"" + upgrade + "\" upgrade.");
        }

        // play animations
        if (SemiFunc.PlayerAvatarLocal() == plr)
        {
            StatsUI.instance.Fetch();
            StatsUI.instance.ShowStats();
            CameraGlitch.Instance.PlayUpgrade();
        } else { 
            GameDirector.instance.CameraImpact.ShakeDistance(5f, 1f, 6f, plr.transform.position, 0.2f);
        }

        if (!GameManager.Multiplayer() || SemiFunc.IsMasterClient())
        {
            plr.playerHealth.MaterialEffectOverride(PlayerHealth.Effect.Upgrade);
        }
    }
}