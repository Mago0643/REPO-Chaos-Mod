using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class CarCrash: ChaosEvent // this is unused, because car was showing errors at other clients, and this events annoys shit out of me
{
    public static AssetBundle? car_assets;
    public CarCrash(): base("evt_driver_of_the_year", "evt_driver_of_the_year_desc")
    {
        options.singleplayerOnly = true;
        minTimer = 120f;
        maxTimer = 180f;
        ID = 26;
        if (car_assets == null)
        {
            car_assets = AssetBundle.LoadFromFile(Util.GetPluginDirectory("car_assets"));
            ChaosMod.Instance.PrefabToAddNetwork.Add(car_assets.LoadAsset<GameObject>("Killer Joe"));
        }
    }

    public override void Start()
    {
        base.Start();

        ChaosMod.Instance.car!.transform.position = Vector3.zero;
        ChaosMod.Instance.car.transform.rotation = Quaternion.identity;
        ChaosMod.Instance.car.Spawn();

        ChaosEventManager.Excludes.Add(Instance);
    }

    public override void OnFinished()
    {
        base.OnFinished();

        ChaosMod.Instance.car!.Despawn();
        ChaosEventManager.Excludes.Remove(Instance);
    }
}