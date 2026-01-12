using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace ChaosMod
{
    public class ChaosController: MonoBehaviour
    {
        internal PhotonView view;
        public static ChaosController instance;
        TextMeshProUGUI DebugText
        {
            get => ChaosMod.Instance.DebugText;
        }

        void Start()
        {
            instance = this;
            view = GetComponent<PhotonView>();
            eventTimer = ChaosMod.MaxEventTimer;
        }

        internal List<Modifier> events = new List<Modifier>();
        internal float eventTimer;
        internal float timeScale = 1f;
        [PunRPC]
        void StartEventTimerRPC(float time, PhotonMessageInfo info = default)
        {
            // if (GameManager.Multiplayer() && !info.Sender.IsMasterClient) return;
            eventTimer = time;
        }

        void StartTimer(float time)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer())
                view.RPC("StartEventTimerRPC", RpcTarget.All, time);
            else
                StartEventTimerRPC(time);
        }

        [PunRPC]
        internal void SendEventRPC(int eventIndex, PhotonMessageInfo info = default)
        {
            // if (GameManager.Multiplayer() && !info.Sender.IsMasterClient) return;
            var array = Modifiers.Events;
            if (SemiFunc.RunIsShop())
                array = Modifiers.ShopEvents;
            if (!(eventIndex >= 0 && eventIndex < array.Count)) return;

            Modifier template = array[eventIndex];
            Modifier mod = template.Clone();
            events.Add(mod);
            mod.Start();

            ChaosMod.Instance.MakeText(mod.GetName(), mod.timerSelf);
        }

        [PunRPC]
        void AddTimeToEventRPC(int eventIndex, float time, PhotonMessageInfo info = default)
        {
            if (!(eventIndex >= 0 && eventIndex < events.Count)) return;

            events[eventIndex].timerSelf += time;
            if (ChaosMod.Instance.eventTimerBars[eventIndex] != null)
                ChaosMod.Instance.eventTimerBars[eventIndex].SetTime(events[eventIndex].timerSelf);
        }

        void RandomEvent()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            int tries = 0;
            const int maxTries = 100;

            while (tries < maxTries)
            {
                try
                {
                    int modIndex = 0;
                    Modifier tempMod = null;
                    if (SemiFunc.RunIsLevel())
                    {
                        modIndex = Random.Range(0, Modifiers.Events.Count);
                        tempMod = Modifiers.Events[modIndex];
                    } /*else if (SemiFunc.RunIsShop()) {
                        modIndex = Random.Range(0, Modifiers.ShopEvents.Count);
                        tempMod = Modifiers.ShopEvents[modIndex];
                    }*/

                    bool isExcludedMod = Modifiers.CheckExcludes(tempMod.Instance);
                    bool chanceChoosen = Random.Range(0f, 1f) <= Mathf.Clamp01(tempMod.Instance.options.chance);
                    bool hasTimerDoneOrIsOnce = tempMod.isOnce || tempMod.timerSelf <= 0f;
                    bool isMultiplayerOnly =  tempMod.Instance.options.multiplayerOnly;
                    bool isSingleplayerOnly = tempMod.Instance.options.singleplayerOnly;
                    bool excludedOptions = ChaosMod.Instance.Exclude_Modifiers[tempMod.GetName()];
                    bool type = true;
                    if ((isMultiplayerOnly && !GameManager.Multiplayer()) || (isSingleplayerOnly && GameManager.Multiplayer()))
                        type = false;

                    if (ChaosMod.IsDebug)
                        print($"!isExcludedMod: {!isExcludedMod} && chanceChoosen: {chanceChoosen} && hasTimerDoneOrIsOnce: {hasTimerDoneOrIsOnce} && isMultiplayerOnly: {isMultiplayerOnly} && !excludeOptions: {!excludedOptions}");

                    if (!isExcludedMod && chanceChoosen && hasTimerDoneOrIsOnce && type && !excludedOptions)
                    {
                        if (!tempMod.isOnce)
                        {
                            Modifier @event = events.Find(mod => mod.name == tempMod.name && !mod.isOnce && mod.timerSelf > 0f);
                            if (@event != null)
                            {
                                if (GameManager.Multiplayer())
                                    view.RPC("AddTimeToEventRPC", RpcTarget.All, events.IndexOf(@event), @event.GetTime());
                                else
                                    AddTimeToEventRPC(events.IndexOf(@event), @event.GetTime());
                                return;
                            }
                        }

                        if (GameManager.Multiplayer())
                            view.RPC("SendEventRPC", RpcTarget.All, modIndex);
                        else
                            SendEventRPC(modIndex);

                        return;
                    }
                } catch (System.Exception e) {
                    ChaosMod.Logger.LogError($"{e.Message}\n{e.StackTrace}");
                }
                tries++;
            }

            ChaosMod.Logger.LogWarning("적절한 이벤트를 찾지 못했습니다.");
        }

        public static Modifier FindModWithName(string name)
        {
            var mod = Modifiers.Events.Find(m => m.GetName() == name);
            if (mod == null)
            {
                if (ChaosMod.IsDebug)
                    ChaosMod.Logger.LogWarning($"이벤트 이름 '{name}'은 존재하지 않습니다.");
                return null;
            }
            return mod.Clone();
        }

        [PunRPC]
        void OnEventFinishedRPC(string evtName, bool resetText = true, PhotonMessageInfo info = default)
        {
            // if (GameManager.Multiplayer() && !info.Sender.IsMasterClient) return;

            try
            {
                Modifier mod = FindModWithName(evtName);
                if (mod != null)
                {
                    if (ChaosMod.IsDebug)
                        ChaosMod.Logger.LogMessage("모드 마침: " + mod.name);

                    if (!mod.isOnce)
                        mod.OnFinished();

                    if (resetText)
                        ChaosMod.Instance.ResetText();
                }
            }
            catch (System.Exception e)
            {
                ChaosMod.Logger.LogError($"{e.Message}\n{e.StackTrace}");
            }
        }

        void OnEventFinished(string modName, bool resetText)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (GameManager.Multiplayer())
                view.RPC("OnEventFinishedRPC", RpcTarget.All, modName, resetText);
            else
                OnEventFinishedRPC(modName, resetText);
        }

        void Update()
        {
            if (!ChaosMod.Generated)
            {
                if (events.Count > 0)
                {
                    foreach (var evt in events)
                    {
                        OnEventFinished(evt.GetName(), false);
                    }
                    events.Clear();
                }

                if (Modifiers.Excludes.Count > 0)
                    Modifiers.Excludes.Clear();

                // 타이머 리셋
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    if (eventTimer != ChaosMod.MaxEventTimer)
                        StartTimer(ChaosMod.MaxEventTimer);
                }

                return;
            }

            if (!ChaosMod.DISABLE_TIMER)
            {
                if (eventTimer > 0f)
                {
                    eventTimer = Mathf.Max(0f, eventTimer - (Time.unscaledDeltaTime * timeScale));
                }
                else
                {
                    if (SemiFunc.IsMasterClientOrSingleplayer())
                    {
                        StartTimer(ChaosMod.MaxEventTimer);
                        RandomEvent();
                    }
                }
            }

            foreach (Modifier mod in events)
            {
                if (mod == null) continue;
                if (mod.timerSelf > 0f)
                    mod.timerSelf -= Time.unscaledDeltaTime;
                else
                {
                    if (mod.timerSelf > -10f)
                    {
                        mod.timerSelf = -10f;
                        if (!mod.isOnce)
                            OnEventFinished(mod.GetName(), true);

                        bool removeCondition = !mod.isOnce && mod.timerSelf <= 0f;
                        int index = events.IndexOf(mod);
                        if (!ChaosMod.Instance.EventToRemove.Contains(index) && removeCondition)
                            ChaosMod.Instance.EventToRemove.Add(index);
                        if (!ChaosMod.Instance.TextToRemove.Contains(index) && removeCondition)
                            ChaosMod.Instance.TextToRemove.Add(index);
                    }
                }

                mod.Update();
            }

            // 디버깅 텍스트
            if (ChaosMod.IsDebug)
            {
                string text = "디버깅 전용 텍스트";
                text += $"\n살아있는 이벤트 개수: {events.Count}";
                text += $"\n지울 예정인 이벤트 개수: {ChaosMod.Instance.EventToRemove.Count}";
                text += $"\n살아있는 텍스트 개수: {ChaosMod.Instance.texts.Count}";
                text += $"\n지울 예정인 텍스트 개수: {ChaosMod.Instance.TextToRemove.Count}";

                DebugText.text = text;
            }

            if (ChaosMod.Instance.EventToRemove.Count > 0)
            {
                var toRemoveSet = ChaosMod.Instance.EventToRemove.Distinct().OrderByDescending(i => i);
                foreach (int i in toRemoveSet)
                {
                    if (i >= 0 && i < events.Count)
                    {
                        if (!events[i].finihshed && !events[i].isOnce)
                            OnEventFinished(events[i].GetName(), false);

                        if (ChaosMod.IsDebug)
                            ChaosMod.Logger.LogMessage($"이벤트 삭제됨: {events[i].name}");

                        events.RemoveAt(i);
                        ChaosMod.Instance.ResetText();
                    }
                }
                ChaosMod.Instance.EventToRemove.Clear();
            }
        }

        // -- Custom Mod Thingie --
        IEnumerator FindCar()
        {
            while (ChaosMod.Instance.carObject == null)
            {
                ChaosMod.Instance.carObject = GameObject.Find("Killer Joe(Clone)");
                yield return null;
            }

            var car_assets = CarCrash.car_assets;
            ChaosMod.Instance.car = ChaosMod.Instance.carObject.AddComponent<CrazyCarAIScript>();
            ChaosMod.Instance.car.honk = car_assets.LoadAsset<AudioClip>("car honk");
            ChaosMod.Instance.car.exp_sprites = car_assets.LoadAssetWithSubAssets<Sprite>("spr_realisticexplosion").ToList();

            if (ChaosMod.IsDebug)
                ChaosMod.Logger.LogMessage("Car Setup is done!");
        }

        [PunRPC]
        public void FindCarRPC()
        {
            StartCoroutine(FindCar());
        }

        [PunRPC]
        internal void GrenadeStunExplosionRPC(int viewID)
        {
            GameObject item = PhotonView.Find(viewID).gameObject;
            var script = item.GetComponent<ItemGrenade>();
            script.tickTime = 0;
            Util.GetInternalVar(script, "isActive").SetValue(script, true);
        }
    } // public class ChaosController: MonoBehaviour
} // namespace ChaosMod
