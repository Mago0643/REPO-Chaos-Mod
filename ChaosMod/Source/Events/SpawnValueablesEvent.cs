using Lego.ChaosMod.Backend;

namespace Lego.ChaosMod.Events;
public class SpawnValuablesEvent: ChaosEvent // i dont know how to spawn valuables, so this is also unused.
{
    public SpawnValuablesEvent(): base("귀중품 소환", "무작위 귀중품을 아무 곳에 소환합니다.")
    {
        isOnce = true;
        options.oncePerLevel = true;
        ID = 24;
    }

    public override void Start()
    {
        base.Start();


    }
}