using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class BreakAllDoorEvent: ChaosEvent // this also gets too annoying if all door breaks
{
    public BreakAllDoorEvent(): base("evt_break_all_door", "evt_break_all_door_desc")
    {
        isOnce = true;
        ID = 6;
    }

    public override void Start()
    {
        base.Start();

        /*if (ModVars.brokeAllDoor_didRun)
        {
            foreach (var @object in Object.FindObjectsByType<HingeJoint>(0))
            {
                @object.GetComponent<PhysGrabObjectImpactDetector>().BreakHeavy(Vector3.zero, true);
            }
            return;
        }

        ModVars.brokeAllDoor_didRun = true;*/
        foreach (var @object in GameObject.FindObjectsByType<HingeJoint>(0))
        {
            if (/*@object.transform.name.Contains("Hinge")*/ true)
            {
                // 물리 충격을 주어서 문 부수기
                Rigidbody rb = @object.GetComponent<Rigidbody>();
                rb.AddForce(Vector3.forward * 5000f, ForceMode.Impulse);
            }
        }
    }
}