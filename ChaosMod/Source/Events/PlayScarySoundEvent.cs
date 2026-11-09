using Lego.ChaosMod.Backend;
using UnityEngine;

namespace Lego.ChaosMod.Events;
// THIS IS NOT DONE YET!!
public class PlayScarySoundEvent: ChaosEvent
{
    public PlayScarySoundEvent(): base("evt_doodoo_scary", "evt_doodoo_scary_desc")
    {
        isOnce = true;
    }

    public override void Start()
    {
        AudioClip clip = ChaosMod.Instance.assets!.LoadAsset<AudioClip>("scary_sound" + Random.Range(0, 12));
        ChaosMod.Instance.AudioSource!.PlayOneShot(clip);

        base.Start();
    }
}