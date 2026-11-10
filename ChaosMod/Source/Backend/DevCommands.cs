using Photon.Pun;
using REPOLib.Modules;
using System.Collections.Generic;
using UnityEngine;
using ChatCommand = DebugCommandHandler.ChatCommand;
using Lego.ChaosMod.Events;

namespace Lego.ChaosMod.Backend;
internal static class DevCommands
{
    static ChaosController cont
    {
        get => ChaosMod.Instance.controller;
    }

    static bool registered = false;

    internal static void Register() {
        if (registered) {
            return;
        }
        registered = true;

        List<ChatCommand> cmds = new() {
            new ChatCommand("runchaos", "(Chaos Mod) Executes an speific event by key name.", Event_Execute, Event_Suggest, DefaultEnabled)
        };

        foreach (ChatCommand cmd in cmds) {
            Commands.RegisterCommand(cmd);
            Logging.Log($"Register Command: {cmd.Name}");
        }
    }

    static void Event_Execute(bool isDebugConsole, string[] args)
    {
        if (args.Length == 0) {
            DebugConsoleUI.instance?.SetResponseText("Failed to execute command: You must provide mod's key to execute.", Color.red);
            DebugCommandHandler.instance?.CommandFailedEffect();
            return;
        }

        string keyName = args[0];
        int evtIdx = ChaosEventManager.Events.IndexOf(ChaosEventManager.Events.Find(mod => mod.nameKey == keyName));
        if (evtIdx != -1) {
            var events = ChaosController.instance.events;
            int hasIdx = events.FindIndex(entry => entry.evt.nameKey == keyName && !entry.evt.isOnce && entry.evt.timerSelf > 0f);
            if (hasIdx != -1) {
                if (GameManager.Multiplayer())
                    ChaosController.instance.view.RPC("AddTimeToEventRPC", RpcTarget.All, evtIdx, events[hasIdx].evt.GetTime());
                else
                    ChaosController.instance.AddTimeToEventRPC(evtIdx, events[hasIdx].evt.GetTime());
            } else {
                if (GameManager.Multiplayer())
                    ChaosController.instance.view.RPC("SendEventRPC", RpcTarget.All, evtIdx);
                else
                    ChaosController.instance.SendEventRPC(evtIdx);
            }
        } else {
            DebugConsoleUI.instance?.SetResponseText($"Failed to run chaos event: Could not find {keyName}.", Color.red);
            DebugCommandHandler.instance?.CommandFailedEffect();
            return;
        }
        DebugConsoleUI.instance?.SetResponseText($"Sucessfully executed {ChaosEventManager.Events[evtIdx].GetName()}!", Color.green);
        DebugCommandHandler.instance?.CommandSuccessEffect();
    }
    static List<string> Event_Suggest(bool isDebugConsole, string partial, string[] args)
    {
        List<string> list = [];
        if (args.Length == 1) {
            foreach (var entry in ChaosEventManager.EventDictionary) {
                list.Add(entry.Key);
            }
        }
        return list;
    }

    static List<string> DefaultSuggest(bool isDebugConsole, string partial, string[] args) => [];
    static bool DefaultEnabled() => SemiFunc.RunIsLevel() && ChaosMod.IsDebug && (SemiFunc.IsMasterClient() || !SemiFunc.IsMultiplayer());
}