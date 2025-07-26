using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using System.Collections;
using UnityEngine.Rendering;
using REPOLib.Modules;

namespace ChaosMod
{
    public class SpawnMonster: Modifier
    {
        private string monsterPrefabPath = "enemies";
        private GameObject[] monsters;
        public int count = 1;
        public SpawnMonster(int count): base("몬스터 스폰", "몬스터를 스폰합니다.")
        {
            this.count = count;
            isOnce = true;
            name = $"몬스터 {count}마리 소환";

            monsters = Resources.LoadAll<GameObject>(monsterPrefabPath);
        }

        public override void Start()
        {
            base.Start();
            if (ChaosMod.IsDebug)
                ChaosMod.Logger.LogInfo("Spawn " + count + " enemys");
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            var points = GameObject.FindObjectsByType<LevelPoint>(0);
            foreach (var mod in Modifiers.Events)
            {
                if (mod is SpawnMonster && !Modifiers.Excludes.Contains(mod.Instance))
                    Modifiers.Excludes.Add(mod.Instance);
            }

            int choosen = 0;
            List<EnemySetup> enemies = [
                ..EnemyDirector.instance.enemiesDifficulty1,
                ..EnemyDirector.instance.enemiesDifficulty2,
                ..EnemyDirector.instance.enemiesDifficulty3
            ];

            foreach (EnemySetup monster in enemies)
            {
                if (choosen >= count) break;
                if (Random.Range(0f, 1f) <= 0.6f) continue;

                Enemies.SpawnEnemy(monster, points[UnityEngine.Random.Range(0, points.Length)].transform.position, Quaternion.identity);
                ChaosMod.Logger.LogInfo($"적 스폰됨: {monster.name}");

                choosen += 1;
            }
        }

        public override Modifier Clone()
        {
            return new SpawnMonster(count) { isClone = true, Instance = this };
        }
    }

    public class Tumble: Modifier
    {
        public Tumble(): base("넘어지기", "강제로 넘어집니다.")
        {
            isOnce = true;
        }

        public override void Start()
        {
            base.Start();

            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                foreach (PlayerTumble tumble in GameObject.FindObjectsByType<PlayerTumble>(0))
                {
                    tumble.TumbleSet(true, false);
                    tumble.TumbleOverrideTime(Random.Range(2f, 3f));
                }
            }
        }

