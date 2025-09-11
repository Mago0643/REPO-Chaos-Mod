using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Yoga;

namespace ChaosMod
{
    public class Shop_DiscountEvent: Modifier
    { 
        public Shop_DiscountEvent(): base("세일", "파격 세일 중!")
        {
            minTimer = 30f;
            maxTimer = 60f;
            isOnce = true;
        }

        public List<int> ogValues = new List<int>();
        public List<ItemAttributes> items = new List<ItemAttributes>();
        public override void Start()
        {
            base.Start();
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            var insta = ((Shop_DiscountEvent)Instance);
            foreach (var attr in GameObject.FindObjectsByType<ItemAttributes>(0))
            {
                int ogValue = Util.GetInternalVar<int>(attr, "value");
                insta.ogValues.Add(ogValue);
                attr.GetValueRPC(Mathf.FloorToInt(ogValue * .75f));
                if (GameManager.Multiplayer())
                    attr.GetComponent<PhotonView>().RPC("GetValueRPC", RpcTarget.Others, Mathf.FloorToInt(ogValue * .75f));
                insta.items.Add(attr);
            }

            if (!Modifiers.Excludes.Contains(Instance))
                Modifiers.Excludes.Add(Instance);
        }

        public override void OnFinished()
        {
            base.OnFinished();
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            int i = 0;
            var insta = ((Shop_DiscountEvent)Instance);
            foreach (var attr in insta.items)
            {
                if (attr == null)
                {
                    i++;
                    continue;
                }

                attr.GetValueRPC(insta.ogValues[i]);
                if (GameManager.Multiplayer())
                    attr.GetComponent<PhotonView>().RPC("GetValueRPC", RpcTarget.Others, insta.ogValues[i]);
                i++;
            }
        }

        public override Modifier Clone()
        {
            return new Shop_DiscountEvent() { Instance = this, isClone = true };
        }
    }
}
