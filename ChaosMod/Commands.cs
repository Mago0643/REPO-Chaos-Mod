using REPOLib.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace ChaosMod;
public static class Commands
{
    static ChaosController cont
    {
        get => ChaosMod.Instance.controller;
    }

    // i think they removed the commands?? i'm not sure why
    // this will be added again if they restore it
#if false

    [CommandExecution(
        "Revive",
        "Revive All Players",
        enabledByDefault: true,
        requiresDeveloperMode: true
    )]
    [CommandAlias("reviveall")]
    [CommandAlias("ra")]
    public static void ReviveAllPlayers(string args)
    {
        if (!ChaosMod.IsDebug) return;
        int evtIndex = Modifiers.Events.IndexOf(Modifiers.Events.Find(mod => mod is ReviveAllPlayers));
        if (evtIndex != -1)
        {
            cont.view.RPC("SendEventRPC", Photon.Pun.RpcTarget.All, evtIndex);
        }
        else ChaosMod.Logger.LogError("evtIndex is Null!");
    }

    [CommandExecution(
        "Spawn Car",
        "Spawns a car",
        enabledByDefault: true,
        requiresDeveloperMode: true
    )]
    [CommandAlias("spawncar")]
    [CommandAlias("sc")]
    public static void SpawnCar(string args)
    {
        if (!ChaosMod.IsDebug) return;
        int evtIndex = Modifiers.Events.IndexOf(Modifiers.Events.Find(mod => mod is CarCrash));
        if (evtIndex != -1)
        {
            if (GameManager.Multiplayer())
                cont.view.RPC("SendEventRPC", Photon.Pun.RpcTarget.All, evtIndex);
            else
                cont.SendEventRPC(evtIndex);
        }
        else ChaosMod.Logger.LogError("evtIndex is Null!");
    }

    [CommandExecution(
        "Low Pitch",
        "Lows the pitch of voices",
        enabledByDefault: true,
        requiresDeveloperMode: true
    )]
    [CommandAlias("lowpitch")]
    [CommandAlias("lp")]
    public static void LowPitch(string args)
    {
        if (!ChaosMod.IsDebug) return;
        int evtIndex = Modifiers.Events.IndexOf(Modifiers.Events.Find(mod => mod is VoicePitch && ((VoicePitch)mod).pitch < 1));
        if (evtIndex != -1)
        {
            cont.view.RPC("SendEventRPC", Photon.Pun.RpcTarget.All, evtIndex);
        }
        else ChaosMod.Logger.LogError("evtIndex is Null!");
    }

    [CommandExecution(
        "High Pitch",
        "Highs the pitch of voices",
        enabledByDefault: true,
        requiresDeveloperMode: true
    )]
    [CommandAlias("highpitch")]
    [CommandAlias("hp")]
    public static void HighPitch(string args)
    {
        if (!ChaosMod.IsDebug) return;
        int evtIndex = Modifiers.Events.IndexOf(Modifiers.Events.Find(mod => mod is VoicePitch && ((VoicePitch)mod).pitch > 1));
        if (evtIndex != -1)
        {
            cont.view.RPC("SendEventRPC", Photon.Pun.RpcTarget.All, evtIndex);
        }
        else ChaosMod.Logger.LogError("evtIndex is Null!");
    }

    [CommandExecution(
        "Show AD",
        "Shows the AD",
        enabledByDefault: true,
        requiresDeveloperMode: true
    )]
    [CommandAlias("sa")]
    [CommandAlias("showad")]
    public static void ShowAD(string args)
    {
        /*if (!ChaosMod.IsDebug) return;
        int evtIndex = Modifiers.Events.IndexOf(Modifiers.Events.Find(mod => mod is ShowAD));
        if (evtIndex != -1)
        {
            cont.view.RPC("SendEventRPC", Photon.Pun.RpcTarget.All, evtIndex);
        }
        else ChaosMod.Logger.LogError("evtIndex is Null!");*/
    }

    [CommandExecution(
        "Flashback",
        "Scout TF2 Flashbacks you!",
        enabledByDefault: true,
        requiresDeveloperMode: true
    )]
    [CommandAlias("thinkfast")]
    [CommandAlias("tf")]
    [CommandAlias("fb"), CommandAlias("flashback")]
    public static void ThinkFastNuts(string args)
    {
        if (!ChaosMod.IsDebug) return;
        int evtIndex = Modifiers.Events.IndexOf(Modifiers.Events.Find(mod => mod is ThinkFast));
        if (evtIndex != -1) {
            cont.view.RPC("SendEventRPC", Photon.Pun.RpcTarget.All, evtIndex);
        }
        else ChaosMod.Logger.LogError("evtIndex is Null!");
    }

    [CommandExecution(
        "Force Pause",
        "Forces show pause menu.",
        enabledByDefault: true,
        requiresDeveloperMode: true
    )]
    [CommandAlias("fp")]
    [CommandAlias("pause")]
    public static void ForcePause(string args)
    {
        if (!ChaosMod.IsDebug) return;
        int evtIndex = Modifiers.Events.IndexOf(Modifiers.Events.Find(mod => mod is ForcePause));
        if (evtIndex != -1)
        {
            cont.view.RPC("SendEventRPC", Photon.Pun.RpcTarget.All, evtIndex);
        }
        else ChaosMod.Logger.LogError("evtIndex is Null!");
    }
#endif
}