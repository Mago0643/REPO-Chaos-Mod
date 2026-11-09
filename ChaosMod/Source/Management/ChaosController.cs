using Lego.ChaosMod.Backend;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Lego.ChaosMod.Backend;
public sealed class ChaosController: MonoBehaviour
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

        var list = ChaosEventManager._events;
        evtRand = new( list, Mathf.FloorToInt(list.Count * 0.4f), 0.125 );
    }

    internal bool pendingPlayerDeath;
    internal bool pendingPlayerSpawned;
    internal bool pendingEnemyDespawned;
    internal bool pendingEnemyRespawned;
    internal bool pendingEnemyDeath;

    internal List<PlayerAvatar> deadPlayers = [];
    internal List<PlayerAvatar> spawnedPlayers = [];
    internal List<EnemyParent> despawnedEnemies = [];
    internal List<EnemyParent> respawnedEnemies = [];
    internal List<EnemyParent> deadEnemies = [];

    internal List<EventListData> events = [];
    internal float eventTimer;
    internal float timeScale = 1f;
    internal RandomSystem<ChaosEvent> evtRand;

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
        var array = ChaosEventManager.Events;
        if (!(eventIndex >= 0 && eventIndex < array.Count)) return;

        ChaosEvent template = array[eventIndex];
        ChaosEvent mod = template.Clone();
        if (!mod.isOnce) { 
            mod.minTimer = template.minTimer;
            mod.maxTimer = template.maxTimer;
        }
        mod.Start();
        events.Add(ChaosMod.Instance.MakeEventText(mod));
        ChaosMod.Instance.ResetText();
        mod.StartPost();

        if (ChaosMod.IsDebug) {
            Logging.Log("Got new event: " + mod.GetName());
        }
    }

    [PunRPC]
    internal void AddTimeToEventRPC(int eventIndex, float time, PhotonMessageInfo info = default)
    {
        if (!(eventIndex >= 0 && eventIndex < events.Count)) return;
        var data = events[eventIndex];
        if (data == null) {
            Logging.Error("Event index " + eventIndex + " does not exists.");
            return;
        }

        data.evt.timerSelf += time;
        data.evtTimeText.SetTime(data.evt.timerSelf);

        if (ChaosMod.IsDebug) {
            Logging.Log("Add time to event: " + data.evt.GetName());
        }
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
                int modIndex = evtRand.GetValueAsInt(ChaosEventManager.Excludes);
                ChaosEvent tempMod = evtRand.Items[modIndex];

                bool isExcludedMod = false;
                bool chanceChoosen = Random.Range(0f, 1f) <= Mathf.Clamp01(tempMod.Instance.options.chance);
                bool hasTimerDoneOrIsOnce = tempMod.isOnce || tempMod.timerSelf <= 0f;
                bool isMultiplayerOnly =  tempMod.Instance.options.multiplayerOnly;
                bool isSingleplayerOnly = tempMod.Instance.options.singleplayerOnly;
                bool excludedOptions = tempMod.enabled;
                bool type = true;
                if ((isMultiplayerOnly && !GameManager.Multiplayer()) || (isSingleplayerOnly && GameManager.Multiplayer()))
                    type = false;

                if (ChaosMod.IsDebug)
                    Logging.Log($"!isExcludedMod: {!isExcludedMod} && chanceChoosen: {chanceChoosen} && hasTimerDoneOrIsOnce: {hasTimerDoneOrIsOnce} && isMultiplayerOnly: {isMultiplayerOnly} && excludeOptions: {excludedOptions}");

                if (!isExcludedMod && chanceChoosen && hasTimerDoneOrIsOnce && type && excludedOptions)
                {
                    if (!tempMod.isOnce)
                    {
                        int index = events.FindIndex(data => data.evt.nameKey == tempMod.nameKey && !data.evt.isOnce && data.evt.timerSelf > 0f);
                        if (index != -1)
                        {
                            if (GameManager.Multiplayer())
                                view.RPC("AddTimeToEventRPC", RpcTarget.All, index, events[index].evt.GetTime());
                            else
                                AddTimeToEventRPC(index, events[index].evt.GetTime());
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
                Logging.Error($"{e.Message}\n{e.StackTrace}");
            }
            tries++;
        }

        Logging.Warn("I don't know what to choose.");
    }

    public static ChaosEvent FindModWithName(string name)
    {
        var mod = ChaosEventManager.Events.Find(m => m.GetName() == name);
        if (mod == null)
        {
            if (ChaosMod.IsDebug)
                Logging.Warn($"Event name '{name}' does not exists!");
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
            ChaosEvent mod = FindModWithName(evtName);
            if (mod != null && !mod.finished)
            {
                if (ChaosMod.IsDebug)
                    Logging.Log($"Event {mod.nameKey} Finished!");

                if (!mod.isOnce)
                    mod.OnFinished();

                if (resetText)
                    ChaosMod.Instance.ResetText();

                if (ChaosMod.IsDebug) {
                    Logging.Log("Event finished: " + mod.GetName());
                }

                /*var frame = new System.Diagnostics.StackFrame(1, true);
                Debug.LogError($"Called on {System.IO.Path.GetFileName(frame.GetFileName())}:{frame.GetFileLineNumber()}");*/
            }
        }
        catch (System.Exception e)
        {
            Logging.Error($"{e.Message}\n{e.StackTrace}");
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

    internal HashSet<int> EventsRemoveIndices = [];

    void Update()
    {
        if (!ChaosMod.Generated)
        {
            if (events.Count > 0)
            {
                foreach (var data in events)
                {
                    if (data.evt.finished) continue;

                    OnEventFinished(data.evt.GetName(), false);
                    data.DisposeTexts();
                }
                events.Clear();
            }

            if (ChaosEventManager.Excludes.Count > 0)
                ChaosEventManager.Excludes.Clear();

            // Reset Timer
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (eventTimer != ChaosMod.MaxEventTimer)
                    StartTimer(ChaosMod.MaxEventTimer);
            }

            return;
        }

        if (!ChaosMod.DISABLE_TIMER) {
            if (eventTimer > 0f) {
                eventTimer = Mathf.Max(0f, eventTimer - (Time.unscaledDeltaTime * timeScale));
            } else {
                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    StartTimer(ChaosMod.MaxEventTimer);
                    RandomEvent();
                }
            }
        }

        foreach (var data in events) {
            if (data == null) continue;

            var mod = data.evt;
            if (mod == null) continue;
            if (mod.finished) continue;
            if (mod.timerSelf > 0f) {
                mod.timerSelf -= Time.unscaledDeltaTime;
                mod.Update();

                if (pendingPlayerDeath && mod.onPlayerDead != null) { // Callback: Dead players
                    pendingPlayerDeath = false;
                    foreach (var e in deadPlayers) {
                        mod.onPlayerDead.Invoke(e);
                    }
                    deadPlayers.Clear();
                }
                if (pendingPlayerSpawned && mod.onPlayerRespawned != null) { // Callback: Spawned Players
                    pendingPlayerSpawned = false;
                    foreach (var e in spawnedPlayers) {
                        mod.onPlayerRespawned.Invoke(e);
                    }
                    spawnedPlayers.Clear();
                }
                if (pendingEnemyDeath && mod.onEnemyDeath != null) { // Callback: Enemy Death
                    pendingEnemyDeath = false;
                    foreach (var e in deadEnemies) {
                        mod.onEnemyDeath.Invoke(e);
                    }
                    deadEnemies.Clear();
                }
                if (pendingEnemyDespawned && mod.onEnemyDespawned != null) { // Callback: Enemy Despawned
                    pendingEnemyDespawned = false;
                    foreach (var e in despawnedEnemies) {
                        mod.onEnemyDespawned.Invoke(e);
                    }
                    despawnedEnemies.Clear();
                }
                if (pendingEnemyRespawned && mod.onEnemyRespawned != null) { // Callback: Enemy Respawned
                    foreach (var e in respawnedEnemies) {
                        mod.onEnemyRespawned.Invoke(e);
                    }
                    respawnedEnemies.Clear();
                }
            } else {
                if (mod.timerSelf > -10f) {
                    mod.timerSelf = -10f;
                    if (!mod.isOnce && !mod.finished)
                        OnEventFinished(mod.GetName(), true);

                    bool removeCondition = !mod.isOnce && mod.timerSelf <= 0f;
                    int index = events.FindIndex(data => data.evt == mod);
                    if (removeCondition) RequestRemoveEvent(index);
                }
            }
        }
    }

    internal Queue<int> queue_setText = new();

    void LateUpdate()
    {
        // doing this just in case...
        while (queue_setText.Count > 0) {
            var idx = queue_setText.Dequeue();
            
            var data = events[idx];
            if (data == null) continue;

            data.evtNameText.text = data.evt.GetName();
            if (data.evtTimeText != null) data.evtTimeText.hide = data.evt.options.hideTimer;
        }

        if (EventsRemoveIndices.Count > 0) {
            var indices = EventsRemoveIndices.Distinct().OrderByDescending(i => i).ToList();
            EventsRemoveIndices.Clear();

            foreach (int index in indices) {
                if (index < 0 || index >= events.Count) continue;

                var kill = events[index];
                events.RemoveAt(index);

                if (kill != null) {
                    if (!kill.evt.finished && !kill.evt.isOnce)
                        OnEventFinished(kill.evt.GetName(), true);
                    kill.DisposeTexts();
                }
            }
        }

        // Debugging text
        if (ChaosMod.IsDebug)
        {
            string text = "debugging text";
            text += $"\nAlive evnts: {events.Count}";

            DebugText.text = text;
        }
    }

    public void RequestRemoveEvent(int index)
    {
        if (EventsRemoveIndices.Contains(index)) return;
        EventsRemoveIndices.Add(index);
    }

    // -- Custom Mod Thingie --
    //IEnumerator FindCar()
    //{
    //    while (ChaosMod.Instance.carObject == null)
    //    {
    //        ChaosMod.Instance.carObject = GameObject.Find("Killer Joe(Clone)");
    //        yield return null;
    //    }

    //    var car_assets = CarCrash.car_assets;
    //    ChaosMod.Instance.car = ChaosMod.Instance.carObject.AddComponent<CrazyCarAIScript>();
    //    ChaosMod.Instance.car.honk = car_assets.LoadAsset<AudioClip>("car honk");
    //    ChaosMod.Instance.car.exp_sprites = car_assets.LoadAssetWithSubAssets<Sprite>("spr_realisticexplosion").ToList();

    //    if (ChaosMod.IsDebug)
    //        ChaosMod.Logger.LogMessage("Car Setup is done!");
    //}

    //[PunRPC]
    //public void FindCarRPC()
    //{
    //    StartCoroutine(FindCar());
    //}

    [PunRPC]
    internal void GrenadeStunExplosionRPC(int viewID)
    {
        GameObject item = PhotonView.Find(viewID).gameObject;
        var script = item.GetComponent<ItemGrenade>();
        script.tickTime = 0;
        script.isActive = true;
    }
} // public class ChaosController: MonoBehaviour