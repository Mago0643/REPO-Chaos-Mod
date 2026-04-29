using Photon.Pun;
using REPOLib.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace ChaosMod
{
    public class SpawnMonster: Modifier
    {
        private string monsterPrefabPath = "enemies";
        private GameObject[] monsters;
        public int count = 1;
        public SpawnMonster(int count): base("evt_spawn_monsters", "evt_spawn_monsters_desc")
        {
            this.count = count;
            isOnce = true;
            ID = 1;

            monsters = Resources.LoadAll<GameObject>(monsterPrefabPath);
        }

        public override string GetName()
        {
            return Language.GetText(name).Replace("%f", count.ToString());
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

                var parents = Enemies.SpawnEnemy(monster, points[UnityEngine.Random.Range(0, points.Length)].transform.position, Quaternion.identity);
                foreach (EnemyParent p in parents) {
                    if (!ChaosMod.Instance.spawnedEnemys.Contains(p))
                        ChaosMod.Instance.spawnedEnemys.Add(p);
                }
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
        public Tumble(): base("evt_tumble", "evt_tumble_desc")
        {
            isOnce = true;
            ID = 2;
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
        public GameSpeed(float mult): base("evt_game_speed", "evt_game_speed_desc")
        {
            minTimer = 20f;
            maxTimer = 25f;
            this.mult = mult;
            ID = 3;
        }

        public override string GetName()
        {
            return Language.GetText(name).Replace("%f", mult.ToString());
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
            return new GameSpeed(mult) { isClone = true, Instance = this };
        }
    }

    public class KillAllMonsters: Modifier
    {
        public KillAllMonsters(): base("evt_kill_all_monsters", "evt_kill_all_monsters_desc")
        {
            isOnce = true;
            ID = 4;
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
        public QuakeFOV(): base("evt_quake_fov", "evt_quake_fov_desc")
        {
            minTimer = 20f;
            maxTimer = 30f;
            ID = 5;
        }

        public override void Update()
        {
            base.Update();
            CameraZoom.Instance.OverrideZoomSet(ChaosMod.Instance.dizzyness ? 150f : 90f, .5f, 0.5f, 1f, ChaosMod.Instance.gameObject, 10);
        }

        public override Modifier Clone()
        {
            return new QuakeFOV() { isClone = true, Instance = this };
        }
    }

    public class BrokeAllDoor: Modifier
    {
        public BrokeAllDoor(): base("evt_break_all_door", "evt_break_all_door_desc")
        {
            isOnce = true;
            ID = 6;
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

    // 버그: 소리 안들림
    public class MonsterSound: Modifier
    {
        public MonsterSound(): base("evt_hallucination", "evt_hallucination_desc")
        {
            isOnce = true;
            ID = 7;
        }

        public override void Start()
        {
            base.Start();

            // ((MonsterSound)Instance).options.chance -= 0.05f;
            // ChaosMod.Instance.EnemyAS.PlayOneShot(sounds[Random.Range(1, sounds.Count)], 0.5f);
            AudioClip clip = null;
            float volume = 1f;
            float pitch = 1f;
            bool getout = false;
            foreach (EnemyParent enemy in GameObject.FindObjectsByType<EnemyParent>(0)) {
                if (Random.Range(0f, 1f) <= 0.5f) { // 50% chance
                    // checks if it is right enemy to play a sound
                    // hardcode for most enemies
                    switch (enemy.name) {
                        case "Clown": {
                            var anim = enemy.transform.GetComponentInChildren<EnemyBeamerAnim>(true);
                            clip = anim.soundAttackIntro.Sounds[0];
                            volume = anim.soundAttackIntro.Volume;

                            getout = true;
                            break;
                        }
                        case "Animal": {
                            var anim = enemy.transform.GetComponentInChildren<EnemyAnimalAnim>(true);
                            clip = anim.noticeSound.Sounds[Random.Range(0, anim.noticeSound.Sounds.Length)];
                            volume = 0.3f;

                            getout = true;
                            break;
                        }
                        case "Chef": {
                            getout = true;
                            break;
                        }
                        case "Cleanup Crew": {
                            var anim = enemy.transform.GetComponentInChildren<EnemyBombThrowerAnim>(true);
                            clip = anim.soundTorsoHeadsScream.Sounds[0];
                            volume = 0.45f;
                            getout = true;
                            break;
                        }
                        case "Apex Predator": {
                            var anim = enemy.transform.GetComponentInChildren<EnemyDuckAnim>(true);
                            clip = anim.noticeSound.Sounds[Random.Range(0, anim.noticeSound.Sounds.Length)];
                            volume = 0.4f;
                            getout = true;
                            break;
                        }
                        case "Elsa": {
                            var anim = enemy.transform.GetComponentInChildren<EnemyElsaAnim>(true);
                            clip = anim.barkSmallSound.Sounds[Random.Range(0, anim.barkSmallSound.Sounds.Length)];
                            volume = 0.375f + Random.Range(0f, 0.02f);
                            pitch += Random.Range(0f, 0.1f);
                            getout = true;
                            break;
                        }
                        case "Mentalist": {
                            var anim = enemy.transform.GetComponentInChildren<EnemyFloaterAnim>(true);
                            clip = anim.sfxMoveLong.Sounds[Random.Range(0, anim.sfxMoveLong.Sounds.Length)];
                            volume = 0.3f + Random.Range(0f, 0.05f);
                            pitch += Random.Range(0f, 0.1f);
                            getout = true;
                            break;
                        }
                        case "Gnome": {
                            var anim = enemy.transform.GetComponentInChildren<EnemyGnomeAnim>(true);
                            clip = anim.soundNotice.Sounds[Random.Range(0, anim.soundNotice.Sounds.Length)];
                            volume = 0.2f + Random.Range(0f, 0.05f);
                            pitch += Random.Range(0f, 0.05f);
                            getout = true;
                            break;
                        }
                        case "Headman": {
                            var anim = enemy.transform.GetComponentInChildren<EnemyHeadAnimationSystem>(true);
                            clip = anim.ChaseBegin.Sounds[0]; // i wanted to find that piano sound but theres too many sounds to find one
                            volume = anim.ChaseBegin.Volume;
                            getout = true;
                            break;
                        }
                        case "Headgrab": {
                            var anim = enemy.transform.GetComponentInChildren<EnemyHeadGrabberAnim>(true);
                            clip = anim.soundNotice.Sounds[0];
                            volume = 0.4f;
                            break;
                        }
                        case "Heart Hugger": {
                            var scr = enemy.transform.GetComponentInChildren<EnemyHeartHugger>(true);
                            clip = scr.soundGrow.Sounds[Random.Range(0, scr.soundGrow.Sounds.Length)];
                            volume = 0.5f + Random.Range(0f, 0.1f);
                            pitch += Random.Range(0f, 0.1f);
                            getout = true;
                            break;
                        }
                        case "Hidden": {
                            var anim = enemy.transform.GetComponentInChildren<EnemyHiddenAnim>(true);
                            clip = anim.soundBreatheInFast.Sounds[Random.Range(0, anim.soundBreatheInFast.Sounds.Length)];
                            volume = 0.1f + Random.Range(0f, 0.02f);
                            pitch += Random.Range(0f, 0.1f);
                            getout = true;
                            break;
                        }
                        case "Huntsman": {
                            var anim = enemy.transform.GetComponentInChildren<EnemyHunterAnim>(true);
                            clip = anim.aimStartClips[Random.Range(0, anim.aimStartClips.Length)];
                            volume = 0.5f;
                            getout = true;
                            break;
                        }
                        case "Trudge": {
                            var anim = enemy.transform.GetComponentInChildren<EnemySlowWalkerAnim>(true);
                            clip = anim.sfxNoticeVoice.Sounds[Random.Range(0, anim.sfxNoticeVoice.Sounds.Length)];
                            volume = 0.5f + Random.Range(0f, 0.1f);
                            pitch += Random.Range(0f, 0.1f);
                            getout = true;
                            break;
                        }
                        case "Tick": {
                            var anim = enemy.transform.GetComponentInChildren<EnemyTickAnim>(true);
                            clip = anim.noticeSound.Sounds[Random.Range(0, anim.noticeSound.Sounds.Length)];
                            volume = 0.5f + Random.Range(0f, 0.1f);
                            pitch += Random.Range(0f, 0.1f);
                            getout = true;
                            break;
                        }
                        case "Bella": {
                            var scr = enemy.transform.GetComponentInChildren<EnemyTricycle>(true);
                            clip = scr.audioBell.Sounds[Random.Range(0, scr.audioBell.Sounds.Length)];
                            volume = 0.8f;
                            getout = true;
                            break;
                        }
                        case "Rugrat": {
                            var anim = enemy.transform.GetComponentInChildren<EnemyValuableThrowerAnim>(true);
                            clip = anim.noticeSound.Sounds[0];
                            volume = 0.4f + Random.Range(0f, 0.05f);
                            pitch += Random.Range(0f, 0.1f);
                            getout = true;
                            break;
                        }
                    }
                }

                if (getout) break;
            }

            if (clip != null) {
                ChaosMod.Instance.EnemyAS.pitch = pitch;
                ChaosMod.Instance.EnemyAS.PlayOneShot(clip, volume);
            }
        }

        public override Modifier Clone()
        {
            return new MonsterSound() { isClone = true, Instance = this };
        }
    }

    public class AddHealth: Modifier
    {
        public int amount = 0;
        public AddHealth(int amount): base("체력 추가", "evt_health_desc")
        {
            this.amount = amount;
            name = (amount >= 0 ? "+" : "") + $"{amount} HP";
            isOnce = true;
            options.oncePerLevel = true;
            ID = 8;
        }

        public override string GetName()
        {
            return name;
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
        public HideHealthUI(): base("evt_hide_health", "evt_hide_health_desc")
        {
            minTimer = 20f;
            maxTimer = 35f;
            ID = 9;
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
        public HideSprintUI() : base("evt_hide_sprint", "evt_hide_sprint_desc")
        {
            minTimer = 20f;
            maxTimer = 35f;
            ID = 10;
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
        public DisablePostProcessing(): base("evt_disable_post_processing", "evt_disable_post_processing_desc")
        {
            minTimer = 30f;
            maxTimer = 60f;
            ID = 11;
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
        public JointAreDoors(): base("evt_show_all_drawers_in_map", "evt_show_all_drawers_in_map_desc")
        {
            isOnce = true;
            options.oncePerLevel = true;
            ID = 12;
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
        private GameObject rubberDuckPrefab;

        public DuckMadness(): base("evt_crazy_ducks", "evt_crazy_ducks_desc")
        {
            minTimer = 25f;
            maxTimer = 30f;

            ID = 13;

            rubberDuckPrefab = Resources.Load<GameObject>(rubberDuckPath);
            if (rubberDuckPrefab == null)
                ChaosMod.Logger.LogError("rubberDuckPrefab이 존재하지 않습니다!");
        }

        public List<ItemRubberDuck> ducks = new List<ItemRubberDuck>();
        public override void Start()
        {
            base.Start();
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            if (!Modifiers.Excludes.Contains(Instance))
                Modifiers.Excludes.Add(Instance);

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
                            plr.ReleaseObject(duck.GetComponent<PhotonView>().ViewID);
                        else
                            plr.photonView.RPC("ReleaseObjectRPC", RpcTarget.All, false, duck.GetComponent<PhotonView>().ViewID, 0.1f);
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

            if (Modifiers.Excludes.Contains(Instance))
                Modifiers.Excludes.Remove(Instance);
        }

        public override Modifier Clone()
        {
            return new DuckMadness() { isClone = true, Instance = this };
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
            "I. AM. STEVE.", "THanks for playing my mod!", "six sevennn"
        };

        public static string[] familyfriendly =
        {
            "You're doing great!", "You're good at this game.", "goo goo ga ga.", "AAAAAAAAAAAAAAAAAAAAAAAA",
            "OOOOOOOOOOOOOOOOOO", "I love you.", "I hate that monster.", "I love this game.", "jaundice",
            "goo goo ga ga?", "I have nothing to say.", "I ran out of ideas.", "I LOOOVEEEE",
            "75000 DOLLARS?? OH MY GOSH", "I feel so REPO!", "Tung Tung Tung Tung Tung Tung Tung Tung Tung Sahur",
            "Brr Brr Patapim", "adorable", "The cake is a lie.", "This is Certified hood classic.",
            "This is the part where he kills you!", "CHICKEN JOCKEY", "skibidi", "I. AM. STEVE.", "Thanks for playing my mod!",
        };

        public SayRandomThnings(): base("evt_say_random_thing", "evt_say_random_thing_desc")
        {
            isOnce = true;
            options.multiplayerOnly = true;
            ID = 14;
        }

        public override void Start()
        {
            base.Start();
            ChatManager chat = ChatManager.instance;

            string speech = tries[Random.Range(0, tries.Length)];
            if (ChaosMod.Instance.familyFriendly)
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
        public ShakeScreen(): base("evt_earthquake", "evt_earthquake_desc")
        {
            minTimer = 15f;
            maxTimer = 20f;
            ID = 15;
        }

        public override Modifier Clone()
        {
            return new ShakeScreen() { isClone = true, Instance = this };
        }

        public override void Start()
        {
            base.Start();
            if (!ChaosMod.Instance.dizzyness)
                return;

            // ChaosMod.Instance.StartCoroutine(AudioFadeIn());
            ChaosMod.Instance.RumbleAS.volume = 1f;
            ChaosMod.Instance.RumbleAS.Play();
        }

        public override void Update()
        {
            base.Update();
            if (!ChaosMod.Instance.dizzyness)
                return;

            GameDirector.instance.CameraShake.Shake(2f, .5f);
            GameDirector.instance.CameraImpact.Shake(2f, .5f);
        }

        public override void OnFinished()
        {
            base.OnFinished();
            if (!ChaosMod.Instance.dizzyness)
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
                src.volume = ease(step);
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
                src.volume = ease(step);
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

        public IncreaseValuableWorth(float mult): base("귀중품 가격", "evt_increase_valuable_desc")
        {
            isOnce = true;
            this.mult = mult;
            ID = 16;
        }

        public override string GetName()
        {
            return Language.GetText("evt_increase_valuable").Replace("%f", mult.ToString());
        }

        public override Modifier Clone()
        {
            return new IncreaseValuableWorth(mult) { isClone = true, Instance = this };
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
        public SpinEternally(): base("evt_spin", "evt_spin_desc")
        {
            minTimer = 20f;
            maxTimer = 40f;
            ID = 17;
        }

        public override void Start()
        {
            base.Start();
            ChaosMod.Instance.spawnedEnemys = GameObject.FindObjectsByType<EnemyParent>(0).ToHashSet();
        }

        public override Modifier Clone()
        {
            return new SpinEternally() { isClone = true, Instance = this };
        }

        public override void Update()
        {
            base.Update();

            // 베이블레이드
            foreach (var enemy in ChaosMod.Instance.spawnedEnemys)
            {
                if (enemy == null) continue;

                var rb = enemy.GetComponent<EnemyRigidbody>();
                if (rb == null) continue;

                var realRB = rb.GetComponent<Rigidbody>();
                if (realRB.angularVelocity.magnitude <= 1500f)
                    realRB.AddTorque(rb.transform.forward * 500f, ForceMode.Impulse);
            }
        }
    }

    public class FriendlyMonsters: Modifier
    {
        public FriendlyMonsters(): base("evt_friendly_monsters", "evt_friendly_monsters_desc")
        {
            minTimer = 60f;
            maxTimer = 120f;
            ID = 18;
        }

        public override Modifier Clone()
        {
            var sex = new FriendlyMonsters();
            sex.isClone = true;
            sex.Instance = this;
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
        public NoJumpAndSprint(): base("evt_no_jump_sprint", "evt_no_jump_sprint_desc")
        {
            minTimer = 20f;
            maxTimer = 40f;
            ID = 19;
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
            ID = 20;
        }

        public override Modifier Clone()
        {
            return new VoicePitch(pitch, name) { isClone = true, Instance = this };
        }

        public float timeShit = 0f;
        public override void Update()
        {
            base.Update();

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
        public InvincibleOrb() : base("evt_titanume_orb", "evt_titanume_orb_desc")
        {
            minTimer = 40f;
            maxTimer = 70f;
            ID = 21;
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
        public EyeBig(): base("evt_big_eyes", "evt_big_eyes_desc")
        {
            minTimer = 30f;
            maxTimer = 42.5f;
            options.multiplayerOnly = true;
            ID = 22;
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
        public ReviveAllPlayers(): base("evt_revive_all", "evt_revive_all_desc")
        {
            ID = 23;
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
            ID = 24;
        }

        public override void Start()
        {
            base.Start();

            // 어찌해야 하오...
        }

        public override Modifier Clone()
        {
            return new SpawnValuables() { isClone = true, Instance = this };
        }
    }

    public class DisableLighting: Modifier
    {
        public DisableLighting(): base("evt_disable_all_lights", "evt_disable_all_lights_desc")
        {
            minTimer = 20f;
            maxTimer = 40f;
            ID = 25;
        }

        public AmbientMode ogAmbientMode;
        public Color ogAmbientLight;

        public override void Start()
        {
            base.Start();

            DisableLighting light = (DisableLighting)Instance;
            light.ogAmbientMode = RenderSettings.ambientMode;
            light.ogAmbientLight = RenderSettings.ambientLight;

            if (!Modifiers.Excludes.Contains(Instance))
                Modifiers.Excludes.Add(Instance);
        }

        public override void Update()
        {
            base.Update();

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.white;
            RenderSettings.fog = false;
        }

        public override void OnFinished()
        {
            base.OnFinished();

            DisableLighting light = (DisableLighting)Instance;
            RenderSettings.ambientMode = light.ogAmbientMode;
            RenderSettings.ambientLight = light.ogAmbientLight;
            RenderSettings.fog = true;

            if (Modifiers.Excludes.Contains(Instance))
                Modifiers.Excludes.Remove(Instance);
        }

        public override Modifier Clone()
        {
            return new DisableLighting();
        }
    }

    // 이것도 지금 생각해보면 많이 짜증남
    public class CarCrash: Modifier
    {
        public static AssetBundle car_assets;
        public CarCrash(bool isClone): base("evt_driver_of_the_year", "evt_driver_of_the_year_desc")
        {
            this.isClone = isClone;
            options.singleplayerOnly = true;
            minTimer = 120f;
            maxTimer = 180f;
            ID = 26;
            if (car_assets == null)
            {
                car_assets = AssetBundle.LoadFromFile(Util.GetPluginDirectory("car_assets"));
                ChaosMod.Instance.PrefabToAddNetwork.Add(car_assets.LoadAsset<GameObject>("Killer Joe"));
            }
        }

        public override void Start()
        {
            base.Start();

            ChaosMod.Instance.car.transform.position = Vector3.zero;
            ChaosMod.Instance.car.transform.rotation = Quaternion.identity;
            ChaosMod.Instance.car.Spawn();

            Modifiers.Excludes.Add(Instance);
        }

        public override void OnFinished()
        {
            base.OnFinished();

            ChaosMod.Instance.car.Despawn();
            Modifiers.Excludes.Remove(Instance);
        }

        public override Modifier Clone()
        {
            return new CarCrash(true) { isClone = true, Instance = this };
        }
    }

    public class NoGravity: Modifier
    {
        public NoGravity(): base("evt_moon_gravity", "evt_moon_gravity_desc")
        {
            minTimer = 60f;
            maxTimer = 120f;
            ID = 27;
        }

        public override void Update()
        {
            base.Update();

            PlayerController.instance.AntiGravity(0.1f);
        }

        public override Modifier Clone()
        {
            return new NoGravity() { isClone = true, Instance = this };
        }
    }

    public class ShowAD: Modifier
    {
        public ShowAD(): base("광고 시청", "개발자도 먹고 살아야죠...")
        {
            
        }

        public override float GetTime()
        {
            return ChaosMod.Instance.adViewer.GetLength();
        }

        public override void Start()
        {
            ChaosMod.Instance.adViewer.SetVideo();
            ChaosMod.Instance.adViewer.Show();

            base.Start();
        }

        public override void OnFinished()
        {
            base.OnFinished();

            ChaosMod.Instance.adViewer.Hide();
        }

        public override Modifier Clone()
        {
            return new ShowAD() { isClone = true, Instance = this };
        }
    }

    public class ThinkFast : Modifier
    {
        internal static Image flash;
        internal static Image scout;

        internal static GameObject text1;
        internal static GameObject text2;

        internal static string StunItemPath = "items/Item Grenade Stun";
        internal static GameObject StunItemPrefab;

        public ThinkFast(): base("evt_think_fast", "evt_think_fast_desc")
        {
            isOnce = true;
            options.oncePerLevel = true;
            ID = 28;

            StunItemPrefab = Resources.Load<GameObject>(StunItemPath);
        }

        public override void Start()
        {
            base.Start();

            ChaosMod.Instance.StartCoroutine(Flashback());
        }

        IEnumerator Flashback()
        {
            var inst = ChaosMod.Instance;
            AudioClip voice = inst.assets.LoadAsset<AudioClip>("Scout_stunballhit15");
            AudioClip flashback = inst.assets.LoadAsset<AudioClip>("flashbang");

            inst.AudioSource.PlayOneShot(voice);

            text1.SetActive(true);
            scout.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(0.578f);

            text2.SetActive(true);
            yield return new WaitForSecondsRealtime(0.667f);

            // flashback
            text1.SetActive(false);
            text2.SetActive(false);
            // inst.AudioSource.PlayOneShot(flashback);
            scout.gameObject.SetActive(false);
            
            //PlayerAvatar.instance.tumble.TumbleSet(true, false);
            //PlayerAvatar.instance.tumble.TumbleOverrideTime(2f);

            if (GameManager.Multiplayer())
            {
                GameObject itemStun = PhotonNetwork.Instantiate(StunItemPath, PlayerAvatar.instance.localCamera.transform.position + new Vector3(0f, 2f, 0f), Quaternion.identity);
                ChaosController.instance.view.RPC("GrenadeStunExplosionRPC", RpcTarget.All, itemStun.GetComponent<PhotonView>().ViewID);
            } else {
                GameObject itemStun = GameObject.Instantiate(StunItemPrefab, PlayerAvatar.instance.localCamera.transform.position + new Vector3(0f, 2f, 0f), Quaternion.identity);
                var script = itemStun.GetComponent<ItemGrenade>();
                script.tickTime = 0;
                Util.GetInternalVar(script, "isActive").SetValue(script, true);
            }

            //System.Func<float, float> easeInExpo = t => Mathf.Pow(2f, 10f * t - 10f);
            //float time = 0f;
            //while (time < flashback.length)
            //{
            //    float alpha = time / flashback.length;

            //    Color col = flash.color;
            //    col.a = 1f - easeInExpo(alpha);
            //    flash.color = col;

            //    time += Time.unscaledDeltaTime;
            //    yield return null;
            //}
            //flash.color = new Color(1,1,1,0);
        }

        public override Modifier Clone()
        {
            return new ThinkFast() { Instance = this, isClone = true };
        }
    }

    public class ForcePause: Modifier
    {
        public ForcePause(): base("일시정지", "어르으음!!!")
        {
            isOnce = true;
        }

        public override void Start()
        {
            base.Start();
            Util.GetInternalVar<MenuPage>(MenuPageEsc.instance, "menuPage").PageStateSet(MenuPage.PageState.Opening);
        }

        public override Modifier Clone()
        {
            return new ForcePause() { Instance = this, isClone = true };
        }
    }

    public class ClubMood: Modifier
    {
        public static float BPM = 140.000f;
        float bps = 0f;
        AudioSource src;
        public ClubMood(): base("evt_club_mood", "evt_club_mood_desc")
        {
            ID = 29;
            float tempBPS = 60f / BPM;

            while (minTimer < 60f)
            {
                minTimer += tempBPS;
            }
            maxTimer = minTimer;
            while (maxTimer < 90f)
            {
                maxTimer += tempBPS;
            }
        }

        private List<EnemyRigidbody> rbList;
        Color ogLight = Color.clear;
        private List<PlayerAvatar> players;
        private List<PhysGrabObject> items;
        public override void Start()
        {
            base.Start();

            foreach (var evt in Modifiers.Events)
            {
                if (evt is ClubMood)
                    Modifiers.Excludes.Add(evt);
            }

            ChaosMod.Instance.spawnedEnemys = GameObject.FindObjectsByType<EnemyParent>(0).ToHashSet();

            src = ChaosMod.Instance.ClubSource;
            src.volume = 0f;
            src.Play();

            bps = 60f / BPM;

            ogLight = RenderSettings.ambientLight;

            items = GameObject.FindObjectsByType<PhysGrabObject>(0).ToList();

            players = SemiFunc.PlayerGetAll();
            ChaosMod.Instance.StartCoroutine(MusicFadeIn());
        }

        IEnumerator MusicFadeIn()
        {
            float t = 0f;
            while (t < 1f)
            {
                src.volume = t;

                t += Time.deltaTime * 2f;
                yield return null;
            }

            src.volume = 1f;
        }

        int lastBeat = -1;
        Color[] colors = [
            Color.red, new Color(1,1,0), Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta
        ];
        Color curColor = Color.clear;
        Color lastColor = Color.blue;
        float fov = 70f;
        float baseFOV = 70f;
        public override void Update()
        {
            float beat = src.time / bps;
            if (Mathf.FloorToInt(beat) != lastBeat)
            {
                lastBeat = Mathf.FloorToInt(beat);

                if (SemiFunc.IsMasterClientOrSingleplayer())
                {
                    foreach (var enemy in ChaosMod.Instance.spawnedEnemys)
                    {
                        var rb = enemy.GetComponent<EnemyRigidbody>();
                        if (rb == null) continue;
                        rb.enemy.GetComponent<EnemyStateStunned>().Set(timerSelf + 5f);

                        var realRB = rb.GetComponent<Rigidbody>();
                        realRB.AddForce(Vector3.up * 15f, ForceMode.Impulse);
                    }


                    foreach (var valuable in items)
                    {
                        if (valuable == null) continue;
                        valuable.OverrideIndestructible(timerSelf + 3f);
                        bool fuckit = true;
                        if (valuable.TryGetComponent<PhysGrabObjectImpactDetector>(out var impact))
                            fuckit = !(impact.inCart || valuable.grabbed);
                        if (valuable.name.Contains("Cart"))
                            fuckit = false;

                        if ((bool)valuable.rb && fuckit)
                            valuable.rb.AddForce(Vector3.up * (3.5f * valuable.massOriginal), ForceMode.Impulse);
                    }
                }

                curColor = colors[Random.Range(0, colors.Length)];
                while (curColor == lastColor)
                {
                    curColor = colors[Random.Range(0, colors.Length)];
                }
                lastColor = curColor;

                fov = ChaosMod.Instance.dizzyness ? baseFOV - 5f : baseFOV - 2.5f;
            }
            RenderSettings.ambientLight = curColor;

            float headTilt = Mathf.Sin(Mathf.PI * 2f * beat) * 25f;

            foreach (var plr in players)
            {
                if (plr == null) continue;
                // local
                if (SemiFunc.PlayerAvatarLocal() == plr)
                {
                    plr.playerExpression.OverrideExpressionSet(4, 100f);
                    PlayerExpressionsUI.instance.playerExpression.OverrideExpressionSet(4, 100f);
                    PlayerExpressionsUI.instance.playerAvatarVisuals.HeadTiltOverride(headTilt * 0.5f);
                }
                else
                {
                    plr.playerAvatarVisuals.HeadTiltOverride(headTilt);
                }
            }

            CameraZoom.Instance.OverrideZoomSet(fov, .5f, 1000f, 1000f, ChaosMod.Instance.gameObject, 5);
            fov = Mathf.Lerp(fov, baseFOV, 1f - Mathf.Exp(-Time.deltaTime * 10f));

            float beatX = Util.BeatMath(beat, BPM, 0f);
            for (int i = 0; i < ChaosMod.Instance.texts.Count; i++)
            {
                TextLerp text = ChaosMod.Instance.texts[i];
                if (!(bool)text) continue;
                text.posOffset = new Vector2(beatX * (i % 2 == 0 ? 1f : -1f), 0f);
            }
        }

        IEnumerator MusicFadeOut()
        {
            AudioSource src = ChaosMod.Instance.ClubSource;
            float t = 0f;
            while (t < 1f)
            {
                src.volume = 1f-t;

                t += Time.deltaTime * 2f;
                yield return null;
            }

            src.volume = 0f;
        }

        public override void OnFinished()
        {
            foreach (var evt in Modifiers.Events)
            {
                if (evt is ClubMood)
                    Modifiers.Excludes.Remove(evt);
            }
            ChaosMod.Instance.StartCoroutine(MusicFadeOut());
            RenderSettings.ambientLight = ogLight;
            for (int i = 0; i < ChaosMod.Instance.texts.Count; i++)
            {
                TextLerp text = ChaosMod.Instance.texts[i];
                if (!(bool)text) continue;
                text.posOffset = Vector2.zero;
            }
            base.OnFinished();
        }

        public override Modifier Clone()
        {
            return new ClubMood() { Instance = this, isClone = true };
        }
    }

    public class Doomsday: Modifier
    {
        public Doomsday(): base("종말", "하루 뒤 지구가 멸망한다면? 하루 뒤라고 하면 한참 남았으니 지금 지구가 멸망하고 있다고 해두죠.")
        {
            minTimer = 30f;
            maxTimer = 60f;
        }

        private List<PlayerAvatar> players;
        private List<PhysGrabObject> items;

        public override void Start()
        {
            base.Start();

            items = GameObject.FindObjectsByType<PhysGrabObject>(0).ToList();
            // do not make cart and tumbled players go crazy move because it makes the game unplayable
            items.RemoveAll(x => x.transform.name.Contains("Cart") || x.transform.name.Contains("Tumble"));
            players = SemiFunc.PlayerGetAll();
            ChaosMod.Instance.spawnedEnemys = GameObject.FindObjectsByType<EnemyParent>(0).ToHashSet();
        }

        public override void Update()
        {
            if (SemiFunc.IsMasterClientOrSingleplayer()) {
                foreach (var enemy in ChaosMod.Instance.spawnedEnemys)
                {
                    var rb = enemy.GetComponent<EnemyRigidbody>();
                    if (rb == null) continue;
                    rb.enemy.GetComponent<EnemyStateStunned>().Set(timerSelf + 5f);

                    var realRB = rb.GetComponent<Rigidbody>();
                    if (realRB.velocity.magnitude <= 50f)
                        realRB.AddForce(realRB.transform.up * 10f, ForceMode.Impulse);
                    if (realRB.angularVelocity.magnitude <= 25f)
                        realRB.AddTorque(realRB.transform.forward * Random.Range(0.5f, 2f) * 10f, ForceMode.Impulse);
                }


                foreach (var valuable in items)
                {
                    if (valuable == null) continue;
                    // TODO: Do not affect if items are in the extraction or cart
                    valuable.OverrideIndestructible(timerSelf + 3f);
                    bool fuckit = true;
                    if (valuable.TryGetComponent<PhysGrabObjectImpactDetector>(out var impact))
                        fuckit = !(impact.inCart || valuable.grabbed);

                    if ((bool)valuable.rb && fuckit)
                    {
                        if (valuable.rb.velocity.magnitude <= 25f)
                            valuable.rb.AddForce(valuable.transform.up * (2f * valuable.massOriginal), ForceMode.Impulse);
                        if (valuable.rb.angularVelocity.magnitude <= 25f)
                            valuable.rb.AddTorque(valuable.transform.forward * (2f * Random.Range(0.5f, 2f) * valuable.massOriginal), ForceMode.Impulse);
                    }
                }
            }

            if (!ChaosMod.Instance.dizzyness)
                return;

            GameDirector.instance.CameraShake.Shake(2f, .5f);
            GameDirector.instance.CameraImpact.Shake(2f, .5f);
        }

        public override Modifier Clone()
        {
            return new Doomsday() { Instance = this, isClone = true };
        }
    }

    public class GravityMult: Modifier
    {
        internal float mult = 1f;
        public GravityMult(float mult): base("evt_gravity_mult", "evt_gravity_mult_desc")
        {
            this.mult = mult;
            minTimer = 30f;
            maxTimer = 60f;
            ID = 30;
        }

        public override string GetName()
        {
            return Language.GetText(name).Replace("%f", mult.ToString());
        }

        public override void Start()
        {
            base.Start();
            Physics.gravity = new Vector3(0f, -9.81f * mult, 0f);
            foreach (Modifier evt in Modifiers.Events)
            {
                if (evt is GravityMult)
                {
                    if (!Modifiers.Excludes.Contains(evt))
                        Modifiers.Excludes.Add(evt);
                }
            }
        }

        public override void OnFinished()
        {
            Physics.gravity = new Vector3(0f, -9.81f, 0f);
            foreach (Modifier evt in Modifiers.Events)
            {
                if (evt is GravityMult)
                {
                    if (Modifiers.Excludes.Contains(evt))
                        Modifiers.Excludes.Remove(evt);
                }
            }
            base.OnFinished();
        }

        public override Modifier Clone()
        {
            return new GravityMult(mult) { isClone = true, Instance = this };
        }
    }

    public class TimeMult: Modifier
    {
        internal float mult = 1f;
        public TimeMult(float mult): base("evt_timer_mult", "evt_timer_mult_desc")
        {
            this.mult = mult;
            minTimer = 30f;
            maxTimer = 60f;
            ID = 31;
        }

        public override string GetName()
        {
            return Language.GetText(name).Replace("%f", mult.ToString());
        }

        public override void Start()
        {
            base.Start();
            ChaosMod.Instance.controller.timeScale = mult;

            foreach (Modifier evt in Modifiers.Events)
            {
                if (evt is TimeMult)
                {
                    if (!Modifiers.Excludes.Contains(evt))
                        Modifiers.Excludes.Add(evt);
                }
            }
        }

        public override void OnFinished()
        {
            ChaosMod.Instance.controller.timeScale = 1f;
            foreach (Modifier evt in Modifiers.Events)
            {
                if (evt is TimeMult)
                {
                    if (Modifiers.Excludes.Contains(evt))
                        Modifiers.Excludes.Remove(evt);
                }
            }
            base.OnFinished();
        }

        public override Modifier Clone()
        {
            return new TimeMult(mult) { isClone = true, Instance = this };
        }
    }

    public class OilFloor: Modifier
    {
        public OilFloor(): base("evt_oil_floor", "evt_oil_floor_desc")
        {
            minTimer = 40f;
            maxTimer = 120f;
            ID = 32;
        }

        private List<PlayerAvatar> players = new List<PlayerAvatar>();
        public override void Start()
        {
            players = SemiFunc.PlayerGetAll();

            base.Start();
        }

        public override void Update()
        {
            foreach (PlayerAvatar plr in players) {
                ChaosMod.Logger.LogInfo(plr.GetComponent<Rigidbody>().drag);
            }
        }

        public override Modifier Clone()
        {
            return new OilFloor { Instance = this, isClone = true }; 
        }
    }

    public class RandomUpgrade: Modifier
    {
        public RandomUpgrade(): base("evt_random_upgrade", "evt_random_upgrade_desc")
        {
            isOnce = true;
            ID = 33;
        }

        public override void Start()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            foreach (PlayerAvatar semibot in SemiFunc.PlayerGetAll())
            {
                UpgradePlayer(semibot);
            }

            base.Start();
        }

        void UpgradePlayer(PlayerAvatar plr)
        {
            // TODO: Maybe think about upgrade mods??
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                string steamID = SemiFunc.PlayerGetSteamID(plr);
                string upgrade = "Map Player Count";
                var upgradeFunc = PunManager.instance.UpgradeMapPlayerCount;
                switch (Random.Range(0, 12))
                {
                    case 0:
                        upgradeFunc = PunManager.instance.UpgradeDeathHeadBattery;
                        upgrade = "Death Head Battery";
                        break;
                    case 1:
                        upgradeFunc = PunManager.instance.UpgradePlayerCrouchRest;
                        upgrade = "Crouch Rest";
                        break;
                    case 2:
                        upgradeFunc = PunManager.instance.UpgradePlayerEnergy;
                        upgrade = "Energy";
                        break;
                    case 3:
                        upgradeFunc = PunManager.instance.UpgradePlayerExtraJump;
                        upgrade = "Extra Jump";
                        break;
                    case 4:
                        upgradeFunc = PunManager.instance.UpgradePlayerGrabRange;
                        upgrade = "Grab Range";
                        break;
                    case 5:
                        upgradeFunc = PunManager.instance.UpgradePlayerGrabStrength;
                        upgrade = "Grab Strength";
                        break;
                    case 6:
                        upgradeFunc = PunManager.instance.UpgradePlayerHealth;
                        upgrade = "Health";
                        break;
                    case 7:
                        upgradeFunc = PunManager.instance.UpgradePlayerSprintSpeed;
                        upgrade = "Sprint Speed";
                        break;
                    //case 8: // i think this is unused? or in the future update?? or this is LORE???????????
                    //        // not adding it cuz felt not so.
                    //    upgradeFunc = PunManager.instance.UpgradePlayerThrowStrength;
                    //    upgrade = "Throw Strength";
                    //    break;
                    case 8:
                        // map upgrade is useless after getting one.
                        // reroll again
                        if (StatsManager.instance.playerUpgradeMapPlayerCount[steamID] > 0)
                            UpgradePlayer(plr);
                        break;
                    case 9:
                        upgradeFunc = PunManager.instance.UpgradePlayerTumbleClimb;
                        upgrade = "Tumble Climb";
                        break;
                    case 10:
                        upgradeFunc = PunManager.instance.UpgradePlayerTumbleLaunch;
                        upgrade = "Tumble Launch";
                        break;
                    case 11:
                        upgradeFunc = PunManager.instance.UpgradePlayerTumbleWings;
                        upgrade = "Tumble Wings";
                        break;
                }
                upgradeFunc(steamID, 1); // only 1 upgrade cuz it makes game pretty boring
                ChaosMod.Logger.LogInfo(plr.name + " got \"" + upgrade + "\" upgrade.");
            }

            // play animations
            if (SemiFunc.PlayerAvatarLocal() == plr)
            {
                StatsUI.instance.Fetch();
                StatsUI.instance.ShowStats();
                CameraGlitch.Instance.PlayUpgrade();
            } else { 
                GameDirector.instance.CameraImpact.ShakeDistance(5f, 1f, 6f, plr.transform.position, 0.2f);
            }

            if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
            {
                plr.playerHealth.MaterialEffectOverride(PlayerHealth.Effect.Upgrade);
            }
        }

        public override Modifier Clone()
        {
            return new RandomUpgrade() { Instance = this, isClone = true };
        }
    }

    public class RandomItems: Modifier
    {
        public RandomItems(): base("evt_random_items", "evt_random_items_desc")
        {
            isOnce = true;
            ID = 34;
            options.chance = 90f;
        }

        public override void Start()
        {
            string itemPrefabName = "items/";
            string[] items = new string[] { // there's a cart named 'Big Cart', it is quite bigger than Medium cart. maybe this is another LORE????
                "Item Cart Cannon", "Item Cart Laser", "Item Cart Small", "Item Cart Medium",
                "Item Drone Battery", "Item Drone Feather", "Item Drone Indestructible", "Item Drone Torque", "Item Drone Zero Gravity",
                "Item Duck Bucket", "Item Extraction Tracker", "Item Grenade Duct Taped", "Item Grenade Explosive", "Item Grenade Human", "Item Grenade Shockwave", "Item Grenade Stun",
                "Item Gun Handgun", "Item Gun Laser", "Item Gun Shockwave", "Item Gun Shotgun", "Item Gun Stun", "Item Gun Tranq",
                 "Item Health Pack Large", "Item Health Pack Medium", "Item Health Pack Small",
                "Item Melee Baseball Bat", "Item Melee Frying Pan", "Item Melee Inflatable Hammer", "Item Melee Sledge Hammer", "Item Melee Stun Baton", "Item Melee Sword",
                "Item Mine Explosive", "Item Mine Shockwave", "Item Mine Stun", "Item Orb Zero Gravity", "Item Phase Bridge", "Item Power Crystal",
                // this is an important one
                "Item Rubber Duck",
                "Item Upgrade Death Head Battery", "Item Upgrade Map Player Count", "Item Upgrade Player Crouch Rest", "Item Upgrade Player Energy", "Item Upgrade Player Extra Jump",
                "Item Upgrade Player Grab Range", "Item Upgrade Player Grab Strength", "Item Upgrade Player Health", "Item Upgrade Player Sprint Speed", "Item Upgrade Player Tumble Climb",
                "Item Upgrade Player Tumble Launch", "Item Upgrade Player Tumble Wings", "Item Valuable Tracker"
            };

            // TODO: Think about modded items
            // maybe i can get all items by messing with
            // REPOLib.Modules.NetworkPrefabs.PrefabRefs?? 

            foreach (PlayerAvatar semibot in SemiFunc.PlayerGetAll())
            {
                string itemName = itemPrefabName + items[Random.Range(0,items.Length)];
                GameObject item = GameObject.Instantiate(Resources.Load<GameObject>(itemName), semibot.transform.position + new Vector3(0f, 2f, 0f), Quaternion.identity);

                // do i need to save them forever? or not.
                // adding option later to turn off because think about when this event happens rapidly
                //ItemAttributes attr = item.GetComponent<ItemAttributes>();
                //StatsManager.instance.itemDictionary[attr.item.name] = attr.item;
                //StatsManager.instance.itemsPurchased[attr.item.name]++;
                //StatsManager.instance.itemsPurchasedTotal[attr.item.name]++;
            }

            base.Start();
        }

        public override Modifier Clone()
        {
            return new RandomItems() { Instance = this, isClone = true };
        }
    }

    public class InfinteStamina: Modifier
    {
        public InfinteStamina(): base("evt_infinite_stamina", "evt_infinite_stamina_desc")
        {
            minTimer = 60f;
            maxTimer = 120f;
            ID = 35;
        }

        public override void Update()
        {
            PlayerController.instance.EnergyCurrent = PlayerController.instance.EnergyStart;
        }

        public override Modifier Clone()
        {
            return new InfinteStamina() { Instance = this, isClone = true };
        }
    }

    public class Invincible: Modifier
    {
        public Invincible(): base("evt_invincible", "evt_invincible_desc")
        {
            minTimer = 60f;
            maxTimer = 120f;
            ID = 36;
        }

        public override void Start()
        {
            foreach (PlayerAvatar plr in SemiFunc.PlayerGetAll())
            {
                Util.GetInternalVar(plr.playerHealth, "godMode").SetValue(plr.playerHealth, true);
            }
            base.Start();
        }

        public override void OnFinished()
        {
            foreach (PlayerAvatar plr in SemiFunc.PlayerGetAll())
            {
                Util.GetInternalVar(plr.playerHealth, "godMode").SetValue(plr.playerHealth, false);
            }
            base.OnFinished();
        }


        public override Modifier Clone()
        {
            return new Invincible() { Instance = this, isClone = true };
        }
    }

    public class FuckGravity: Modifier
    {
        public FuckGravity(): base("evt_fuck_gravity", "evt_fuck_gravity_desc")
        {
            minTimer = 30f;
            maxTimer = 40f;
            ID = 39;
        }

        float gravity = 0f;
        public override void Start()
        {
            gravity = -9.81f;
            base.Start();
        }

        float rand()
        {
            return (Random.Range(0, 2) - 0.5f) * 0.3f;
        }

        public override void Update()
        {
            gravity += rand();
            Physics.gravity = new Vector3(0f, gravity, 0f);
            base.Update();
        }

        public override void OnFinished()
        {
            Physics.gravity = new Vector3(0f, -9.81f, 0f);
            base.OnFinished();
        }
    }

    public class InfiniteJump: Modifier
    {
        public InfiniteJump(): base("evt_infinite_jump", "evt_infinite_jump_desc")
        {
            minTimer = 60f;
            maxTimer = 120f;
            ID = 37;
        }

        private FieldInfo jumpCount;
        public override void Start()
        {
            jumpCount = Util.GetInternalVar(PlayerController.instance, "JumpExtraCurrent");
            jumpCount.SetValue(PlayerController.instance, 100);

            base.Start();
        }

        private float timer = 0f;
        public override void Update()
        {
            // 더 좋은 방법 추천받습니다
            // 1초에 100번 뛰는 사람은 없겠지???????????????????????????????????
            if (timer > 0f)
                timer -= Time.fixedDeltaTime;
            else {
                jumpCount.SetValue(PlayerController.instance, 100);
                timer = 1f;
            }
        }

        public override void OnFinished()
        {
            timer = 9e9f;
            Util.GetInternalVar(PlayerController.instance, "JumpExtraCurrent").SetValue(
                PlayerController.instance, Util.GetInternalVar<int>(PlayerController.instance, "JumpExtra")
            );

            base.OnFinished();
        }

        public override Modifier Clone()
        {
            return new InfiniteJump { Instance = this, isClone = true };
        }
    }

    // THIS IS NOT DONE YET!!
    public class PlayScarySound: Modifier
    {
        public PlayScarySound(): base("evt_doodoo_scary", "evt_doodoo_scary_desc")
        {
            isOnce = true;
        }

        public override void Start()
        {
            AudioClip clip = ChaosMod.Instance.assets.LoadAsset<AudioClip>("scary_sound" + Random.Range(0, 12));
            ChaosMod.Instance.AudioSource.PlayOneShot(clip);

            base.Start();
        }

        public override Modifier Clone()
        {
            return new PlayScarySound() { Instance = this, isClone = true };
        }
    }
}