        public override Modifier Clone()
        {
            return new Tumble() { isClone = true, Instance = this };
        }
    }

    public class GameSpeed: Modifier
    {
        public float mult = 1f;
        public GameSpeed(float mult, string name): base("게임 속도", "게임 속도를 늦추거나 빠르게 합니다.")
        {
            this.name = name;
            minTimer = 20f;
            maxTimer = 25f;
            this.mult = mult;
        }

        public override void Start()
        {
            base.Start();
            foreach (var e in Modifiers.Events)
            {
                if (e is GameSpeed)
                    Modifiers.Excludes.Add(e.Instance);
            }
            Time.timeScale = mult;
        }

        public override void OnFinished()
        {
            base.OnFinished();
            foreach (var e in Modifiers.Events)
            {
                if (e is GameSpeed)
                    Modifiers.Excludes.Remove(e.Instance);
            }
            while (Time.timeScale != 1)
            {
                Time.timeScale = 1;
            }
        }

        public override Modifier Clone()
        {
            return new GameSpeed(mult,name) { isClone = true, Instance = this };
        }
    }

    public class KillAllMonsters: Modifier
    {
        public KillAllMonsters(): base("모든 몬스터 죽이기", "강제로 모든 몬스터를 죽입니다.")
        {
            isOnce = true;
        }

        public override void Start()
        {
            base.Start();
            foreach (var enemy in GameObject.FindObjectsByType<EnemyHealth>(0))
            {
                enemy.Hurt(int.MaxValue, Vector3.zero);
            }
        }

        public override Modifier Clone()
        {
            return new KillAllMonsters() { isClone = true, Instance = this };
        }
    }

    public class QuakeFOV: Modifier
    {
        public QuakeFOV(): base("넓은 시야각", "시야각을 강제로 넓힙니다.")
        {
            minTimer = 20f;
            maxTimer = 30f;
        }

        public override void Update()
        {
            base.Update();
            CameraZoom.Instance.OverrideZoomSet(ChaosMod.Instance.ConfigDizzyness.Value ? 150f : 90f, .5f, 0.5f, 1f, ChaosMod.Instance.gameObject, 10);
        }

        public override Modifier Clone()
        {
            return new QuakeFOV() { isClone = true, Instance = this };
        }
    }

    public class BrokeAllDoor: Modifier
    {
        public BrokeAllDoor(): base("모든 미닫이문 부수기", "모든 출입문, 서랍문 등이 부숴집니다.")
        {
            isOnce = true;
        }

        public override void Start()
        {
            base.Start();

            if (ModVars.brokeAllDoor_didRun)
            {
                foreach (var @object in GameObject.FindObjectsByType<HingeJoint>(0))
                {
                    @object.GetComponent<PhysGrabObjectImpactDetector>().BreakHeavy(Vector3.zero, true);
                }
                return;
            }

            ModVars.brokeAllDoor_didRun = true;
            foreach (var @object in GameObject.FindObjectsByType<HingeJoint>(0))
            {
                if (/*@object.transform.name.Contains("Hinge")*/ true)
                {
                    // 물리 충격을 주어서 문 부수기
                    Rigidbody rb = @object.GetComponent<Rigidbody>();
                    rb.AddForce(Vector3.forward * 5000f, ForceMode.Impulse);
                }
            }
        }

        public override Modifier Clone()
        {
            var lmao = new BrokeAllDoor();
            if (ModVars.brokeAllDoor_didRun)
            {
                Instance.name = "문 확인사살";
                Instance.options.oncePerLevel = true;
            }
            lmao.isClone = true;
            lmao.Instance = this;
            return lmao;
        }
    }

    public class MonsterSound: Modifier
    {
        public string aud_name;
        public string modName;
        public int length;
        public List<AudioClip> sounds;

        public MonsterSound(string modName, string name, int length): base("환청", "진짜일까요? 아니면 그냥 망상이였을까요?")
        {
            aud_name = name;
            this.length = length;
            this.modName = modName;
            this.name = modName;
            isOnce = true;
            
            sounds = new List<AudioClip>();
            if (length <= 1)
            {
                sounds.Add(ChaosMod.Instance.assets.LoadAsset<AudioClip>(name));
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    sounds.Add(ChaosMod.Instance.assets.LoadAsset<AudioClip>($"{name}{i}"));
                }
            }
        }

        public override string GetName()
        {
            return Language.GetText("환청");
        }

        public override void Start()
        {
            base.Start();

            ((MonsterSound)Instance).options.chance -= 0.05f;
            if (length > 1)
                ChaosMod.Instance.EnemyAS.PlayOneShot(sounds[Random.Range(1,length)], 0.5f);
            else
                ChaosMod.Instance.EnemyAS.PlayOneShot(sounds[0], 0.5f);
        }

        public override Modifier Clone()
        {
            return new MonsterSound(name, aud_name, length) { isClone = true, Instance = this };
        }
    }

    public class AddHealth: Modifier
    {
        public int amount = 0;
        public AddHealth(int amount): base("체력 추가", "플레이어를 힐하거나 딜합니다")
        {
            this.amount = amount;
            name = (amount > 0 ? "+" : "") + $"{amount} HP";
            isOnce = true;
            options.oncePerLevel = true;
        }

        public override void Start()
        {
            base.Start();

            if (amount == 0)
                return;

            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                foreach (var plr in GameObject.FindObjectsByType<PlayerHealth>(0))
                {
                    bool isMinus = Mathf.Sign(amount) < 0;
                    if (isMinus)
                        plr.Hurt(Mathf.Abs(amount), true);
                    else
                        plr.Heal(Mathf.Abs(amount));
                }
            }

            foreach (var mod in Modifiers.Events)
            {
                if (mod is AddHealth)
                    Modifiers.Excludes.Add(mod);
            }
        }

        public override Modifier Clone()
        {
            return new AddHealth(amount) { isClone = true, Instance = this };
        }
    }

    public class HideHealthUI : Modifier
    {
        public HideHealthUI(): base("체력 숨기기", "체력 UI를 숨깁니다.")
        {
            minTimer = 20f;
            maxTimer = 35f;
        }

        public override void Update()
        {
            base.Update();

            var inst = HealthUI.instance;
            inst.Hide();
        }

        public override void OnFinished()
        {
            base.OnFinished();

            var inst = HealthUI.instance;
            inst.Show();
        }

        public override Modifier Clone()
        {
            return new HideHealthUI() { isClone = true, Instance = this };
        }
    }

    public class HideSprintUI : Modifier
    {
        public HideSprintUI() : base("기력 숨기기", "에너지 UI를 숨깁니다.")
        {
            minTimer = 20f;
            maxTimer = 35f;
        }

        public override void Update()
        {
            base.Update();

            var inst = EnergyUI.instance;
            inst.Hide();
        }

        public override void OnFinished()
        {
            base.OnFinished();

            var inst = HealthUI.instance;
            inst.Show();
        }

        public override Modifier Clone()
        {
            return new HideSprintUI() { isClone = true, Instance = this };
        }
    }

    public class DisablePostProcessing: Modifier
    {
        public DisablePostProcessing(): base("포스트 프로세싱 비활성화", "화면에 적용된 효과를 제거합니다.")
        {
            minTimer = 30f;
            maxTimer = 60f;
        }

        public static GameObject post;
        public override void Start()
        {
            base.Start();
            if (post == null) post = GameObject.FindAnyObjectByType<PostProcessing>().transform.parent.gameObject;
            post.SetActive(false);

        }

        public override Modifier Clone()
        {
            return new DisablePostProcessing() { isClone = true, Instance = this };
        }

        public override void OnFinished()
        {
            base.OnFinished();
            post.SetActive(true);
        }
    }

    public class JointAreDoors : Modifier
    {
        public JointAreDoors(): base("모든 서랍 문을 지도에 표시", "서랍 문, 냉장고 문 등을 지도에 표시합니다.")
        {
            isOnce = true;
            options.oncePerLevel = true;
        }

        public override void Start()
        {
            base.Start();

            var doorTarget = GameObject.FindAnyObjectByType<DirtFinderMapDoorTarget>();
            if (doorTarget == null)
            {
                if (ChaosMod.IsDebug)
                    ChaosMod.Logger.LogWarning("DirtFinderMapDoorTarget을 찾지 못하였습니다.");
                return;
            }

            foreach (var @object in GameObject.FindObjectsByType<HingeJoint>(0))
            {
                if (!@object.TryGetComponent<DirtFinderMapDoor>(out var map))
                {
                    var door = @object.gameObject.AddComponent<DirtFinderMapDoor>();
                    door.DoorPrefab = doorTarget.gameObject;
                }
            }
        }

        public override Modifier Clone()
        {
            return new JointAreDoors() { isClone = true, Instance = this };
        }
    }

    public class DuckMadness: Modifier
    {
        private string rubberDuckPath = "items/Item Rubber Duck";
        private float chanceBefore = 0f;
        private GameObject rubberDuckPrefab;

        public DuckMadness(): base("축지법 쓰는 오리", "고무 오리가 축지법을 씁니다.")
        {
            minTimer = 25f;
            maxTimer = 30f;

            rubberDuckPrefab = Resources.Load<GameObject>(rubberDuckPath);
            if (rubberDuckPrefab == null)
                ChaosMod.Logger.LogError("rubberDuckPrefab이 존재하지 않습니다!");
        }

        public List<ItemRubberDuck> ducks = new List<ItemRubberDuck>();
        public override void Start()
        {
            base.Start();
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            chanceBefore = ((DuckMadness)Instance).options.chance;
            ((DuckMadness)Instance).options.chance = 0f;

            var players = SemiFunc.PlayerGetAll();
            foreach (var player in players)
            {
                var pos = player.transform.position + Vector3.up;
                var rot = player.transform.rotation;

                // 각 플레이어마다 5개의 고무 오리를 소환
                for (int i = 0; i < 9-players.Count; i++)
                {
                    // 고무 오리 아이템 복제
                    GameObject @object = null;
                    if (GameManager.Multiplayer())
                        @object = PhotonNetwork.Instantiate(rubberDuckPath, pos, rot);
                    else
                        @object = GameObject.Instantiate(rubberDuckPrefab, pos, rot);

                    // 오리 던지기
                    var rig = @object.GetComponent<Rigidbody>();
                    rig.isKinematic = false;
                    rig.AddForce(@object.transform.forward * 100f);

                    @object.GetComponent<ItemBattery>().autoDrain = false;
                    @object.GetComponent<ItemBattery>().batteryActive = false;

                    // 스크립트를 리스트에 추가
                    var duck = @object.GetComponent<ItemRubberDuck>();
                    ((DuckMadness)Instance).ducks.Add(duck);
                }
            }
        }

        public override void Update()
        {
            base.Update();
            
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            foreach (var duck in ((DuckMadness)Instance).ducks)
            {
                var grabber = duck.GetComponent<PhysGrabObject>();

                // 고무 오리의 속도를 계속 높게 하기
                var rig = duck.GetComponent<Rigidbody>();
                rig.AddForce(duck.transform.forward * 100f);
                if (rig.velocity.magnitude > 500f)
                    rig.velocity = rig.velocity.normalized * 500f;

                // 플레이어가 고무 오리를 잡지 못하게 하기
                if (grabber.playerGrabbing.Count > 0 && SemiFunc.IsMasterClientOrSingleplayer())
                    foreach (var plr in grabber.playerGrabbing)
                    {
                        if (plr == null) continue;

                        if (!GameManager.Multiplayer())
                            plr.ReleaseObject();
                        else
                            plr.photonView.RPC("ReleaseObjectRPC", RpcTarget.All, false, 0.1f);
                    }
            }
        }

        public override void OnFinished()
        {
            base.OnFinished();
            
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            for (int i = 0; i < ((DuckMadness)Instance).ducks.Count; i++)
            {
                var obj = ((DuckMadness)Instance).ducks[i].gameObject;
                if (obj == null)
                    continue;

                if (GameManager.Multiplayer())
                    PhotonNetwork.Destroy(obj);
                else
                    Object.Destroy(obj);
            }
            ((DuckMadness)Instance).ducks.Clear();
        }

        public override Modifier Clone()
        {
            return new DuckMadness() { isClone = true, Instance = this };
        }
    }

    public class JumpJumpJump: Modifier
    {
        public JumpJumpJump(): base("점핑! 예!", "플레이어가 1초마다 점프합니다.")
        {
            minTimer = 20f;
            maxTimer = 25f;
        }

        public override Modifier Clone()
        {
            return new JumpJumpJump() { isClone = true, Instance = this };
        }

        float jumpTimer = 1f;
        public override void Update()
        {
            base.Update();

            // if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            if (jumpTimer > 0f)
                jumpTimer -= Time.deltaTime;
            else
            {
                jumpTimer = 1f;
                var players = SemiFunc.PlayerGetAll();
                foreach (var plr in players)
                {
                    plr.Jump(true);
                }
                CameraJump.instance.Jump();
            }
        }
    }

    public class SayRandomThnings: Modifier
    {
        public static string[] tries =
        {
            "What the fuck did you just fucking say about me, you little bitch?",
            "You're suck at this game.", "git gud", "goo goo ga ga.", "AAAAAAAAAAAAAAAAAAAAAAAA",
            "OOOOOOOOOOOOOOOOOO", "I love you.", "I hate you.", "go fuck yourself.",
            "That monster I fucking hate", "That robot I fucking hate", "I love this game.",
            "You are an idiot.", "Do you have one digit IQ?", "son of a bitch.", "jaundice",
            "goo goo ga ga?", "I have nothing to say.", "I ran out of ideas.", "I LOOOVEEEE",
            "75000 DOLLARS?? OH MY GOSH", "I feel so REPO!", "Tung Tung Tung Tung Tung Tung Tung Tung Tung Sahur",
            "Brr Brr Patapim", "adorable", "I hate everything.", "The cake is a lie.", "This is Certified hood classic.",
            "This is the part where he kills you.", "skill issue", "CHICKEN JOCKEY", "skibidi",
            "I. AM. STEVE.", "THanks for playing my mod!"
        };

        public static string[] familyfriendly =
        {
            "You're doing great!", "You're good at this game.", "goo goo ga ga.", "AAAAAAAAAAAAAAAAAAAAAAAA",
            "OOOOOOOOOOOOOOOOOO", "I love you.", "I hate that monster.", "I love this game.", "jaundice",
            "goo goo ga ga?", "I have nothing to say.", "I ran out of ideas.", "I LOOOVEEEE",
            "75000 DOLLARS?? OH MY GOSH", "I feel so REPO!", "Tung Tung Tung Tung Tung Tung Tung Tung Tung Sahur",
            "Brr Brr Patapim", "adorable", "The cake is a lie.", "This is Certified hood classic.",
            "This is the part where he kills you!", "CHICKEN JOCKEY", "skibidi", "I. AM. STEVE.", "Thanks for playing my mod!"
        };

        public SayRandomThnings(): base("무작위 대사 말하기", "무작위 대사를 말합니다. (멀티플레이어 전용)")
        {
            isOnce = true;
            options.multiplayerOnly = true;
        }

        public override void Start()
        {
            base.Start();
            ChatManager chat = ChatManager.instance;

            string speech = tries[Random.Range(0, tries.Length)];
            if (ChaosMod.Instance.ConfigFamilyFriendly.Value)
            {
                string[] cussWords = { "fuck", "fucking", "suck", "bitch", "idiot", "kill" };
                foreach (string word in cussWords)
                {
                    if (speech.Contains(word))
                        speech = speech.Replace(word, "(REDACTED)");
                }
            }

            chat.PossessChatScheduleStart(4);
            chat.PossessChat(ChatManager.PossessChatID.None, speech, 1.75f, Color.Lerp(Color.yellow, new Color(0f, 0.5f, 1f), Random.Range(0f, 1f)));
            chat.PossessChatScheduleEnd();
        }

        public override Modifier Clone()
        {
            return new SayRandomThnings() { isClone = true, Instance = this };
        }
    }

    public class ShakeScreen: Modifier
    {
        // 너무 어지러움 줄여서 너어
        public ShakeScreen(): base("지진", "강도 5.5의 지진")
        {
            minTimer = 15f;
            maxTimer = 20f;
        }

        public override Modifier Clone()
        {
            return new ShakeScreen() { isClone = true, Instance = this };
        }

        public override void Start()
        {
            base.Start();
            if (!ChaosMod.Instance.ConfigDizzyness.Value)
                return;

            // ChaosMod.Instance.StartCoroutine(AudioFadeIn());
            ChaosMod.Instance.RumbleAS.volume = .75f;
            ChaosMod.Instance.RumbleAS.Play();
        }

        public override void Update()
        {
            base.Update();
            if (!ChaosMod.Instance.ConfigDizzyness.Value)
                return;

            GameDirector.instance.CameraShake.Shake(2f, .5f);
            GameDirector.instance.CameraImpact.Shake(2f, .5f);
        }

        public override void OnFinished()
        {
            base.OnFinished();
            if (!ChaosMod.Instance.ConfigDizzyness.Value)
                return;

            ChaosMod.Instance.StartCoroutine(AudioFadeOut());
        }

        IEnumerator AudioFadeIn()
        {
            AudioSource src = ChaosMod.Instance.RumbleAS;
            src.volume = 0f;
            src.Play();

            System.Func<float, float> ease = t => Mathf.Pow(t, 3.322f);
            float step = 0f;
            while (step < 1f)
            {
                src.volume = ease(step) * .75f;
                step += Time.unscaledDeltaTime;
                yield return new WaitForEndOfFrame();
            }
            src.volume = 1f;
        }

        IEnumerator AudioFadeOut()
        {
            AudioSource src = ChaosMod.Instance.RumbleAS;
            src.volume = 1f;

            System.Func<float, float> ease = t => Mathf.Pow(-t, 3.322f) + 1f;
            float step = 0f;
            while (step < 1f)
            {
                src.volume = ease(step) * .75f;
                step += Time.unscaledDeltaTime;
                yield return new WaitForEndOfFrame();
            }
            src.volume = 0f;
            src.Stop();
        }
    }

    public class IncreaseValuableWorth : Modifier
    {
        public float mult = 1f;

        public IncreaseValuableWorth(float mult, string name): base("귀중품 가격 상승", "모든 귀중품의 가격을 올리거나 내립니다.")
        {
            isOnce = true;
            this.mult = mult;
            this.name = name;
        }

        public override Modifier Clone()
        {
            return new IncreaseValuableWorth(mult, name) { isClone = true, Instance = this };
        }

        public override void Start()
        {
            base.Start();

            List<ValuableObject> items = ValuableDirector.instance.valuableList;
            if (items == null || items.Count == 0)
            {
                if (ChaosMod.IsDebug)
                    Debug.LogWarning("귀중품을 찾지 못했습니다. 가격 상승은 없습니다.");
                return;
            }

            foreach (var item in items)
            {
                if (item == null)
                {
                    if (ChaosMod.IsDebug)
                        ChaosMod.Logger.LogWarning("아이템 하나를 찾지 못하였습니다. 다음 아이템으로 스킵합니다.");
                    continue;
                }

                var dollarValueCurrent = Util.GetInternalVar(item, "dollarValueCurrent");
                var dollarValueOriginal = Util.GetInternalVar(item, "dollarValueOriginal");

                float ogMoney = (float)dollarValueCurrent.GetValue(item);
                ogMoney *= mult;

                dollarValueCurrent.SetValue(item, Mathf.RoundToInt(ogMoney));
                dollarValueOriginal.SetValue(item, Mathf.Max(ogMoney, (float)dollarValueOriginal.GetValue(item)));
            }

            foreach (var mod in Modifiers.Events)
            {
                if (mod is IncreaseValuableWorth)
                    Modifiers.Excludes.Add(mod);
            }
        }
    }

    public class SpinEternally: Modifier
    {
        public SpinEternally(): base("회전회오리이이이", "몬스터 베이블레이드")
        {
            minTimer = 20f;
            maxTimer = 40f;
        }

        public override Modifier Clone()
        {
            return new SpinEternally() { isClone = true, Instance = this };
        }

        private List<EnemyRigidbody> rbList;
        public override void Start()
        {
            base.Start();

            rbList = GameObject.FindObjectsByType<EnemyRigidbody>(0).ToList();
            // 몬스터 스턴 주기
            foreach (var rb in rbList)
            {
                if (rb == null) continue;
                rb.enemy.GetComponent<EnemyStateStunned>().Set(timerSelf + 5f);
            }
        }

        public override void Update()
        {
            base.Update();

            // 베이블레이드
            foreach (var rb in rbList)
            {
                if (rb == null) continue;

                var realRB = rb.GetComponent<Rigidbody>();
                if (realRB.angularVelocity.magnitude <= 1500f)
                    realRB.AddTorque(rb.transform.forward * 500f, ForceMode.Impulse);
            }
        }
    }

    public class FriendlyMonsters: Modifier
    {
        public FriendlyMonsters(): base("친화적인 몬스터", "몬스터가 공격하지 않습니다.")
        {
            minTimer = 60f;
            maxTimer = 120f;
        }

        public override Modifier Clone()
        {
            var sex = new FriendlyMonsters();
            sex.isClone = true;
            sex.Instance = this;
            sex.options.chance = options.chance;
            return sex;
        }

        public override void Start()
        {
            base.Start();
            if (!Modifiers.Excludes.Contains(Instance))
                Modifiers.Excludes.Add(Instance);
            Util.GetInternalVar(EnemyDirector.instance, "debugNoVision").SetValue(EnemyDirector.instance, true);
        }

        public override void OnFinished()
        {
            base.OnFinished();
            if (Modifiers.Excludes.Contains(Instance))
                Modifiers.Excludes.Remove(Instance);
            Util.GetInternalVar(EnemyDirector.instance, "debugNoVision").SetValue(EnemyDirector.instance, false);
        }
    }

    public class NoJumpAndSprint: Modifier
    {
        public NoJumpAndSprint(): base("점프 & 달리기 없음", "말 그대로.")
        {
            minTimer = 20f;
            maxTimer = 40f;
        }

        public override void Update()
        {
            base.Update();

            PlayerController.instance.EnergyCurrent = 0f;
            PlayerController.instance.OverrideJumpCooldown(timerSelf);
        }

        public override void OnFinished()
        {
            base.OnFinished();

            PlayerController.instance.EnergyCurrent = PlayerController.instance.EnergyStart;
            PlayerController.instance.OverrideJumpCooldown(0f);
        }

        public override Modifier Clone()
        {
            return new NoJumpAndSprint() { isClone = true, Instance = this };
        }
    }

    public class VoicePitch: Modifier
    {
        public float pitch = 0f;

        public VoicePitch(float pitch, string name): base("목소리 음조절", "목소리의 음을 조절합니다.")
        {
            minTimer = 30f;
            maxTimer = 60f;

            this.name = name;

            options.multiplayerOnly = true;
            this.pitch = pitch;
        }

        public override Modifier Clone()
        {
            return new VoicePitch(pitch, name) { isClone = true, Instance = this };
        }

        public float timeShit = 0f;
        public override void Update()
        {
            base.Update();

            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            if (timeShit > 0f)
                timeShit -= Time.unscaledDeltaTime;
            else
            {
                timeShit = .5f;
                foreach (var pa in SemiFunc.PlayerGetAll())
                {
                    var voiceChat = Util.GetInternalVar<PlayerVoiceChat>(pa, "voiceChat");
                    voiceChat.OverridePitch(pitch, 1f, 1f, 0.5f);
                }
            }

        }
    }

    public class InvincibleOrb : Modifier
    {
        public InvincibleOrb() : base("티타늄 오브", "오브가 절대로 깨지지 않습니다.")
        {
            minTimer = 40f;
            maxTimer = 70f;
        }

        public override void Update()
        {
            base.Update();

            foreach (var orb in GameObject.FindObjectsByType<EnemyValuable>(0))
            {
                orb.GetComponent<PhysGrabObjectImpactDetector>().destroyDisable = true;
            }
        }

        public override Modifier Clone()
        {
            return new InvincibleOrb() { isClone = true, Instance = this };
        }
    }

    public class EyeBig: Modifier
    {
        public EyeBig(): base("동공 확장", "눈이 커집니다.")
        {
            minTimer = 30f;
            maxTimer = 42.5f;
            options.multiplayerOnly = true;
        }

        public override void Update()
        {
            base.Update();
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            foreach (PlayerAvatar avatar in SemiFunc.PlayerGetAll())
            {
                avatar.OverridePupilSize(2f, 10, 1f, 1f, 5f, 0.5f);
            }
        }

        public override Modifier Clone()
        {
            return new EyeBig() { isClone = true, Instance = this };
        }
    }

    public class ReviveAllPlayers: Modifier
    {
        public ReviveAllPlayers(): base("죽은 플레이어 소생", "모든 죽은 플레이어를 살립니다.")
        {
            isOnce = true;
        }

        public override void Start()
        {
            base.Start();
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            foreach (var plr in SemiFunc.PlayerGetAll())
            {
                if (Util.GetInternalVar<bool>(plr, "deadSet"))
                {
                    plr.Revive();
                    // 2
                    plr.playerHealth.Heal(1);
                }
            }
        }

        public override Modifier Clone()
        {
            return new ReviveAllPlayers() { isClone = true, Instance = this };
        }
    }

    public class SpawnValuables: Modifier
    {
        public SpawnValuables(): base("귀중품 소환", "무작위 귀중품을 아무 곳에 소환합니다.")
        {
            isOnce = true;
            options.oncePerLevel = true;
        }

        public override void Start()
        {
            base.Start();

            
        }

        public override Modifier Clone()
        {
            return new SpawnValuables() { isClone = true, Instance = this };
        }
    }

    public class DisableLighting: Modifier
    {
        public DisableLighting(): base("전역 조명 비활성화", "게임 내 전체 조명 효과를 비활성화 합니다.")
        {
            minTimer = 20f;
            maxTimer = 40f;
        }

        public AmbientMode ogAmbientMode;
        public Color ogAmbientLight;

        public override void Start()
        {
            base.Start();

            DisableLighting light = (DisableLighting)Instance;
            light.ogAmbientMode = RenderSettings.ambientMode;
            light.ogAmbientLight = RenderSettings.ambientLight;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.white;

            Instance.options.chance = 0f;
        }

        public override void OnFinished()
        {
            base.OnFinished();

            DisableLighting light = (DisableLighting)Instance;
            RenderSettings.ambientMode = light.ogAmbientMode;
            RenderSettings.ambientLight = light.ogAmbientLight;

            Instance.options.chance = 1f;
        }

        public override Modifier Clone()
        {
            return new DisableLighting() { isClone = true, Instance = this };
        }
    }

    public class CarCrash: Modifier
    {
        public static AssetBundle car_assets;
        public CarCrash(): base("올해의 운전사", "반어법입니다.")
        {
            minTimer = 120f;
            maxTimer = 180f;
            if (car_assets == null)
            {
                car_assets = AssetBundle.LoadFromFile(Util.GetPluginDirectory("car_assets"));
                ChaosMod.Instance.PrefabToAddNetwork.Add(car_assets.LoadAsset<GameObject>("Killer Joe"));
            }
        }

        public GameObject carObject;
        public CrazyCarAIScript car;

        public override void Start()
        {
            base.Start();
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            CarCrash realInstance = (CarCrash)Instance;

            if (!GameManager.Multiplayer())
                realInstance.carObject = GameObject.Instantiate(car_assets.LoadAsset<GameObject>("Killer Joe"), Vector3.zero, Quaternion.identity);
            else
                realInstance.carObject = PhotonNetwork.Instantiate("Killer Joe", Vector3.zero, Quaternion.identity);
            realInstance.car = realInstance.carObject.AddComponent<CrazyCarAIScript>();
            realInstance.car.honk = car_assets.LoadAsset<AudioClip>("car honk");
            realInstance.car.exp_sprites = car_assets.LoadAssetWithSubAssets<Sprite>("spr_realisticexplosion").ToList();
            foreach (var lp in GameObject.FindObjectsByType<LevelPoint>(0))
            {
                realInstance.car.waypoints.Add(lp.transform.position);
            }

            Modifiers.Excludes.Add(Instance);
        }

        public override void OnFinished()
        {
            base.OnFinished();

            CarCrash realInstance = (CarCrash)Instance;
            realInstance.car.Death();

            Modifiers.Excludes.Remove(Instance);
        }

        public override Modifier Clone()
        {
            return new CarCrash() { isClone = true, Instance = this };
        }
    }

    public class NoGravity: Modifier
    {
        public NoGravity(): base("중력 없음", "...")
        {
            minTimer = 60f;
            maxTimer = 120f;
        }

        public override void Update()
        {
            base.Update();

            PlayerController.instance.AntiGravity(0.1f);
        }
    }
}