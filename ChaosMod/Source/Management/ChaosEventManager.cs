using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Lego.ChaosMod.Events;

namespace Lego.ChaosMod.Backend;

[Obsolete("Modifiers is deprecated, use ChaosEventManager instead.", true)]
public sealed class Modifiers {}
public sealed class ChaosEventManager {
    internal static readonly List<ChaosEvent> _events = [];
    internal static readonly Dictionary<string, ChaosEvent> _eventDick = [];

    /// <summary>
    /// All registered events goes here.
    /// </summary>
    public static IReadOnlyList<ChaosEvent> Events => _events.AsReadOnly();
    /// <summary>
    /// Dictionary that has <c>nameKey</c> as TKey, <c>ChaosEvent</c> as TValue.
    /// </summary>
    public static ReadOnlyDictionary<string, ChaosEvent> EventDictionary => new(_eventDick);

    private static readonly List<ChaosEvent> QueuedEvents = [];
    private static bool cannotAddEvents = false;
    internal static void Init(Action<ChaosEvent> callback = null!)
    {
        object[][] DefaultEventTypes = [ // [event_type_name, constructor_parameters, ...]
            ["NothingEvent"], ["TumbleEvent"], ["KillAllMonsterEvent"], ["GameSpeedEvent", new float[] { 2f, .75f }],
            ["BreakAllDoorEvent"], ["PlayMonsterSoundEvent"], ["AddHealthEvent", new int[] { 100, 50, 0, -25 }],
            ["HideHealthUIEvent"], ["HideSprintUIEvent"], ["JointAreDoorsEvent"], ["DuckMadnessEvent"],
            ["SayRandomThingsEvent"], ["ShakeScreenEvent"], ["IncreaseValuableWorthEvent", new float[] { 2f, .9f }],
            ["SpinEternallyEvent"], ["FriendlyMonstersEvent"], ["VoicePitchEvent", new float[] {0.75f, 1.5f}], ["DisablePostProcessingEvent"],
            ["InvincibleOrbEvent"], ["ReviveAllPlayersEvent"], ["NoJumpAndSprintEvent"], ["EyesBigEvent"], ["DisableLightingEvent"],
            ["SpawnMonsterEvent", new int[] { 2, 4 }], ["NoGravityEvent"], ["ThinkFastEvent"], ["ClubMoodEvent"],
            ["GravityMultEvent", new float[] { .25f, 5f }], ["TimeMultEvent", new float[] { 2f, .5f, 4f }], ["RandomItemsEvent"],
            ["RandomUpgradeEvent"], ["InfiniteStaminaEvent"], ["InvincibleEvent"], ["InfiniteJumpEvent"], ["DidYouSeeThatEvent"],
            ["ForcePauseEvent"]
        ];

        foreach (var entry in DefaultEventTypes) {
            string tName = (string)entry[0];
            Type t = typeof(NothingEvent).Assembly.GetTypes().First(t => t.Name == tName);
            object[] args = entry.Skip(1).ToArray();
            QueuedEvents.Add((ChaosEvent)Activator.CreateInstance(t, args)!);
        }

        if (ChaosMod.IsDebug)
            Logging.Log("There's currently " + QueuedEvents.Count + " events active.");

        foreach (ChaosEvent mod in QueuedEvents)
        {
            if (callback != null) callback(mod);
            _events.Add(mod);
            _eventDick.Add(mod.nameKey, mod);
        }
        QueuedEvents.Clear();

        cannotAddEvents = true;
    }

    // I'm planning to make other peoples can make their unique events.
    public static void Register(ChaosEvent mod)
    {
        if (cannotAddEvents) {
            Logging.Error("You cannot add a Modifier in runtime. This function only works on Awake().");
            return;
        }
        QueuedEvents.Add(mod);
    }

    internal static readonly List<ChaosEvent> Excludes = new List<ChaosEvent>();
    public static bool CheckExcludes(ChaosEvent mod)
    {
        List<ChaosEvent> list = Excludes.FindAll(e =>
        {
            return e.GetName() == mod.GetName();
        });
        return list.Count > 0;
    }
}