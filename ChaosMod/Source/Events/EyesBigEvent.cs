using Lego.ChaosMod.Backend;

namespace Lego.ChaosMod.Events;
public class EyesBigEvent: ChaosEvent // oohhh man i love this little robots... theyre too cute that I CANT HANDLE IT
{
    public EyesBigEvent(): base("evt_big_eyes", "evt_big_eyes_desc")
    {
        minTimer = 30f;
        maxTimer = 42.5f;
        options.multiplayerOnly = true;
        ID = 22;
    }

    public override void Update()
    {
        base.Update();
        if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
        foreach (PlayerAvatar avatar in SemiFunc.PlayerGetAll())
        {
            avatar.OverridePupilSize(2f, 10, 1f, 1f, 5f, 0.5f);
        }
    }
}