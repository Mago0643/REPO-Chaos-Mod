using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class JointAreDoorsEvent: ChaosEvent // hmm
{
    public JointAreDoorsEvent(): base("evt_show_all_drawers_in_map", "evt_show_all_drawers_in_map_desc")
    {
        isOnce = true;
        options.oncePerLevel = true;
        ID = 12;
    }

    public override void Start()
    {
        base.Start();

        var doorTarget = Object.FindAnyObjectByType<DirtFinderMapDoorTarget>();
        if (doorTarget == null)
        {
            if (ChaosMod.IsDebug)
                Logging.Warn("DirtFinderMapDoorTarget을 찾지 못하였습니다.");
            return;
        }

        foreach (var @object in Object.FindObjectsByType<HingeJoint>(0))
        {
            if (!@object.TryGetComponent<DirtFinderMapDoor>(out var map))
            {
                var door = @object.gameObject.AddComponent<DirtFinderMapDoor>();
                door.DoorPrefab = doorTarget.gameObject;
            }
        }
    }
}