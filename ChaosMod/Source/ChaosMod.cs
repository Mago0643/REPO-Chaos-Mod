using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Lego.ChaosMod.Backend;
using Lego.ChaosMod.Events;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
// using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using OptionItem = Lego.ChaosMod.Backend.OptionsMenuController.OptionItem;
using OptionType = Lego.ChaosMod.Backend.OptionsMenuController.OptionType;
using PInfo = Lego.ChaosMod.Backend.PluginInfo;

namespace Lego.ChaosMod;
[BepInPlugin(PInfo.GUID, PInfo.Name, PInfo.Version)]
[BepInDependency("nickklmao.menulib", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("REPOLib", BepInDependency.DependencyFlags.HardDependency)]
public sealed class ChaosMod: BaseUnityPlugin {
    internal static ChaosMod Instance { get; private set; } = null!;
    internal static bool IsDebug {
        get {
            // change this true to experience the "real" debug mode.
            bool enable = true;
            return Instance.DevMode || enable;
        }
    }
    internal static bool DISABLE_TIMER = false;
    internal static float MaxEventTimer = 20f;
    /// <summary>
    /// This list contains enemy who is not despawned. (if despawns removes from the list automatically)
    /// </summary>
    internal List<EnemyParent> spawnedEnemys = new();
    internal new static ManualLogSource Logger => Instance._logger;
    private ManualLogSource _logger => base.Logger;
    internal Harmony? Harmony { get; set; }
    internal bool DevMode = false;

    internal Canvas? UICanvas;
    internal RectTransform? barRect;

    internal PhotonView? view;

    internal AudioSource? EnemyAS;
    internal AudioSource? RumbleAS;
    internal AudioSource? AudioSource;
    internal AudioSource? ClubSource;

    // internal PostProcessVolume? shaderOverlay;

    internal ConfigEntry<KeyCode>? ConfigOptionsMenu;

    internal List<GameObject> PrefabToAddNetwork = new();

    internal TMP_SpriteAsset? eyesFont;
    internal UnityEvent<RunManager.ChangeLevelType> OnLevelChange = new();
    
    private void Awake() {
        Instance = this;

        // Prevent the plugin from being deleted
        gameObject.transform.parent = null;
        gameObject.hideFlags = HideFlags.HideAndDontSave;

        Language.Load(CultureInfo.CurrentCulture.Name == "ko-KR" ? "ko" : "en-us");
        ConfigOptionsMenu =    Config.Bind<KeyCode>("General", "Option Key",  KeyCode.F4, "AAAAAAAAAAAAAAAAAA");

        Logging.Init();
        Application.quitting += Logging.CleanUp;

        Patch();
        Util.MakeASCIIColorsWork();
        DevCommands.Register();
        CONFIG_PATH = Util.GetPluginDirectory("CHAOS_MOD.config");
        Logging.Log($"<b>Chaos mod has been activated.</b>");
    }

    internal void Patch()
    {
        Harmony ??= new Harmony(Info.Metadata.GUID);
        Harmony.PatchAll();
    }

    internal void Unpatch()
    {
        Harmony?.UnpatchSelf();
    }

    public static bool Generated
    {
        get
        {
            // return LevelGenerator.Instance.Generated && !SemiFunc.IsMainMenu() && (SemiFunc.RunIsLevel() || SemiFunc.RunIsShop()) && !SemiFunc.RunIsArena();
            return LevelGenerator.Instance.Generated && !SemiFunc.IsMainMenu() && SemiFunc.RunIsLevel() && !SemiFunc.RunIsArena();
        }
    }

    float canvasHeight = 0f;
    float canvasWidth = 0f;
    internal GameObject? timeBar;
    internal RectTransform? textGroup;
    internal AssetBundle? assets;
    internal TextMeshProUGUI? DebugText;
    internal Image? barImg;
    internal OptionsMenuController? options;
    internal RectTransform? EventTransform;
    void Start()
    {
        // Assetbundle 로드
        string path = Util.GetPluginDirectory("assets");
        Logging.Log($"Loading asset bundle at '{path}'");
        assets = AssetBundle.LoadFromFile(path);
        if (assets == null)
            Logging.Error("Failed to load asset bundle. Fonts and some assets will be seen broken.");
        else {
            pretendard = assets.LoadAsset<TMP_FontAsset>("Pretendard-Bold SDF");
            if (pretendard == null)
                Logging.Error("Failed to load font. This is not good.");
        }

        eyesFont = assets!.LoadAsset<TMP_SpriteAsset>("1f440");

        GameObject UIPrefab = Instantiate(assets.LoadAsset<GameObject>("Chaos Controller"));
        UICanvas = UIPrefab.GetComponent<Canvas>();

        // if i can figure this out, i can make many effect such like grayscale, tiled screen, perspective effect.
        // shaderOverlay = canvas.AddComponent<PostProcessVolume>();
        /*shaderOverlay.isGlobal = true;
        shaderOverlay.priority = 1f;
        shaderOverlay.sharedProfile = ScriptableObject.CreateInstance<PostProcessProfile>();

        var profile = shaderOverlay.sharedProfile;
        if (!profile.HasSettings<RainbowEffect>())
        {
            var rainbow = ScriptableObject.CreateInstance<RainbowEffect>();
            rainbow.intensity.Override(0.25f);
            profile.AddSettings(rainbow);
        }*/

        GameObject EventObject = new("Chaos Event Object", typeof(RectTransform));
        EventObject.transform.SetParent(UICanvas.transform);
        Util.RectTransformFullscreen((RectTransform)EventObject.transform);

        EnemyAS = UIPrefab.transform.Find("Audio Source").Find("Enemy Sound Source").GetComponent<AudioSource>();
        EnemyAS.outputAudioMixerGroup = AudioManager.instance.SoundMasterGroup;

        RumbleAS = UIPrefab.transform.Find("Audio Source").Find("Rumble Sound Source").GetComponent<AudioSource>();
        RumbleAS.outputAudioMixerGroup = AudioManager.instance.SoundMasterGroup;

        AudioSource = UIPrefab.transform.Find("Audio Source").Find("Master Sound Source").GetComponent<AudioSource>();
        AudioSource.outputAudioMixerGroup = AudioManager.instance.SoundMasterGroup;

        ClubSource = UIPrefab.transform.Find("Audio Source").Find("Club Music Source").GetComponent<AudioSource>();
        ClubSource.outputAudioMixerGroup = AudioManager.instance.MusicMasterGroup;

        GameObject boombox = Instantiate(Resources.Load<GameObject>("valuables/04 big/Valuable Museum Boombox"), new Vector3(9e9f, 0f, 0f), Quaternion.identity);
        boombox.SetActive(false);
        var boom = boombox.GetComponent<ValuableBoombox>();
        ClubSource.clip = boom.soundBoomboxMusic.Sounds[0];
        Destroy(boombox);

        canvasHeight = ((RectTransform)UICanvas.transform).rect.height;
        canvasWidth = ((RectTransform)UICanvas.transform).rect.width;
        DontDestroyOnLoad(UICanvas);

        ThinkFastEvent.scout = UIPrefab.transform.Find("Scout TF2").GetComponent<Image>();
        ThinkFastEvent.text1 = UIPrefab.transform.Find("Upper Text").gameObject;
        ThinkFastEvent.text2 = UIPrefab.transform.Find("Lower Text").gameObject;

        // ThinkFast.flash = flashObj.AddComponent<Image>();
        // ThinkFast.flash.color = new Color(1f, 1f, 1f, 0f);

        timeBar = UIPrefab.transform.Find("Time Bar").gameObject;

        barImg = timeBar.transform.Find("FG").GetComponent<Image>();
        // barImg.color = new Color(ConfigBarColorR.Value / 255f, ConfigBarColorG.Value / 255f, ConfigBarColorB.Value / 255f, 1f);

        barRect = (RectTransform)barImg.transform;

        GameObject r = new("Chaos Event Objects", typeof(RectTransform));
        r.transform.SetParent(UIPrefab.transform, false);
        EventTransform = (RectTransform)r.transform;

        textGroup = (RectTransform)UIPrefab.transform.Find("Modifier Text Group").transform;

        timeBar.SetActive(false);
        ThinkFastEvent.scout.gameObject.SetActive(false);
        ThinkFastEvent.text1.SetActive(false);
        ThinkFastEvent.text2.SetActive(false);

        ThinkFastEvent.text1.GetComponent<TextMeshProUGUI>().text = Language.GetText("evt_text_think_fast");
        ThinkFastEvent.text2.GetComponent<TextMeshProUGUI>().text = Language.GetText("evt_text_chucklenuts");

        SceneManager.sceneLoaded += OnSceneChange;
        photonViewInited = false;

        DebugText = UIPrefab.transform.Find("Debug Text").GetComponent<TextMeshProUGUI>();
        if (IsDebug)
        {
            DebugText.font = pretendard;
            DebugText.text = "";
            DebugText.fontSize = 20;
        }
        DebugText.gameObject.SetActive(IsDebug);

        // fuck.....
        GameObject settings = Instantiate(assets.LoadAsset<GameObject>("Settings"), UICanvas.transform, false);
        options = settings.AddComponent<OptionsMenuController>();
        FakeCursor = settings.transform.Find("Mouse Cursor").GetComponent<RawImage>();

        var fuck = GameObject.Find("Cursor").transform.GetChild(0).GetComponent<RawImage>();
        /*FakeCursor.texture = fuck.texture;
        FakeCursor.material = fuck.material;
        FakeCursor.rectTransform.sizeDelta = new Vector2(128f/2f, 111f/2f);
        FakeCursor.raycastTarget = false;
        FakeCursor.rectTransform.pivot = new Vector2(0f, 1f);*/
        FakeCursor.gameObject.SetActive(false);

        StartCoroutine(MakeOptions());

        //if (IsDebug)
        //    Logger.LogInfo("프리팹을 풀에 추가하는 중...");
        //foreach (GameObject prefab in PrefabToAddNetwork)
        //{
        //    NetworkPrefabs.RegisterNetworkPrefab(prefab);
        //    if (IsDebug)
        //        Logger.LogMessage($"프리팹 추가됨: {prefab.name}");
        //}

        Logging.Log("Setup Done. '-'");

        OnLevelChange.AddListener(type => {
            switch (type) {
                case RunManager.ChangeLevelType.RunLevel: {
                    foreach (var evt in ChaosEventManager.Events) {
                        evt.OnStartGame(EventTransform!);
                    }
                    Logging.Log("All event objects are created!");
                    break;
                }

                case RunManager.ChangeLevelType.MainMenu: {
                    foreach (var evt in ChaosEventManager.Events) {
                        evt.OnLeave();
                    }
                    Logging.Log("All event objects are removed!");
                    break;
                }
            }
        });
    } // void Start()

    private RawImage? FakeCursor;
    private float TextDespawnTime = 0f;

    // i fucking hate unity
    IEnumerator MakeOptions()
    {
        // TODO: UI sucks so use MenuLib to match game's mood.
        yield return new WaitForSeconds(0.1f);
        var options_general = Language.GetText("options_general");

        string timebarColor = Language.GetText("options_gauge_color");
        string dizz = Language.GetText("options_dizziness");
        string family = Language.GetText("options_family_friendly");
        string dev = Language.GetText("options_dev_mode");
        string timer = Language.GetText("options_timer");
        string despawn_txt = Language.GetText("options_event_despawn_time");
        options!.AddOption(options_general, new List<OptionItem>
        {
            new(timer, OptionType.Value, 5f, 120f),
            new(Language.GetText("options_timer_desc"), OptionType.Description),
            new(timebarColor, OptionType.Color),
            new(Language.GetText("options_gauge_color_desc"), OptionType.Description),
            new(dizz, OptionType.Checkbox),
            new(Language.GetText("options_dizziness_desc"), OptionType.Description),
            new(family, OptionType.Checkbox),
            new(Language.GetText("options_family_friendly_desc"), OptionType.Description),
            new(dev, OptionType.Checkbox),
            new(Language.GetText("options_dev_mode_desc"), OptionType.Description),
            new(despawn_txt, OptionType.Value, 0f, float.MaxValue, "s"),
            new(Language.GetText("options_event_despawn_time_desc"), OptionType.Description)
        });

        options.SetValue($"{options_general}:{timebarColor}", new Color(144f/255f, 0, 0), false);
        options.SetValue($"{options_general}:{dizz}", false, false);
        options.SetValue($"{options_general}:{family}", false, false);
        options.SetValue($"{options_general}:{dev}", false, false);
        options.SetValue($"{options_general}:{timer}", MaxEventTimer, false);
        options.SetValue($"{options_general}:{despawn_txt}", 0, false);

        var options_lvl_evts = Language.GetText("options_lvl_evts");
        string evt_enabled = Language.GetText("options_evts_enabled");
        string evt_chance = Language.GetText("options_evts_chance");
        string evt_duration = Language.GetText("options_evts_range");
        string range = Language.GetText("options_range");
        string float_maxValue = Language.GetText("infinity");
        string seconds = Language.GetText("options_seconds");
        options.AddCategory(options_lvl_evts);
        ChaosEventManager.Init(mod =>
        {
            var items = new List<OptionItem> {
                new OptionItem(mod.GetDesc(), OptionType.Description),
                new OptionItem(evt_enabled, OptionType.Checkbox),
                new OptionItem(evt_chance, OptionType.Value, 0f, 100f),
                new OptionItem($"{range}: 0.0 ~ 100.0", OptionType.Description)
            };

            if (!mod.isOnce)
            {
                items.Add(new OptionItem(evt_duration, OptionType.Range, 1, float.MaxValue));
                items.Add(new OptionItem($"{range}: 1.0 ~ {float_maxValue} ({seconds})", OptionType.Description));
            }

            options.AddOption(mod.GetName(), items);
            options.SetValue($"{mod.GetName()}:{evt_enabled}", mod.enabled, false);
            options.SetValue($"{mod.GetName()}:{evt_chance}", mod.options.chance * 100f, false);
            if (!mod.isOnce)
                options.SetValue($"{mod.GetName()}:{evt_duration}", new float[] { mod.minTimer, mod.maxTimer }, false);

            mod.OnCreate(EventTransform!);
        });
        LoadAndApplySettings();
        TextDespawnTime = options.GetValue($"{options_general}:{despawn_txt}");
        options.onValueChanged.AddListener((OptionType type, string key, object value) => {
            if (key.StartsWith(options_general)) {
                string fuckSwitch = key.Substring(options_general.Length + 1);
                if (fuckSwitch == timebarColor) {
                    Color col = (Color)value;
                    barImg!.color = col;
                    hasChanges = true;
                    preferences.Set("timebar_color", col);
                } else if (fuckSwitch == dizz) {
                    dizziness = (bool)value;
                    hasChanges = true;
                    preferences.Set("dizziness", dizziness);
                } else if (fuckSwitch == family) {
                    familyFriendly = (bool)value;
                    hasChanges = true;
                    preferences.Set("familyFriendly", familyFriendly);
                } else if (fuckSwitch == dev) {
                    DevMode = (bool)value;
                    hasChanges = true;
                    preferences.Set("devMode", DevMode);
                } else if (fuckSwitch == timer) {
                    MaxEventTimer = (float)value;
                    hasChanges = true;
                    preferences.Set("maxEventTimer", MaxEventTimer);
                } else if (fuckSwitch == despawn_txt) {
                    if (Generated) { // if player changed this in-game, scales the time with it.
                        var events = controller!.events;
                        foreach (var data in controller.events) {
                            if (data == null) continue;
                            if (!data.evt.isOnce) continue; // if this is timer event, we don't need to change it.

                            // remap to range!!!
                            data.evtNameText.DespawnTimer = SemiFunc.Remap(0, TextDespawnTime, 0, (float)value, data.evtNameText.DespawnTimer);
                        }
                    }

                    TextDespawnTime = (float)value;
                    hasChanges = true;
                    preferences.Set("textDespawnTime", TextDespawnTime);
                }
            } else {
                foreach (ChaosEvent mod in ChaosEventManager.Events)
                {
                    string name = mod.GetName();
                    if (key.StartsWith(name)) { // TODO: Make Custom Options for events
                        string sub = key.Substring(name.Length + 1);
                        if (sub == evt_enabled) {
                            mod.enabled = (bool)value;
                            preferences.Set($"event.{mod.nameKey}.enabled", mod.enabled);
                        } else if (sub == evt_chance) {
                            mod.options.chance = (float)value / 100f;
                            preferences.Set($"event.{mod.nameKey}.chance", mod.options.chance);
                        } else if (sub == evt_duration) {
                            float[] array = (float[])value;
                            mod.minTimer = array[0];
                            mod.maxTimer = array[1];
                            preferences.Set($"event.{mod.nameKey}.duration", array);
                        }
                        hasChanges = true;
                        break;
                    }
                }
            }
        });
        Logging.Log("Settings are created!");
    }

    internal static SaveData GetPreferences() => Instance?.preferences;

    public const string CONFIG_HEADER = "CHAOS_MOD_CONFIG";
    public const bool CONFIG_DEBUG = false;
    public string CONFIG_PATH { get; private set; } = "";
    internal SaveData preferences = new(CONFIG_HEADER);
    public void LoadAndApplySettings()
    {
        #pragma warning disable CS0162 // I KNOW SHUT THE FUCK UP
        if (CONFIG_DEBUG) Logging.Warn("[CONFIG DEBUGGING ENABLED]");
        #pragma warning restore CS0162
        try {
            string path = CONFIG_PATH;
            if (!File.Exists(path))
            {
                Logging.Warn($"I didn't found the config file. making a new one at \"{path}\"");
                File.WriteAllText(path, "");
                return;
            }

            Logging.Log($"I found config file at \"{path}\"");

            List<ChaosEvent> tempEvents = ChaosEventManager._events;

            preferences.Load(path);
            foreach (var item in preferences.GetAll()) {
                if (item.Key.StartsWith("event.")) {
                    var lmao = item.Key.Split(".");
                    
                    string evt_name = lmao[1];
                    string evt_var = lmao[2];

                    // use reflection instead of doing hardcoding
                    
                }
            }
        } catch (Exception e) {
            Logging.Error($"An Error has occured while parsing config file!\n{e.Message}\n{e.StackTrace}");
            return;
        }
        Logging.Log("Loaded config sucessfully!");
    }

    internal void SaveSettings()
    {
        Logging.Log("Trying to save settings...");
        preferences.Save(CONFIG_PATH);
        Logging.Log("Done!");
    }

    internal bool photonViewInited = false;
    void OnSceneChange(Scene scene, LoadSceneMode mode)
    {
        photonViewInited = false;
    }

    internal GameObject? carObject;
    // goodbye..
    // internal CrazyCarAIScript? car;

    internal AdManager? adViewer;
    internal GameObject? adObject;
    internal bool dizziness = true;
    internal bool familyFriendly = false;
    private bool hasChanges = false;
    bool godModOn = false;
    internal bool InitPhotonView()
    {
        /*if (adObject == null)
        {
            adObject = Instantiate(assets.LoadAsset<GameObject>("AD Viewer"), Vector3.zero, Quaternion.identity);
            adViewer = adObject.AddComponent<AdManager>();
            adViewer.clips = new List<VideoClip>();

            int adCount = 3;
            for (int i = 1; i <= adCount; i++)
            {
                adViewer.clips.Add(assets.LoadAsset<VideoClip>($"ad{i}.mp4"));
            }

            adObject.transform.SetParent(UICanvas.transform, false);
        }*/

        var NETWORKMAN = GameObject.FindAnyObjectByType<NetworkManager>();
        if (NETWORKMAN == null)
            return false;

        if (controller == null)
        {
            if (!NETWORKMAN.gameObject.TryGetComponent(out controller))
                controller = NETWORKMAN.gameObject.AddComponent<ChaosController>();

            if (!GameManager.Multiplayer())
            {
                view = null!;
                photonViewInited = false;
                return false;
            }
        }

        // yep
        if (!godModOn && IsDebug && !DevMode) {
            if (SemiFunc.PlayerAvatarLocal() != null) { // fuck i keep dying while testing the mod alone. its really annoying
                godModOn = true;
                var me = SemiFunc.PlayerAvatarLocal();
                me.playerHealth.godMode = true;
            }
        }

        if (!GameManager.Multiplayer()) return false;
        if (view == null)
        {
            view = NETWORKMAN.GetComponent<PhotonView>();
            if (IsDebug)
                Logging.Log("PhotonView ID: " + view.ViewID);
            photonViewInited = view != null;
        }
        return photonViewInited;
    }

    internal ChaosController? controller;

    // offsetMin = (왼쪽, -하단)
    // offsetMax = (오른쪽, 상단)

    // 텍스트 높이
    float text_height = 40f;

    // 프리텐다드 폰트 에셋
    internal TMP_FontAsset? pretendard;

    internal EventListData MakeEventText(ChaosEvent mod)
    {
        string text = mod.GetName();
        float time = mod.timerSelf;

        GameObject obj = new(text.ToLower().Trim());
        obj.transform.parent = textGroup;

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.offsetMin = new Vector2(0f, -text_height/2);
        rt.offsetMax = new Vector2(0f, text_height/2);

        // Make text
        TextLerp bText = obj.AddComponent<TextLerp>();
        bText.text = text;
        bText.font = pretendard;
        bText.fontSize = 30;
        bText.rectTransform.anchoredPosition = Vector2.zero;
        bText.targetPosition.Set(0f, 0f);
        bText.horizontalAlignment = HorizontalAlignmentOptions.Left;
        bText.verticalAlignment = VerticalAlignmentOptions.Middle;
        bText.DespawnTimer = TextDespawnTime;
        bText.spriteAsset = eyesFont;

        var data = new EventListData(mod, bText);

        // Make time text
        if (Mathf.Max(0f, time) > 0f)
        {
            GameObject obj2 = new("Time Text");
            obj2.transform.parent = obj.transform;

            RectTransform rect = obj2.AddComponent<RectTransform>();
            EventTimerBar timeTxt = obj2.AddComponent<EventTimerBar>();
            rect.anchoredPosition = Vector2.zero;
            timeTxt.targetPosition.Set(100f, 0f);
            timeTxt.SetTime(time);
            timeTxt.font = pretendard;
            timeTxt.fontSize = 30;
            timeTxt.horizontalAlignment = HorizontalAlignmentOptions.Left;
            timeTxt.verticalAlignment = VerticalAlignmentOptions.Middle;
            timeTxt.hide = mod.options.hideTimer;
            
            data.evtTimeText = timeTxt;
        }

        return data;
    }


    public void ResetText()
    {
        try
        {
            int visibleCount = 0;
            var events = controller!.events;
            foreach (var data in events)
            {
                ChaosEvent evt = data.evt;

                float timeLeft = 0f;
                bool isOnce = true;
                if (data != null) {
                    timeLeft = data.evt.timerSelf;
                    isOnce = timeLeft <= 0 && !evt.isOnce;
                }

                if (!isOnce)
                    visibleCount++;
            }

            int visibleIndex = 0;
            for (int i = events.Count - 1; i >= 0; i--)
            {
                var data = events[i];
                if (data == null) continue;

                var text = data.evtNameText;
                var timeTxt = data.evtTimeText;

                float y = text_height * visibleIndex;

                bool timerFinished = data.evt.timerSelf <= 0f && text.DespawnTimer <= 0f;
                bool isOnceEvent = data.evt.isOnce;
                bool isOutsideOfRange = y >= 375f * controller.timeScale;

                if ((isOnceEvent && isOutsideOfRange) || (!isOnceEvent && timerFinished)) {
                    controller.RequestRemoveEvent(i);
                } else {
                    text.targetPosition = new Vector2(text.targetPosition.x, y);
                }

                visibleIndex++;
            }
        }
        catch (Exception e)
        {
            Logging.Error($"텍스트의 위치를 리셋하던 중 오류가 발생했습니다: {e.Message}\n{e.StackTrace}");
        }
    }

    public void SetEventText(int idx)
    {
        var events = controller!.events;
        if (events == null || idx < 0 || idx >= events.Count || events[idx] == null) {
            Logging.Error("SetEventText | Cannot find event with index.");
            return;
        }

        if (!controller.queue_setText.Contains(idx)) controller.queue_setText.Enqueue(idx);
    }

    bool didReset = false;
    private void Update()
    {
        bool shouldOn = Generated && !DISABLE_TIMER;
        if (timeBar!.activeSelf != shouldOn)
            timeBar.SetActive(shouldOn);

        if (Input.GetKeyDown(ConfigOptionsMenu!.Value))
        {
            if (options!.isShown)
            {
                FakeCursor!.gameObject.SetActive(false);
                options.Hide();
                if (hasChanges)
                {
                    hasChanges = false;
                    SaveSettings();
                }
            } else {
                options.Show();
                FakeCursor!.gameObject.SetActive(true);
            }
        }
        FakeCursor!.rectTransform.position = Input.mousePosition;

        if (!Generated)
        {
            photonViewInited = false;

            if (IsDebug)
                DebugText!.text = "";

            if (!didReset)
            {
                didReset = true;
                controller?.evtRand?.Reset();
                godModOn = false;
            }

            return;
        }

        if (didReset)
            didReset = false;

        if (view == null || controller == null)
        {
            photonViewInited = InitPhotonView();
        }

        barRect!.offsetMax = new Vector2(SemiFunc.Remap(0f, MaxEventTimer, 0f, -canvasWidth, controller!.eventTimer), barRect.offsetMax.y);
    }
}