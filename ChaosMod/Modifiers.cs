using Photon.Pun;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ChaosMod
{
    public class Modifiers
    {
        public static readonly List<Modifier> Events = new List<Modifier>();
        public static readonly List<Modifier> ShopEvents = new List<Modifier>();
        public static readonly List<Modifier> DebugEvents = new List<Modifier>(); // 개발자 모드에서는 이벤트가 이걸로 고정

        public static void Init(Action<Modifier> callback = null)
        {
            List<Modifier> modifiers = new List<Modifier>() {
                new Modifier("evt_none",""){isOnce = true},// 무효
                new Tumble(),                              // 넘어지기
                new KillAllMonsters(),                     // 모든 몬스터 죽이기
                new QuakeFOV(),                            // 넓은 시야각
                new GameSpeed(2f),
                new GameSpeed(0.75f),
                new BrokeAllDoor(),                        // 모든 미닫이문 부수기
                new MonsterSound(),                        // 환청
                new AddHealth(100),                        // +100 HP
                new AddHealth(50),                         // +50 HP
                new AddHealth(0),                          // 0 HP
                new AddHealth(-25),                        // -25 HP
                new HideHealthUI(),                        // 체력 숨기기
                new HideSprintUI(),                        // 기력 숨기기
                new JointAreDoors(),                       // 모든 서랍 문을 지도에 표시
                new DuckMadness(),                         // 축지법 쓰는 오리
                new SayRandomThnings(),                    // 무작위 대사 말하기
                new ShakeScreen(),                         // 지진
                new IncreaseValuableWorth(2f),
                new IncreaseValuableWorth(0.9f),
                new SpinEternally(),                       // 회전회오리이이이
                new FriendlyMonsters(),                    // 친화적인 몬스터
                new VoicePitch(1.5f,"evt_voice_high"),     // 목소리 음조절 x1.5
                new VoicePitch(0.7f,"evt_voice_low"),      // 목소리 음조절 x0.7
                new DisablePostProcessing(),               // 포스트 프로세싱 비활성화
                new InvincibleOrb(),                       // 티타늄 오브
                new ReviveAllPlayers(),                    // 죽은 플레이어 소생
                new NoJumpAndSprint(),                     // 달리기 & 점프 없음
                new EyeBig(),                              // 동공 확장
                new DisableLighting(),                     // 전역 조명 비활성화
                // new CarCrash(false),                       // 올해 최고의 운전사
                new SpawnMonster(2),                       // 몬스터 2마리 소환
                new SpawnMonster(4),                       // 몬스터 4마리 소환
                new NoGravity(),                           // 중력 없음
                new ThinkFast(),                           // think fast ---
                new ClubMood(),                            // 클럽 분위기
                new GravityMult(0.25f),                    // 중력 0.25배
                new GravityMult(5f),                       // 중력 5배
                new TimeMult(2f),                          // 타이머 2배
                new TimeMult(0.5f),                        // 타이머 0.5배
                new TimeMult(4f),                          // 타이머 4배
                // new OilFloor(),
                // 모든 플레이어가 무적
                // 모든 몬스터가 무적
                // 술 취함
            };

            foreach (Modifier mod in modifiers)
            {
                if (callback != null)
                    callback(mod);
                Events.Add(mod);
            }

            // 혹시 모르니 지운다.
            //if (ChaosMod.Instance != null)
            //{
            //    // 설정에서 킨 개발자 모드가 아닐때
            //    if (ChaosMod.IsDebug && !ChaosMod.Instance.DevMode)
            //    {
            //        var mods = new List<Modifier>() {
            //            new OilFloor(),
            //            new ClubMood(),
            //            new TimeMult(0.5f),
            //        };

            //        foreach (Modifier mod in mods)
            //        {
            //            DebugEvents.Add(mod);
            //        }
            //    }
            //}

            //List<Modifier> ShopMods = new List<Modifier>() {
            //    new Shop_DiscountEvent(),
            //    new Modifier("무효",""){isOnce=true},
            //    new EyeBig(),
            //    new DisableLighting(),
            //    new NoGravity(),
            //    new ReviveAllPlayers(),
            //    new DisablePostProcessing()
            //};

            //foreach (Modifier mod in ShopMods)
            //{
            //    if (callback != null && modifiers.Find(m => m.name == mod.name) == null)
            //        callback(mod);
            //    ShopEvents.Add(mod);
            //}
        }

        public static readonly List<Modifier> Excludes = new List<Modifier>();
        public static bool CheckExcludes(Modifier mod)
        {
            List<Modifier> list = Excludes.FindAll(e =>
            {
                return e.GetName() == mod.GetName();
            });
            return list.Count > 0;
        }
    }

    public class ModifierOptions
    {
        /// <summary>
        /// 이벤트가 실행될 확률입니다. (0% ~ 100% 범위)
        /// </summary>
        public float chance = 1f;

        /// <summary>
        /// 이벤트가 레벨 당 한번씩만 실행되어야 하는지 여부입니다.
        /// </summary>
        public bool oncePerLevel = false;

        /// <summary>
        /// 이벤트가 멀티플레이에서만 실행되어야 하는지 여부입니다.
        /// </summary>
        public bool multiplayerOnly = false;

        /// <summary>
        /// 이벤트가 싱글플레이에서만 실행되어야 하는지 여부입니다.
        /// </summary>
        public bool singleplayerOnly = false;

        public ModifierOptions(float chance = 1f, bool oncePerLevel = false, bool multiplayerOnly = false, bool singleplayerOnly = false)
        {
            this.chance = Mathf.Clamp01(chance);
            this.oncePerLevel = oncePerLevel;
            this.multiplayerOnly = multiplayerOnly;
            this.singleplayerOnly = singleplayerOnly;
        }
    }

    public class Modifier
    {
        /// <summary>
        /// 이 이벤트의 "원본" 인스턴스입니다.
        /// </summary>
        public Modifier Instance { get; set; }

        /// <summary>
        /// 이 이벤트가 복제품인지 여부입니다.
        /// </summary>
        public bool isClone = false;

        /// <summary>
        /// 이 이벤트가 끝마쳐졌는지 여부입니다.
        /// </summary>
        public bool finihshed = false;

        /// <summary>
        /// 이 이벤트가 한 번만 실행되어야 하는지 여부입니다.
        /// </summary>
        public bool isOnce = false;

        /// <summary>
        /// 이 이벤트의 최대 시간 값입니다.
        /// </summary>
        public float maxTimer = 10f;
        
        /// <summary>
        /// 이 이벤트의 최소 시간 값입니다.
        /// </summary>
        public float minTimer = 9f;

        /// <summary>
        /// 이 이벤트의 남은 시간입니다.
        /// </summary>
        public float timerSelf = 0f;

        /// <summary>
        /// 이 이벤트의 이름입니다.
        /// </summary>
        public string name;

        /// <summary>
        /// 이 이벤트의 설정입니다.
        /// </summary>
        public ModifierOptions options;

        /// <summary>
        /// 이 이벤트의 설명입니다.
        /// </summary>
        public string description = "";

        /// <summary>
        /// 이 이벤트가 실행 중인지 여부입니다.
        /// </summary>
        public bool isRunning { get => timerSelf > 0f; }

        // 이걸 진작에 만들었어야 되는데...
        /// <summary>
        /// 이 이벤트만의 특별한 ID입니다.
        /// </summary>
        public int ID = 0;

        /// <summary>
        /// 이 이벤트가 실행되는지의 여부입니다.
        /// </summary>
        public bool enabled = true;

        public Modifier(string name, string description)
        {
            this.name = name;
            this.description = description;
            options = new ModifierOptions();
            if (!isClone)
                Instance = this;
        }

        /// <summary>
        /// 이벤트가 처음 시작될 때 호출되는 함수입니다.
        /// </summary>
        public virtual void Start()
        {
            timerSelf = GetTime();

            if (ChaosMod.IsDebug)
                ChaosMod.Logger.LogInfo("새 이벤트: " + name);

            if (options.oncePerLevel)
            {
                if (!Modifiers.Excludes.Contains(this))
                    Modifiers.Excludes.Add(this);
            }
        }

        /// <summary>
        /// 이 이벤트의 시작 시간을 반환합니다.
        /// </summary>
        public virtual float GetTime()
        {
            if (isOnce) return 0;
            return UnityEngine.Random.Range(minTimer, maxTimer);
        }

        /// <summary>
        /// 이 이벤트의 이름을 반환합니다.
        /// </summary>
        public virtual string GetName()
        {
            return Language.GetText(name);
        }

        /// <summary>
        /// 이 이벤트의 설명을 반환합니다.
        /// </summary>
        public virtual string GetDesc()
        {
            return Language.GetText(description);
        }
        
        /// <summary>
        /// 이벤트가 활성화 된 동안 프레임 당 호출되는 함수입니다.
        /// </summary>
        public virtual void Update()
        {

        }

        /// <summary>
        /// 이 이벤트가 끝날 때 호출되는 함수입니다.
        /// </summary>
        public virtual void OnFinished()
        {
            finihshed = true;
        }

        /// <summary>
        /// 이 이벤트를 복제합니다.
        /// </summary>
        public virtual Modifier Clone()
        {
            Modifier Instance = new Modifier(name, description);
            Instance.maxTimer = maxTimer;
            Instance.minTimer = minTimer;
            Instance.timerSelf = timerSelf;
            Instance.ID = ID;
            Instance.options = new ModifierOptions(options.chance, options.oncePerLevel, options.multiplayerOnly, options.singleplayerOnly);
            Instance.isClone = true;
            Instance.Instance = this;
            return Instance;
        }

        /// <summary>
        /// 이 이벤트 설정을 저장할때 사용됩니다.
        /// </summary>
        public virtual void Write(BinaryWriter bw)
        {
            bw.Write(ID);
            bw.Write(enabled);
            bw.Write(isOnce);
            bw.Write(minTimer);
            bw.Write(maxTimer);
        }

        public virtual void Read(BinaryReader br)
        {
            if (br.ReadInt32() != ID)
            {

                return;
            }

            enabled = br.ReadBoolean();
            //isOnce = br.ReadBoolean();
            br.ReadBoolean();
            minTimer = br.ReadSingle();
            maxTimer = br.ReadSingle();
        }
    }

    public class ModVars
    {
        public static bool brokeAllDoor_didRun;

        public static void Reset()
        {
            brokeAllDoor_didRun = false;
        }
    }
}