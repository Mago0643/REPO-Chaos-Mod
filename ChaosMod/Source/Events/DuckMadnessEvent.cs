using Lego.ChaosMod.Backend;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace Lego.ChaosMod.Events;
public class DuckMadnessEvent: ChaosEvent // another annoying event that i made
{
    private string rubberDuckPath = "items/Item Rubber Duck";
    private GameObject rubberDuckPrefab;

    public DuckMadnessEvent(): base("evt_crazy_ducks", "evt_crazy_ducks_desc")
    {
        minTimer = 25f;
        maxTimer = 30f;

        ID = 13;

        rubberDuckPrefab = Resources.Load<GameObject>(rubberDuckPath);
        if (rubberDuckPrefab == null)
            Logging.Error("rubberDuckPrefab does not exists!");
    }

    public List<ItemRubberDuck> ducks = new List<ItemRubberDuck>();
    public override void Start()
    {
        base.Start();
        if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

        if (!ChaosEventManager.Excludes.Contains(Instance))
            ChaosEventManager.Excludes.Add(Instance);

        var players = SemiFunc.PlayerGetAll();
        foreach (var player in players)
        {
            var pos = player.transform.position + Vector3.up;
            var rot = player.transform.rotation;

            // spawn total 10 rubber ducks to players.
            for (int i = 0; i < 9-players.Count; i++)
            {
                // make duck
                GameObject @object = null!;
                if (GameManager.Multiplayer())
                    @object = PhotonNetwork.Instantiate(rubberDuckPath, pos, rot);
                else
                    @object = GameObject.Instantiate(rubberDuckPrefab, pos, rot);

                // throw it
                var rig = @object.GetComponent<Rigidbody>();
                rig.isKinematic = false;
                rig.AddForce(@object.transform.forward * 100f);

                var battery = @object.GetComponent<ItemBattery>();

                battery.autoDrain = false;
                battery.batteryActive = false;
                battery.batteryDrainRate = 0f;
                battery.batteryLife = 100f;

                // add to list
                var duck = @object.GetComponent<ItemRubberDuck>();
                ((DuckMadnessEvent)Instance).ducks.Add(duck);
            }
        }
    }

    public override void Update()
    {
        base.Update();
        
        if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

        foreach (var duck in ((DuckMadnessEvent)Instance).ducks)
        {
            var grabber = duck.GetComponent<PhysGrabObject>();

            // makes ducks keep going crazy
            var rig = duck.GetComponent<Rigidbody>();
            rig.AddForce(duck.transform.forward * 100f);
            if (rig.linearVelocity.magnitude > 500f)
                rig.linearVelocity = rig.linearVelocity.normalized * 500f;

            // do not make player can grab ducks
            if (grabber.playerGrabbing.Count > 0 && SemiFunc.IsMasterClientOrSingleplayer())
                foreach (var plr in grabber.playerGrabbing)
                {
                    if (plr == null) continue;

                    if (!GameManager.Multiplayer())
                        plr.ReleaseObject(duck.GetComponent<PhotonView>().ViewID);
                    else
                        plr.photonView.RPC("ReleaseObjectRPC", RpcTarget.All, false, duck.GetComponent<PhotonView>().ViewID, 0.1f);
                }
        }
    }

    public override void OnFinished()
    {
        base.OnFinished();
        
        if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
        for (int i = 0; i < ((DuckMadnessEvent)Instance).ducks.Count; i++)
        {
            var obj = ((DuckMadnessEvent)Instance).ducks[i].gameObject;
            if (obj == null)
                continue;

            if (GameManager.Multiplayer())
                PhotonNetwork.Destroy(obj);
            else
                Object.Destroy(obj);
        }
        ((DuckMadnessEvent)Instance).ducks.Clear();

        if (ChaosEventManager.Excludes.Contains(Instance))
            ChaosEventManager.Excludes.Remove(Instance);
    }
}