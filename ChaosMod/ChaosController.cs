using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace ChaosMod
{
    public class ChaosController: MonoBehaviour
    {
        PhotonView view;
        TextMeshProUGUI DebugText
        {
            get => ChaosMod.Instance.DebugText;
        }

        void Start()
        {
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
        void SendEventRPC(int eventIndex, PhotonMessageInfo info = default)
        {
            // if (GameManager.Multiplayer() && !info.Sender.IsMasterClient) return;
            if (!(eventIndex >= 0 && eventIndex < Modifiers.Events.Count)) return;

            Modifier template = Modifiers.Events[eventIndex];
            Modifier mod = template.Clone();
            events.Add(mod);
            mod.Start();

            ChaosMod.Instance.MakeText(mod.GetName(), mod.timerSelf);
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
                    int modIndex = Random.Range(0, Modifiers.Events.Count);
                    Modifier tempMod = Modifiers.Events[modIndex];

                    if (!Modifiers.CheckExcludes(tempMod.Instance) &&
                        Random.Range(0f, 1f) <= Mathf.Clamp01(tempMod.Instance.options.chance) &&
                        (tempMod.isOnce || tempMod.timerSelf <= 0f) &&
                        (!tempMod.Instance.options.multiplayerOnly || GameManager.Multiplayer()))
                    {
                        if (GameManager.Multiplayer())
                            view.RPC("SendEventRPC", RpcTarget.All, modIndex);
                        else
                            SendEventRPC(modIndex);

                        return;
                    }
                } catch (System.Exception e) {
                    ChaosMod.Logger.LogError(e.Message);
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
                ChaosMod.Logger.LogError(e.Message);
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
                        if (!events[i].finihshed)
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
    }
}
