using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using System.Collections;
using REPOLib.Modules;

namespace ChaosMod
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class ChaosMod : BaseUnityPlugin
    {
        internal static ChaosMod Instance { get; private set; } = null!;
        public static readonly bool IsDebug = true;
        internal const float MaxEventTimer = 20f;
        internal new static ManualLogSource Logger => Instance._logger;
        private ManualLogSource _logger => base.Logger;
        internal Harmony? Harmony { get; set; }

        internal Canvas UICanvas;
        internal RectTransform barRect;

        internal PhotonView view;

        internal AudioSource EnemyAS;
        internal AudioSource RumbleAS;

        internal PostProcessVolume shaderOverlay;

        internal ConfigEntry<bool> ConfigKorean;
        internal ConfigEntry<int> ConfigBarColorR;
        internal ConfigEntry<int> ConfigBarColorG;
        internal ConfigEntry<int> ConfigBarColorB;
        internal ConfigEntry<bool> ConfigDizzyness;
        internal ConfigEntry<bool> ConfigFamilyFriendly;
        internal List<string> Exclude_events = new List<string>();

        internal List<GameObject> PrefabToAddNetwork = new List<GameObject>();
        
        private void Awake()
        {
            Instance = this;

            // Prevent the plugin from being deleted
            this.gameObject.transform.parent = null;
            this.gameObject.hideFlags = HideFlags.HideAndDontSave;

            Language.Init();
            string general = Language.GetText("기본");
            ConfigKorean = Config.Bind<bool>(general, "한국어", false, new ConfigDescription("켜져 있으면 한국어를 사용합니다. If it's on, this mod will use korean."));
            ConfigBarColorR = Config.Bind<int>(general, Language.GetText("게이지 빨간색 차지 비중"), 130, new ConfigDescription("바의 빨간 색입니다. Bar's Red Color.", new AcceptableValueRange<int>(0, 255)));
            ConfigBarColorG = Config.Bind<int>(general, Language.GetText("게이지 초록색 차지 비중"), 0, new ConfigDescription("바의 초록 색입니다. Bar's Green Color.", new AcceptableValueRange<int>(0, 255)));
            ConfigBarColorB = Config.Bind<int>(general, Language.GetText("게이지 파란색 차지 비중"), 0, new ConfigDescription("바의 파란 색입니다. Bar's Blue Color.", new AcceptableValueRange<int>(0, 255)));
            ConfigDizzyness = Config.Bind<bool>(general, Language.GetText("어지러움"), true, new ConfigDescription("꺼져 있으면 어지러운 효과를 낮추거나 비활성화합니다. If it's off, disables some dizzy effect."));
            ConfigFamilyFriendly = Config.Bind<bool>(general, Language.GetText("가족 친화적"), false, new ConfigDescription("켜져 있으면 비속어를 필터링합니다. (호스트 전용) If it's on, filters some bad words. (Host only)"));

            ConfigBarColorR.SettingChanged += (sender, value) =>
            {
                if (barImg != null)
                    barImg.color = new Color(ConfigBarColorR.Value / 255f, barImg.color.g, barImg.color.b, 1f);
            };
            ConfigBarColorG.SettingChanged += (sender, value) =>
            {
                if (barImg != null)
                    barImg.color = new Color(barImg.color.r, ConfigBarColorG.Value / 255f, barImg.color.b, 1f);
            };
            ConfigBarColorB.SettingChanged += (sender, value) =>
            {
                if (barImg != null)
                    barImg.color = new Color(barImg.color.r, barImg.color.g, ConfigBarColorB.Value / 255f, 1f);
            };

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
                return LevelGenerator.Instance.Generated && !SemiFunc.IsMainMenu() && SemiFunc.RunIsLevel();
            }
            set { }
        }

        float canvasHeight = 0f;
        float canvasWidth = 0f;
        internal GameObject timeBar;
        internal RectTransform textGroup;
        internal AssetBundle assets;
        internal TextMeshProUGUI creditTxt;
        internal TextMeshProUGUI DebugText;
        internal Image barImg;
        void Start()
        {
            GameObject canvas = new GameObject("Chaos Controller");
            UICanvas = canvas.AddComponent<Canvas>();
            UICanvas.sortingOrder = 999;
            UICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.AddComponent<CanvasScaler>();
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvas.AddComponent<GraphicRaycaster>();

            shaderOverlay = canvas.AddComponent<PostProcessVolume>();
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

            GameObject yoinky = new GameObject("Audio Sources");
            yoinky.transform.parent = canvas.transform;
            yoinky.AddComponent<RectTransform>();

            GameObject spinky = new GameObject("Enemy Sound Source");
            spinky.transform.parent = yoinky.transform;

            EnemyAS = spinky.AddComponent<AudioSource>();
            EnemyAS.dopplerLevel = 0f;
            EnemyAS.volume = 0.75f;
            EnemyAS.outputAudioMixerGroup = AudioManager.instance.SoundMasterGroup;

            GameObject fuck = new GameObject("Rumble Sound Source");
            fuck.transform.parent = yoinky.transform;
            RumbleAS = fuck.AddComponent<AudioSource>();
            RumbleAS.dopplerLevel = 0f;
            // RumbleAS.volume = 0.5f;
            RumbleAS.loop = true;
            RumbleAS.playOnAwake = false;
            RumbleAS.outputAudioMixerGroup = AudioManager.instance.SoundMasterGroup;
            
            var reverb = spinky.AddComponent<AudioReverbFilter>();
            reverb.reverbPreset = AudioReverbPreset.Concerthall;
            reverb.reverbLevel = 1f;
            reverb.decayTime = .5f;
            reverb.enabled = true;

            // 캔버스의 가로, 세로 크기
            canvasHeight = ((RectTransform)canvas.transform).rect.height;
            canvasWidth = ((RectTransform)canvas.transform).rect.width;
            DontDestroyOnLoad(canvas);

            // 타임 바 만들기
            timeBar = new GameObject("Time Bar");
            timeBar.transform.parent = canvas.transform;
            timeBar.SetActive(false);
            
            // 크기가 부모 전체를 꽉 채우도록 설정
            RectTransform lord = timeBar.AddComponent<RectTransform>();
            lord.anchorMin = Vector2.zero;
            lord.anchorMax = Vector2.one;
            lord.offsetMin = Vector2.zero;
            lord.offsetMax = Vector2.zero;

            GameObject timeBarBG = new GameObject("BG");
            timeBarBG.transform.parent = timeBar.transform;
            GameObject timeBarFG = new GameObject("FG");
            timeBarFG.transform.parent = timeBar.transform;

            Image bgImg = timeBarBG.AddComponent<Image>();
            bgImg.color = Color.black;

            // 타임바 배경을 화면 위에 위치
            bgImg.rectTransform.anchorMin = Vector2.zero;
            bgImg.rectTransform.anchorMax = Vector2.one;
            bgImg.rectTransform.offsetMin = new Vector2(0, canvasHeight - 30);
            bgImg.rectTransform.offsetMax = Vector2.zero;

            barImg = timeBarFG.AddComponent<Image>();
            barImg.color = new Color(ConfigBarColorR.Value / 255f, ConfigBarColorG.Value / 255f, ConfigBarColorB.Value / 255f, 1f);

            // 타임바 전경을 화면 위에 위치, 5픽셀 정도 떨어져서
            barRect = barImg.rectTransform;
            barRect.anchorMin = Vector2.zero;
            barRect.anchorMax = Vector2.one;
            barRect.offsetMin = new Vector2(0f, canvasHeight - 25);
            barRect.offsetMax = new Vector2(0f, -5f);

            // 텍스트 그룹 만들기
            GameObject groupParent = new GameObject("Text Group");
            groupParent.transform.parent = canvas.transform;

            // 화면의 오른쪽에 위치, 길이는 화면의 1/4정도
            textGroup = groupParent.AddComponent<RectTransform>();
            textGroup.anchorMin = Vector2.zero;
            textGroup.anchorMax = Vector2.one;

            float centerX = canvasWidth / 2;
            textGroup.offsetMin = new Vector2(centerX + (centerX / 2f), 0f);
            textGroup.offsetMax = Vector2.zero;

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

            RumbleAS.clip = assets.LoadAsset<AudioClip>("rumble");
            RumbleAS.Stop();

            SceneManager.sceneLoaded += OnSceneChange;
            photonViewInited = false;

            GameObject creditObject = new GameObject("Credit Text");
            creditObject.transform.parent = canvas.transform;
            RectTransform rt = creditObject.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;

            rt.offsetMin = new Vector2(centerX + (centerX / 2f), 0f);
            rt.offsetMax = Vector2.zero;

            // 크레딧 텍스트
            creditTxt = creditObject.AddComponent<TextMeshProUGUI>();
            creditTxt.font = pretendard;
            creditTxt.alignment = TextAlignmentOptions.Center;
            if (IsDebug)
                creditTxt.text += "\n<size=30>디버그 모드 활성화됨</size>";
            creditObject.SetActive(false);

            // 디버깅 전용 텍스트
            if (IsDebug)
            {
                GameObject textObject = new GameObject("Debug Text");
                textObject.transform.parent = canvas.transform;

                // 화면에서 100만큼 작음
                Vector2 offset = Vector2.one * 100f;

                rt = textObject.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = offset;
                rt.offsetMax = offset * -1f;

                DebugText = textObject.AddComponent<TextMeshProUGUI>();
                DebugText.font = pretendard;
                DebugText.text = "";
                DebugText.fontSize = 20;
            }

            Exclude_events.Clear();
            Modifiers.Init(mod =>
            {
                var config = Config.Bind<bool>(Language.GetText("레벨 이벤트"), mod.GetName(), true, new ConfigDescription(mod.description));
                config.SettingChanged += (sender, value) =>
                {
                    OnEventSettingChanged(config, mod);
                };
                OnEventSettingChanged(config, mod);
            });

            Logger.LogInfo("Adding Prefabs to the pool...");
            foreach (GameObject prefab in PrefabToAddNetwork)
            {
                NetworkPrefabs.RegisterNetworkPrefab(prefab);
                Logger.LogMessage($"Added Prefab: {prefab.name}");
            }

            Logger.LogMessage("Done. '-' here have a clover's face");
        }

        void OnEventSettingChanged(ConfigEntry<bool> config, Modifier mod)
        {
            if (config.Value)
            {
                if (!Exclude_events.Contains(mod.name))
                    Exclude_events.Add(mod.name);
            }
            else
            {
                if (Exclude_events.Contains(mod.name))
                    Exclude_events.Remove(mod.name);
            }

            if (IsDebug)
                Logger.LogInfo($"이벤트 상태 변경: {mod.name} => {config.Value}");
        }

        internal bool photonViewInited = false;
        void OnSceneChange(Scene scene, LoadSceneMode mode)
        {
            photonViewInited = false;
        }

        //GameObject carObject;
        //CrazyCarAIScript car;

        internal bool InitPhotonView()
        {
            //if (carObject == null)
            //{
            //    carObject = Instantiate(assets.LoadAsset<GameObject>("Killer Joe"), Vector3.zero, Quaternion.identity);
            //    car = carObject.AddComponent<CrazyCarAIScript>();
            //    car.honk = assets.LoadAsset<AudioClip>("car honk");
            //    car.exp_sprites = assets.LoadAssetWithSubAssets<Sprite>("spr_realisticexplosion").ToList();
            //    car.transform.localScale = Vector3.one * 0.125f;
            //    foreach (var lp in FindObjectsByType<LevelPoint>(0))
            //    {
            //        car.waypoints.Add(lp.transform.position);
            //    }

            //    for (int i = 0; i < UnityEngine.AI.NavMesh.GetSettingsCount(); i++)
            //    {
            //        var setting = UnityEngine.AI.NavMesh.GetSettingsByIndex(i);
            //        Logger.LogInfo($"Name: {UnityEngine.AI.NavMesh.GetSettingsNameFromID(i)}, ID: {setting.agentTypeID}");
            //    }
            //}

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
        float text_height = 55f;

        // 프리텐다드 폰트 에셋
        internal TMP_FontAsset pretendard;

        // 텍스트가 뭉탱이로 모여있는 리스트
        internal List<TextLerp> texts = new List<TextLerp>();
        List<EventTimerBar> eventTimerBars = new List<EventTimerBar>();

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

                    if (y >= 375f)
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

        float startTimer = 0f;
        bool didReset = false;
        private void Update()
        {
            if (timeBar.activeSelf != Generated)
                timeBar.SetActive(Generated);
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
                startTimer = 0f;
                if (creditTxt.gameObject.activeSelf)
                    creditTxt.gameObject.SetActive(false);

                if (IsDebug)
                    DebugText.text = "";

                if (!didReset)
                {
                    didReset = true;
                    foreach (Modifier mod in Modifiers.Events)
                    {
                        mod.options.chance = 1f;
                    }
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

            if (startTimer < 5f)
            {
                startTimer += Time.unscaledDeltaTime;
                if (!creditTxt.gameObject.activeSelf)
                    creditTxt.gameObject.SetActive(true);
            }
            else
            {
                if (creditTxt.gameObject.activeSelf)
                    creditTxt.gameObject.SetActive(false);
            }

            barRect.offsetMax = new Vector2(MathUtil.remapToRange(controller.eventTimer, 0f, MaxEventTimer, 0f, -canvasWidth), barRect.offsetMax.y);

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