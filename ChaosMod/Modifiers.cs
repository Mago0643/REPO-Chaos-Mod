using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ChaosMod
{
    public class Modifiers
    {
        public static readonly List<Modifier> Events = new List<Modifier>();

        public static void Init(Action<Modifier> callback = null)
        {
            Modifier[] modifiers = {
                new Modifier("무효",""){isOnce = true},    // 무효
                new Tumble(),                              // 넘어지기
                new KillAllMonsters(),                     // 모든 몬스터 죽이기
                new QuakeFOV(),                            // 넓은 시야각
                new GameSpeed(2f, "게임 속도 두배"),
                new GameSpeed(0.75f, "게임 속도 -35%"),
                new BrokeAllDoor(),                        // 모든 미닫이문 부수기
                new MonsterSound("갓난쟁이", "rugurt", 1), // 갓난쟁이
                new MonsterSound("광대", "clown", 1),      // 광대
                new MonsterSound("짐승", "animal", 3),     // 짐승
                new MonsterSound("오리", "duck", 3),       // 오리
                new MonsterSound("사냥꾼", "hunter", 3),   // 사냥꾼
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
                new IncreaseValuableWorth(2f,"귀중품 가격 두배"),
                new IncreaseValuableWorth(0.9f,"귀중품 가격 -10%"),
                new SpinEternally(),                       // 회전회오리이이이
                new FriendlyMonsters(),                    // 친화적인 몬스터
                new VoicePitch(1.5f,"높은음 목소리"),      // 목소리 음조절 x1.5
                new VoicePitch(0.7f,"낮은음 목소리"),      // 목소리 음조절 x0.7
                new DisablePostProcessing(),               // 포스트 프로세싱 비활성화
                new InvincibleOrb(),                       // 티타늄 오브
                new ReviveAllPlayers(),                    // 죽은 플레이어 소생
                new NoJumpAndSprint(),                     // 달리기 & 점프 없음
                new EyeBig(),                              // 동공 확장
                new DisableLighting(),                     // 전역 조명 비활성화
                new CarCrash(false),                       // 올해 최고의 운전사
                new SpawnMonster(2),                       // 몬스터 2마리 소환
                new SpawnMonster(4),                       // 몬스터 4마리 소환
                new NoGravity(),                           // 중력 없음
                // 모든 플레이어가 무적
                // 모든 몬스터가 무적
                // CRT
                // 술 취함
            };

            foreach (Modifier mod in modifiers)
            {
                if (callback != null)
                    callback(mod);
                Events.Add(mod);
            }
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
        public bool multiplayerOnly = true;

        public ModifierOptions(float chance = 1f, bool oncePerLevel = false, bool multiplayerOnly = false)
        {
            this.chance = Mathf.Clamp01(chance);
            this.oncePerLevel = oncePerLevel;
            this.multiplayerOnly = multiplayerOnly;
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
        public bool isRunning
        {
            set { }
            get
            {
                return timerSelf > 0f;
            }
        }

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
            Instance.options = new ModifierOptions(options.chance, options.oncePerLevel);
            Instance.isClone = true;
            Instance.Instance = this;
            return Instance;
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