using Lego.ChaosMod.Backend;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Lego.ChaosMod.Events;
public class ThinkFastEvent: ChaosEvent
{
    internal static Image? flash;
    internal static Image? scout;

    internal static GameObject? text1;
    internal static GameObject? text2;

    internal static string StunItemPath = "items/Item Grenade Stun";
    internal static GameObject? StunItemPrefab;

    public ThinkFastEvent(): base("evt_think_fast", "evt_think_fast_desc")
    {
        isOnce = true;
        options.oncePerLevel = true;
        ID = 28;

        StunItemPrefab = Resources.Load<GameObject>(StunItemPath);
    }

    public override void Start()
    {
        base.Start();

        ChaosMod.Instance.StartCoroutine(Flashback());
    }

    IEnumerator Flashback()
    {
        var inst = ChaosMod.Instance;
        AudioClip voice = inst.assets!.LoadAsset<AudioClip>("Scout_stunballhit15");
        AudioClip flashback = inst.assets.LoadAsset<AudioClip>("flashbang");

        inst.AudioSource!.PlayOneShot(voice);

        text1!.SetActive(true);
        scout!.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(0.578f);

        text2!.SetActive(true);
        yield return new WaitForSecondsRealtime(0.667f);

        // flashback
        text1.SetActive(false);
        text2.SetActive(false);
        // inst.AudioSource.PlayOneShot(flashback);
        scout.gameObject.SetActive(false);
        
        //PlayerAvatar.instance.tumble.TumbleSet(true, false);
        //PlayerAvatar.instance.tumble.TumbleOverrideTime(2f);

        if (GameManager.Multiplayer())
        {
            GameObject itemStun = PhotonNetwork.Instantiate(StunItemPath, PlayerAvatar.instance.localCamera.transform.position + new Vector3(0f, 2f, 0f), Quaternion.identity);
            ChaosController.instance.view.RPC("GrenadeStunExplosionRPC", RpcTarget.All, itemStun.GetComponent<PhotonView>().ViewID);
        } else {
            GameObject itemStun = Object.Instantiate(StunItemPrefab, PlayerAvatar.instance.localCamera.transform.position + new Vector3(0f, 2f, 0f), Quaternion.identity)!;
            var script = itemStun.GetComponent<ItemGrenade>();
            script.tickTime = 0;
            Util.GetInternalVar(script, "isActive").SetValue(script, true);
        }

        //System.Func<float, float> easeInExpo = t => Mathf.Pow(2f, 10f * t - 10f);
        //float time = 0f;
        //while (time < flashback.length)
        //{
        //    float alpha = time / flashback.length;

        //    Color col = flash.color;
        //    col.a = 1f - easeInExpo(alpha);
        //    flash.color = col;

        //    time += Time.unscaledDeltaTime;
        //    yield return null;
        //}
        //flash.color = new Color(1,1,1,0);
    }
}