using System;
using UnityEngine;

namespace Lego.ChaosMod.Backend;
public class EventOptions
{
    /// <summary>
    /// The probability that the event will occur. (0 ~ 1 range)
    /// </summary>
    public float chance = 1f;

    /// <summary>
    /// Whether the event should run only once per level.
    /// </summary>
    public bool oncePerLevel = false;

    /// <summary>
    /// Whether the event should run only in multiplayer.
    /// </summary>
    public bool multiplayerOnly = false;

    /// <summary>
    /// Whether the event should run only in singleplayer.
    /// </summary>
    public bool singleplayerOnly = false;

    /// <summary>
    /// Whether the event should not display time text. <para>
    /// This is useful for making fake once events.    </para>
    /// If you're changing this after text is created, call <c>ChaosEvent.ForceUpdateName</c> to update it manually.
    /// </summary>
    public bool hideTimer = false;

    public EventOptions(float chance = 1f, bool oncePerLevel = false, bool multiplayerOnly = false, bool singleplayerOnly = false, bool hideTimer = false)
    {
        this.chance = Mathf.Clamp01(chance);
        this.oncePerLevel = oncePerLevel;
        this.multiplayerOnly = multiplayerOnly;
        this.singleplayerOnly = singleplayerOnly;
        this.hideTimer = hideTimer;
    }
}

[Obsolete("Modifier is deprecated, use ChaosEvent instead.")]
public class Modifier: ChaosEvent { public Modifier(string nameKey, string descriptionKey): base(nameKey, descriptionKey) {} }

public class ChaosEvent
{
    /// <summary>
    /// Original instance of this event.
    /// </summary>
    public ChaosEvent Instance { get; set; }

    /// <summary>
    /// The instance of <c>ChaosController</c>.
    /// </summary>
    protected ChaosController controller => ChaosController.instance;

    /// <summary>
    /// Whether this event is a replica.
    /// </summary>
    public bool isClone = false;

    /// <summary>
    /// Whether this event has ended.
    /// </summary>
    public bool finished = false;

    /// <summary>
    /// Whether this event is of the one-time trigger type.
    /// </summary>
    public bool isOnce = false;

    /// <summary>
    /// The maximum time value for this event.
    /// </summary>
    public float maxTimer = 10f;
    
    /// <summary>
    /// The minimum time value for this event.
    /// </summary>
    public float minTimer = 9f;

    /// <summary>
    /// Time remaining for this event.
    /// </summary>
    public float timerSelf = 0f;

    /// <summary>
    /// The name of this event.
    /// <para>
    /// <b>This gives you the language key.</b> to get real name of event, call <see cref="GetName"/> instead.
    /// </para>
    /// </summary>
    public string nameKey = "";

    /// <summary>
    /// The option of this event.
    /// </summary>
    public EventOptions options;

    /// <summary>
    /// The description of this event.
    /// <para>
    /// <b>This gives you the language key.</b> to get real description of event, call <see cref="GetDesc"/> instead.
    /// </para>
    /// </summary>
    public string descriptionKey = "";

    /// <summary>
    /// Whether this event is running.
    /// </summary>
    public bool isRunning { get => timerSelf > 0f; }

    /// <summary>
    /// The special ID of this event.
    /// </summary>
    public int ID = 0;

    /// <summary>
    /// Whether this event runs. (for options)
    /// </summary>
    public bool enabled = true;

    /// <summary>
    /// Fired when players revived.
    /// </summary>
    public Action<PlayerAvatar> onPlayerRespawned;

    /// <summary>
    /// Fired when player is dead.
    /// </summary>
    public Action<PlayerAvatar> onPlayerDead;

    /// <summary>
    /// Fired when enemy is despawned.
    /// </summary>
    public Action<EnemyParent> onEnemyDespawned;

    /// <summary>
    /// Fired when enemy is respawned.
    /// </summary>
    public Action<EnemyParent> onEnemyRespawned;

    /// <summary>
    /// Fired when enemy is dead.
    /// </summary>
    public Action<EnemyParent> onEnemyDeath;

    public ChaosEvent(string nameKey, string descriptionKey)
    {
        this.nameKey = nameKey;
        this.descriptionKey = descriptionKey;
        options = new EventOptions();
        if (!isClone)
            Instance = this;
    }

    /// <summary>
    /// Called when this event starts.
    /// <para>
    /// <c>controller.events</c> does not have this event at this time.
    /// </para>
    /// </summary>
    public virtual void Start()
    {
        timerSelf = GetTime();

        if (ChaosMod.IsDebug)
            Logging.Log("New Event: " + GetName());

        if (options.oncePerLevel)
        {
            if (!ChaosEventManager.Excludes.Contains(this))
                ChaosEventManager.Excludes.Add(this);
        }
    }

    /// <summary>
    /// Called when chaos mod is making UI objects. (Runs when the mod is loading) <para>
    /// When you need to add some assets for this event, this is the place.</para>
    /// <paramref name="rect"/> is layered under the overlay texts. <para>
    /// Also remember that objects that you create here keep remains while game is keep running, so clean up those objects in <see cref="OnLeave"/> if needed.</para>
    /// </summary>
    /// <param name="rect">RectTransform of chaos mod's canvas.</param>
    public virtual void OnCreate(RectTransform rect) {}

    /// <summary>
    /// Same as <see cref="OnCreate"/> function, but this runs when player started game.
    /// </summary>
    /// <param name="rect">RectTransform of chaos mod's canvas.</param>
    public virtual void OnStartGame(RectTransform rect) {}

    /// <summary>
    /// Called when player leaved current room. <para>
    /// If you need to clean up some objects that made on <see cref="OnCreate"/>, this is the place.</para>
    /// </summary>
    public virtual void OnLeave() {}

    /// <summary>
    /// Called when this event starts, but late.
    /// <para>
    /// You can find this event at <c>controller.events</c> in this method.
    /// </para>
    /// </summary>
    public virtual void StartPost() {}

    /// <summary>
    /// Returns the event's validity period.
    /// </summary>
    public virtual float GetTime()
    {
        if (isOnce) return 0;
        return UnityEngine.Random.Range(minTimer, maxTimer);
    }

    /// <summary>
    /// Returns this event's real name.
    /// </summary>
    public virtual string GetName()
        => Language.GetText(nameKey);

    /// <summary>
    /// Returns this event's real description.
    /// </summary>
    public virtual string GetDesc()
        => Language.GetText(descriptionKey);

    /// <summary>
    /// Forces event text to update its name. <para>
    /// (also updates text for <see cref="options.hideTimer"/>) </para>
    /// </summary>
    public void ForceUpdateName() {
        if (controller == null) return;
        
        int index = controller.events.FindIndex(data => data.evt.nameKey == nameKey);
        if (index == -1) {
            Logging.Warn($"ForceUpdateName | Cannot find event name with key: {nameKey}");
            return;
        }

        ChaosMod.Instance.SetEventText(index);
    }
    
    /// <summary>
    /// Fired on every frame while event is active.
    /// </summary>
    public virtual void Update() {}

    /// <summary>
    /// Called when event is finished.
    /// </summary>
    public virtual void OnFinished()
    {
        finished = true;
    }

    /// <summary>
    /// Clones this event.
    /// </summary>
    public ChaosEvent Clone()
    {
        var copy = (ChaosEvent)MemberwiseClone();
        copy.isClone = true;
        copy.Instance = this;
        
        return Instance;
    }

    /// <summary>
    /// Called when cloned this object. <para>
    /// Do it here if you need to make new references. such as List, Dictionary, Some other classes. </para>
    /// </summary>
    public virtual void OnCloned() {}
}