using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class RandomItemsEvent: ChaosEvent
{
    public RandomItemsEvent(): base("evt_random_items", "evt_random_items_desc")
    {
        isOnce = true;
        ID = 34;
        options.chance = 90f;
    }

    public override void Start()
    {
        string itemPrefabName = "items/";
        string[] items = [ // there's a cart named 'Big Cart', it is quite bigger than Medium cart. maybe this is another LORE????
            "Item Cart Cannon", "Item Cart Laser", "Item Cart Small", "Item Cart Medium",
            "Item Drone Battery", "Item Drone Feather", "Item Drone Indestructible", "Item Drone Torque", "Item Drone Zero Gravity",
            "Item Duck Bucket", "Item Extraction Tracker", "Item Grenade Duct Taped", "Item Grenade Explosive", "Item Grenade Human", "Item Grenade Shockwave", "Item Grenade Stun",
            "Item Gun Handgun", "Item Gun Laser", "Item Gun Shockwave", "Item Gun Shotgun", "Item Gun Stun", "Item Gun Tranq",
            "Item Health Pack Large", "Item Health Pack Medium", "Item Health Pack Small",
            "Item Melee Baseball Bat", "Item Melee Frying Pan", "Item Melee Inflatable Hammer", "Item Melee Sledge Hammer", "Item Melee Stun Baton", "Item Melee Sword",
            "Item Mine Explosive", "Item Mine Shockwave", "Item Mine Stun", "Item Orb Zero Gravity", "Item Phase Bridge", "Item Power Crystal",
            // this is an important one
            "Item Rubber Duck",
            "Item Upgrade Death Head Battery", "Item Upgrade Map Player Count", "Item Upgrade Player Crouch Rest", "Item Upgrade Player Energy", "Item Upgrade Player Extra Jump",
            "Item Upgrade Player Grab Range", "Item Upgrade Player Grab Strength", "Item Upgrade Player Health", "Item Upgrade Player Sprint Speed", "Item Upgrade Player Tumble Climb",
            "Item Upgrade Player Tumble Launch", "Item Upgrade Player Tumble Wings", "Item Valuable Tracker"
        ];

        // TODO: Think about modded items
        // maybe i can get all items by messing with
        // REPOLib.Modules.NetworkPrefabs.PrefabRefs?? 

        foreach (PlayerAvatar semibot in SemiFunc.PlayerGetAll())
        {
            string itemName = itemPrefabName + items[Random.Range(0,items.Length)];
            GameObject item = Object.Instantiate(Resources.Load<GameObject>(itemName), semibot.transform.position + new Vector3(0f, 2f, 0f), Quaternion.identity);

            // do i need to save them forever? or not.
            // adding option later to turn off because think about when this event happens rapidly
            //ItemAttributes attr = item.GetComponent<ItemAttributes>();
            //StatsManager.instance.itemDictionary[attr.item.name] = attr.item;
            //StatsManager.instance.itemsPurchased[attr.item.name]++;
            //StatsManager.instance.itemsPurchasedTotal[attr.item.name]++;
        }

        base.Start();
    }
}