using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using REPOLib.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using OptionItem = ChaosMod.OptionsMenuController.OptionItem;
using OptionType = ChaosMod.OptionsMenuController.OptionType;

namespace ChaosMod
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class ChaosMod : BaseUnityPlugin
    {
        internal static ChaosMod Instance { get; private set; } = null!;
        internal static bool IsDebug
        {
            get {
                bool enable = false;
                return Instance.DevMode || enable;
            }
        }
        public static readonly bool DISABLE_TIMER = false;
        internal const float MaxEventTimer = 20f;
        /// <summary>
        /// This list contains enemy who is not despawned. (if despawns removes from the list automatically)
        /// </summary>
        internal HashSet<EnemyParent> spawnedEnemys = new HashSet<EnemyParent>();
        internal new static ManualLogSource Logger => Instance._logger;
        private ManualLogSource _logger => base.Logger;
        internal Harmony? Harmony { get; set; }
        internal bool DevMode = false;

        internal Canvas UICanvas;
        internal RectTransform barRect;

        internal PhotonView view;

        internal AudioSource EnemyAS;
        internal AudioSource RumbleAS;
        internal AudioSource AudioSource;
        internal AudioSource ClubSource;

        internal PostProcessVolume shaderOverlay;

        internal ConfigEntry<KeyCode> ConfigOptionsMenu;

        internal List<GameObject> PrefabToAddNetwork = new List<GameObject>();
        
        private void Awake()
        {
            Instance = this;

            // Prevent the plugin from being deleted
            this.gameObject.transform.parent = null;
            this.gameObject.hideFlags = HideFlags.HideAndDontSave;

            Language.Load(CultureInfo.CurrentCulture.Name == "ko-KR" ? "ko" : "en-us");
            ConfigOptionsMenu =    Config.Bind<KeyCode>("General", "Option Key",  KeyCode.F4, "AAAAAAAAAAAAAAAAAA");

            Patch();
            Logger.LogInfo($"{Info.Metadata.Name} has loaded!");
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
        internal GameObject timeBar;
        internal RectTransform textGroup;
        internal AssetBundle assets;
        internal TextMeshProUGUI DebugText;
        internal Image barImg;
        internal OptionsMenuController options;
        void Start()
        {
            // Assetbundle 로드
            string path = Util.GetPluginDirectory("assets");
            Logger.LogInfo($"에셋 번들을 '{path}'에서 불러옵니다.");
            assets = AssetBundle.LoadFromFile(path);
            if (assets == null)
            {
                Logger.LogError("에셋 번들 로드에 실패했습니다. 폰트가 깨지거나 몇 개의 에셋이 보이지 않을 수 있습니다.");
            }
            else
            {
                pretendard = assets.LoadAsset<TMP_FontAsset>("Pretendard-Bold SDF");
                if (pretendard == null)
                {
                    Logger.LogError("폰트 로드에 실패했습니다. 폰트가 깨질 수 있습니다.");
                }
            }

            GameObject UIPrefab = Instantiate(assets.LoadAsset<GameObject>("Chaos Controller"));
            UICanvas = UIPrefab.GetComponent<Canvas>();

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

            EnemyAS = UIPrefab.transform.Find("Audio Source").Find("Enemy Sound Source").GetComponent<AudioSource>();
            EnemyAS.outputAudioMixerGroup = AudioManager.instance.SoundMasterGroup;

            RumbleAS = UIPrefab.transform.Find("Audio Source").Find("Rumble Sound Source").GetComponent<AudioSource>();
            RumbleAS.outputAudioMixerGroup = AudioManager.instance.SoundMasterGroup;

            AudioSource = UIPrefab.transform.Find("Audio Source").Find("Master Sound Source").GetComponent<AudioSource>();
            AudioSource.outputAudioMixerGroup = AudioManager.instance.SoundMasterGroup;

            ClubSource = UIPrefab.transform.Find("Audio Source").Find("Club Music Source").GetComponent<AudioSource>();
            ClubSource.outputAudioMixerGroup = AudioManager.instance.MusicMasterGroup;

            // ㅅㅂ 모드 만드는거 못해먹겠다
            // 붐박스 노래 꺼내오기
            GameObject boombox = Instantiate(Resources.Load<GameObject>("valuables/04 big/Valuable Museum Boombox"), new Vector3(9e9f, 0f, 0f), Quaternion.identity);
            boombox.SetActive(false);
            ClubSource.clip = boombox.GetComponent<ValuableBoombox>().soundBoomboxMusic.Sounds[0];
            Destroy(boombox);

            // 캔버스의 가로, 세로 크기
            canvasHeight = ((RectTransform)UICanvas.transform).rect.height;
            canvasWidth = ((RectTransform)UICanvas.transform).rect.width;
            DontDestroyOnLoad(UICanvas);

            ThinkFast.scout = UIPrefab.transform.Find("Scout TF2").GetComponent<Image>();
            ThinkFast.text1 = UIPrefab.transform.Find("Upper Text").gameObject;
            ThinkFast.text2 = UIPrefab.transform.Find("Lower Text").gameObject;

            // ThinkFast.flash = flashObj.AddComponent<Image>();
            // ThinkFast.flash.color = new Color(1f, 1f, 1f, 0f);

            timeBar = UIPrefab.transform.Find("Time Bar").gameObject;

            barImg = timeBar.transform.Find("FG").GetComponent<Image>();
            // barImg.color = new Color(ConfigBarColorR.Value / 255f, ConfigBarColorG.Value / 255f, ConfigBarColorB.Value / 255f, 1f);

            barRect = (RectTransform)barImg.transform;

            textGroup = (RectTransform)UIPrefab.transform.Find("Modifier Text Group").transform;

            timeBar.SetActive(false);
            ThinkFast.scout.gameObject.SetActive(false);
            ThinkFast.text1.SetActive(false);
            ThinkFast.text2.SetActive(false);

            ThinkFast.text1.GetComponent<TextMeshProUGUI>().text = Language.GetText("evt_text_think_fast");
            ThinkFast.text2.GetComponent<TextMeshProUGUI>().text = Language.GetText("evt_text_chucklenuts");

            SceneManager.sceneLoaded += OnSceneChange;
            photonViewInited = false;

            DebugText = UIPrefab.transform.Find("Debug Text").GetComponent<TextMeshProUGUI>();
            // 디버깅 전용 텍스트
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
            FakeCursor.texture = fuck.texture;
            FakeCursor.material = fuck.material;
            FakeCursor.gameObject.SetActive(false);
            FakeCursor.rectTransform.sizeDelta = new Vector2(128f/2f, 111f/2f);
            FakeCursor.raycastTarget = false;
            FakeCursor.rectTransform.pivot = new Vector2(0.2f, 0.8f);

            if (IsDebug)
                Logger.LogInfo("프리팹을 풀에 추가하는 중...");
            foreach (GameObject prefab in PrefabToAddNetwork)
            {
                NetworkPrefabs.RegisterNetworkPrefab(prefab);
                if (IsDebug)
                    Logger.LogMessage($"프리팹 추가됨: {prefab.name}");
            }

            Logger.LogMessage("Setup Done. '-'");

            StartCoroutine(MakeOptions());
        } // void Start()

        private RawImage FakeCursor;

        // i fucking hate unity
        IEnumerator MakeOptions()
        {
            yield return new WaitForSeconds(0.1f);
            var options_general = Language.GetText("options_general");

            string timebarColor = Language.GetText("options_gauge_color");
            string dizz = Language.GetText("options_dizzyness");
            string family = Language.GetText("options_family_friendly");
            string dev = Language.GetText("options_dev_mode");
            options.AddOption(options_general, new List<OptionItem>
            {
                new OptionItem(timebarColor, OptionType.Color),
                new OptionItem(dizz, OptionType.Checkbox),
                new OptionItem(family, OptionType.Checkbox),
                new OptionItem(dev, OptionType.Checkbox),
            });

            options.SetValue($"{options_general}:{timebarColor}", new Color(144f/255f, 0, 0), false);
            options.SetValue($"{options_general}:{dizz}", false, false);
            options.SetValue($"{options_general}:{family}", false, false);
            options.SetValue($"{options_general}:{dev}", false, false);

            var options_lvl_evts = Language.GetText("options_lvl_evts");
            string evt_enabled = Language.GetText("options_evts_enabled");
            string evt_chance = Language.GetText("options_evts_chance");
            string evt_duration = Language.GetText("options_evts_range");
            options.AddCategory(options_lvl_evts);
            Modifiers.Init(mod =>
            {
                var items = new List<OptionItem> {
                    new OptionItem(evt_enabled, OptionType.Checkbox),
                    new OptionItem(evt_chance, OptionType.Value, 0f, 100f),
                };

                if (!mod.isOnce)
                {
                    items.Add(new OptionItem(evt_duration, OptionType.Range, 0, float.MaxValue));
                }

                options.AddOption(mod.GetName(), items);
                options.SetValue($"{mod.GetName()}:{evt_enabled}", mod.enabled, false);
                options.SetValue($"{mod.GetName()}:{evt_chance}", mod.options.chance * 100f, false);
                if (!mod.isOnce)
                    options.SetValue($"{mod.GetName()}:{evt_duration}", new float[] { mod.minTimer, mod.maxTimer }, false);
            });
            LoadAndApplySettings();
            options.onValueChanged.AddListener((OptionType type, string key, object value) => {
                if (key.StartsWith(options_general)) {
                    string fuckSwitch = key.Substring(options_general.Length + 1);
                    if (fuckSwitch == timebarColor) {
                        Color col = (Color)value;
                        barImg.color = col;
                        hasChanges = true;
                    } else if (fuckSwitch == dizz) {
                        dizzyness = (bool)value;
                        hasChanges = true;
                    } else if (fuckSwitch == family) {
                        familyFriendly = (bool)value;
                        hasChanges = true;
                    } else if (fuckSwitch == dev) {
                        DevMode = (bool)value;
                        hasChanges = true;
                    }
                } else {
                    foreach (Modifier mod in Modifiers.Events)
                    {
                        string name = mod.GetName();
                        if (key.StartsWith(name)) {
                            string sub = key.Substring(name.Length + 1);
                            if (sub == evt_enabled) {
                                mod.enabled = (bool)value;
                            } else if (sub == evt_chance) {
                                mod.options.chance = (float)value / 100f;
                            } else if (sub == evt_duration) {
                                float[] array = (float[])value;
                                mod.minTimer = array[0];
                                mod.maxTimer = array[1];
                            }
                            hasChanges = true;
                            break;
                        }
                    }
                }

                if (hasChanges && IsDebug)
                    Logger.LogInfo("User has changed something");
            });
            Logger.LogInfo("Settings are created!");
        }

        private const string CONFIG_HEADER = "CHAOS_MOD_CONFIG";
        private const int CONFIG_VERSION = 1;
        void LoadAndApplySettings()
        {
            string path = Util.GetPluginDirectory("settings.bytes");
            if (!File.Exists(path))
            {
                Logger.LogWarning("Settings File not found. Making a new one.");
                File.WriteAllText(path, "");
                return;
            }

            using var fs = new FileStream(path, FileMode.Open);
            using var br = new BinaryReader(fs);

            try
            {
                if (br.ReadString() != CONFIG_HEADER)
                {
                    Logger.LogError(Language.GetText("exception_config_invaild_header"));
                    return;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.GetBaseException() + ": " + e.Message + "\n" + e.StackTrace);
                if (e.GetBaseException() is EndOfStreamException)
                {
                    Logger.LogMessage(Language.GetText("exception_endofstream_config_1"));
                    Logger.LogMessage(Language.GetText("exception_endofstream_config_2"));
                }
                return;
            }

            if (br.ReadInt32() > CONFIG_VERSION) { // if file version is higher, we cannot parse it.
                Logger.LogError(Language.GetText("exception_config_version_match"));
                return;
            }

            for (int i = 0; i < br.ReadInt32(); i++)
            {
                string key = br.ReadString();
                string type = br.ReadString();
                switch (type)
                {
                    case "bool":
                        {
                            options.SetValue(key, br.ReadBoolean(), true);
                            break;
                        }
                    case "float":
                        {
                            options.SetValue(key, br.ReadSingle(), true);
                            break;
                        }
                    case "range":
                        {
                            options.SetValue(key, new float[] { br.ReadSingle(), br.ReadSingle() }, true);
                            break;
                        }
                    case "color":
                        {
                            Color col = new Color(
                                br.ReadSingle(),
                                br.ReadSingle(),
                                br.ReadSingle()
                            );
                            options.SetValue(key, col, true);
                            break;
                        }
                }
            }

            
        }

        void SaveSettings()
        {
            Logger.LogInfo("Trying to save settings...");
            using var fs = new FileStream(Util.GetPluginDirectory("settings.bytes"), FileMode.Create);
            using var bw = new BinaryWriter(fs);

            bw.Write(CONFIG_HEADER);
            bw.Write(CONFIG_VERSION);
            bw.Write(options.datas.Count);

            foreach (var item in options.datas)
            {
                bw.Write(item.Key);
                // FUCKKK
                if (item.Value is bool bChecked) {
                    bw.Write("bool");
                    bw.Write(bChecked);
                } else if (item.Value is float fValue) {
                    bw.Write("float");
                    bw.Write(fValue);
                } else if (item.Value is float[] arr) {
                    bw.Write("range");
                    bw.Write(arr[0]);
                    bw.Write(arr[1]);
                } else if (item.Value is Color col) {
                    bw.Write("color");
                    bw.Write(col.r);
                    bw.Write(col.g);
                    bw.Write(col.b);
                }
            }

            bw.Flush();
            Logger.LogInfo("Done!");
        }

        internal bool photonViewInited = false;
        void OnSceneChange(Scene scene, LoadSceneMode mode)
        {
            photonViewInited = false;
        }

        internal GameObject carObject;
        internal CrazyCarAIScript car;

        internal AdManager adViewer;
        internal GameObject adObject;
        internal bool dizzyness = true;
        internal bool familyFriendly = false;
        private bool hasChanges = false;

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
                    view = null;
                    photonViewInited = false;
                    return false;
                }
            }

            if (!GameManager.Multiplayer()) return false;
            if (view == null)
            {
                view = NETWORKMAN.GetComponent<PhotonView>();
                if (IsDebug)
                    Logger.LogMessage("PhotonView ID: " + view.ViewID);
                photonViewInited = view != null;
            }
            return photonViewInited;
        }

        internal ChaosController controller;

        // offsetMin = (왼쪽, -하단)
        // offsetMax = (오른쪽, 상단)

        // 텍스트 높이
        float text_height = 40f;

        // 프리텐다드 폰트 에셋
        internal TMP_FontAsset pretendard;

        // 텍스트가 뭉탱이로 모여있는 리스트
        internal List<TextLerp> texts = new List<TextLerp>();
        internal List<EventTimerBar> eventTimerBars = new List<EventTimerBar>();

        public TextMeshProUGUI MakeText(string text, float time)
        {
            GameObject obj = new GameObject(text.ToLower().Trim());
            obj.transform.parent = textGroup;

            // 수평은 부모 전체를 꽉 채우고, 수직은 부모의 가운데 위치
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.offsetMin = new Vector2(0f, -text_height/2);
            rt.offsetMax = new Vector2(0f, text_height/2);

            // 텍스트 만들기
            TextLerp bText = obj.AddComponent<TextLerp>();
            bText.text = text;
            bText.font = pretendard;
            bText.fontSize = 30;
            bText.rectTransform.anchoredPosition = Vector2.zero;
            bText.targetPosition.Set(0f, 0f);
            bText.horizontalAlignment = HorizontalAlignmentOptions.Left;
            bText.verticalAlignment = VerticalAlignmentOptions.Middle;
            texts.Add(bText);

            // 타임바 만들기
            if (Mathf.Max(0f, time) > 0f)
            {
                GameObject obj2 = new GameObject("Time Text");
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
                eventTimerBars.Add(timeTxt);
            }

            else eventTimerBars.Add(null);

            // 텍스트 업데이트
            ResetText();

            return bText;
        }

        internal List<int> EventToRemove = new List<int>();
        internal List<int> TextToRemove = new List<int>();

        public void ResetText()
        {
            try
            {
                int visibleCount = 0;
                for (int i = 0; i < texts.Count; i++)
                {
                    float timeLeft = 0f;
                    bool isOnce = true;
                    if ((i >= 0 && i < controller.events.Count) && controller.events[i] != null)
                    {
                        timeLeft = controller.events[i].timerSelf;
                        isOnce = timeLeft <= 0 && !controller.events[i].isOnce;
                    }

                    if (!isOnce)
                        visibleCount++;
                    
                }

                int visibleIndex = 0;
                for (int i = texts.Count - 1; i >= 0; i--)
                {
                    if (EventToRemove.Contains(i)) continue;

                    var text = texts[i];
                    var timeTxt = eventTimerBars[i];

                    float y = text_height * visibleIndex;

                    if (y >= 375f * controller.timeScale)
                    {
                        EventToRemove.Add(i);
                        TextToRemove.Add(i);
                    }
                    else
                    {
                        text.targetPosition = new Vector2(text.targetPosition.x, y);
                    }

                    visibleIndex++;
                }
            }
            catch (System.Exception e)
            {
                Logger.LogError($"텍스트의 위치를 리셋하던 중 오류가 발생했습니다: {e.Message}\n{e.StackTrace}");
            }
        }

        public bool RemoveText(TextLerp text)
        {
            int index = texts.IndexOf(text);
            if (index == -1)
            {
                if (IsDebug)
                    Logger.LogWarning($"텍스트 \"{text.text}\"의 인덱스를 찾지 못했습니다.");
                return false;
            }

            return RemoveText(index);
        }

        public bool RemoveText(int index)
        {
            if (index >= 0 && index < texts.Count)
            {
                var txt = texts[index];
                Destroy(txt.gameObject);

                var txt2 = eventTimerBars[index];
                if (txt2 != null)
                {
                    Destroy(txt2.gameObject);
                }
                texts.RemoveAt(index);
                eventTimerBars.RemoveAt(index);
                return true;
            }
            return false;
        }

        bool didReset = false;
        private void Update()
        {
            if (timeBar.activeSelf != Generated)
                timeBar.SetActive(Generated);

            if (Input.GetKeyDown(ConfigOptionsMenu.Value))
            {
                if (options.isShown)
                {
                    FakeCursor.gameObject.SetActive(false);
                    options.Hide();
                    if (hasChanges)
                    {
                        hasChanges = false;
                        SaveSettings();
                    }
                }
                else
                {
                    options.Show();
                    FakeCursor.gameObject.SetActive(true);
                }
            }
            FakeCursor.rectTransform.position = Input.mousePosition;

            if (!Generated)
            {
                // 레벨에 들어가있지 않으면 텍스트 전체 제거
                if (texts.Count > 0)
                {
                    for (int i = 0; i < texts.Count; i++)
                    {
                        Destroy(texts[i]);
                    }
                    texts.Clear();
                }

                if (eventTimerBars.Count > 0)
                {
                    for (int i = 0; i < eventTimerBars.Count; i++)
                    {
                        Destroy(eventTimerBars[i]);
                    }
                    eventTimerBars.Clear();
                }
                photonViewInited = false;

                if (IsDebug)
                    DebugText.text = "";

                if (!didReset)
                {
                    didReset = true;
                    //foreach (Modifier mod in Modifiers.Events)
                    //{
                    //    mod.options.chance = 1f;
                    //}
                    ModVars.Reset();
                }

                return;
            }

            if (didReset)
                didReset = false;

            if (view == null || controller == null)
            {
                photonViewInited = InitPhotonView();
            }

            // 개 병신새끼야 나가 뒤져 시발 아주 잘도 태어났네
            //if (carObject == null || car == null)
            //{
            //    var car_assets = CarCrash.car_assets;
            //    if (!GameManager.Multiplayer())
            //    {
            //        carObject = Instantiate(car_assets.LoadAsset<GameObject>("Killer Joe"), Vector3.zero, Quaternion.identity);
            //        car = carObject.AddComponent<CrazyCarAIScript>();
            //        car.honk = car_assets.LoadAsset<AudioClip>("car honk");
            //        car.exp_sprites = car_assets.LoadAssetWithSubAssets<Sprite>("spr_realisticexplosion").ToList();
            //    }
            //    else if (controller != null && controller.view != null && SemiFunc.IsMasterClient())
            //    {
            //        PhotonNetwork.Instantiate("Killer Joe", Vector3.zero, Quaternion.identity);
                    
            //    }
                
            //    // add scripts to all clients!!!!
            //    if (SemiFunc.IsMultiplayer())
            //    {
            //        controller.FindCarRPC();
            //    }
            //} else if (!car.setupDone) {
            //    car.Start();
            //}

            barRect.offsetMax = new Vector2(SemiFunc.Remap(0f, MaxEventTimer, 0f, -canvasWidth, controller.eventTimer), barRect.offsetMax.y);

            if (TextToRemove.Count > 0)
            {
                var toRemoveSet = TextToRemove.Distinct().OrderByDescending(i => i);
                foreach (int i in toRemoveSet)
                {
                    if (i >= 0 && i < texts.Count)
                    {
                        RemoveText(i);
                        // ResetText();
                    }
                }
                TextToRemove.Clear();
            }
        }
    }
}