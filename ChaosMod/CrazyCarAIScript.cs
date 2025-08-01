using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace ChaosMod
{
    public class CrazyCarAIScript : MonoBehaviour
    {
        public GameObject MapCustomPrefab;
        public PhotonView photonView;
        public List<Vector3> waypoints = new List<Vector3>();
        public List<Sprite> exp_sprites = new List<Sprite>();
        public float exp_anim_frame = 0.05f;
        public AudioClip honk;

        private AudioSource src;
        private AudioSource exp_src;
        private SpriteRenderer exp_rend;
        private List<Transform> FrontWheels = new List<Transform>();
        private List<Transform> BackWheels = new List<Transform>();

        private List<Light> frontLights = new List<Light>();
        private List<Light> backLights = new List<Light>();

        private List<Light> allLights = new List<Light>();

        private NavMeshAgent agent;
        private ParticleSystem spawnParticles;
        private Transform root;

        private Vector3 startPos;
        private Vector3 startEuler;
        private HurtCollider hurt;
        private string[] hitMessages = new string[]
        {
            "Wow, great driving.",                                                     // 와, 운전 끝내준다.
            "Nice driving, really.",                                                   // 운전 제대로 한다 진짜..
            "You're a real pro driver, huh?",                                          // 진짜 프로 운전자시네?
            "Wow, did you get your license from a cereal box?",                        // 운전면허를 시리얼 상자에서 뽑았니?
            "10 out of 10 driving skills.",                                            // 운전 점수는 10점 중에 10점이네.
            "Nice driving, genius.",                                                   // 운전 잘하네, 똑똑아.
            "Wow, great driving.",                                                     // 운전 참 잘한다.
            "Did you get your license in a lucky draw?",                               // 운전면허 뽑기로 땄어?
            "Ever heard of a turn signal?",                                            // 깜빡이라는 거 들어는 봤어?
            "Call an Ambulance!",                                                      // 구급차 불러요!
            "This driver must be the reason for those warning signs.",                 // 교통 경고 표지판은 저 운전자를 위해 만든 거구나..
            "Was that a human driving or a blindfolded chimp?",                        // 지금 운전한 거 사람이야? 눈가린 침팬지 아니고?
            "That’s supposed to be a human driving? A dog could’ve done a better job.",// 저게 사람이 운전하는 꼬라지라고? 개한테 시켜도 그것보단 잘하겠다.
            "That driver must be the reason for those warning signs.",                 // 교통 표지판은 저 운전자를 위한 거군.
            "YOU DRIVE LIKE YOU'RE PLAYING GRAND THEFT AUTO.",                         // 운전이 GTA 실사판이네?
        };
        private string[,] startMessages = new string[,]
        {
            {"Ah shit, here we go again.", "Ah shoot, here we go again."},                              // 젠장, 또 시작이네.
            {"That car i fucking hate.", "I hate that car so much."},                                   // 내가 존나 싫어하는 자동차임.
            {"Not again?", "Not again??"},                                                              // 또 시작이야?
            {"Great, now we are going to have bad time.", "Great, now we are going to have bad time."}, // 좋아, 이제 고생길 열렸네.
            {"ourple", "ourple"},                                                                       // 버라색
            {"IT ALWAYS COME BACK.", "IT ALWAYS COME BACK."}                                            // "그건 항상 돌아온다!"
        };
        private string[,] deadMessages = new string[,]
        {
            {"Finally, that stupid bitch is gone.", "Finally, some peace."},  // 드디어, 저 멍청한 놈이 떠났군.
            {"WAHOOOO", "WAHOOOO"},                                           // 와후!!
            {"Finally, some peace.", "Okay, That car is gone for now."}       // 드디어 조용해졌네.
        };
        private bool yapping = false;
        private UnityEvent chat_callback;

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                ChaosMod.Logger.LogError("NavMeshAgent가 없습니다.");
                return;
            }

            src = GetComponent<AudioSource>();
            if (src == null)
            {
                ChaosMod.Logger.LogError("AudioSource가 없습니다.");
                return;
            }

            photonView = GetComponent<PhotonView>();
            if (photonView == null)
            {
                ChaosMod.Logger.LogError("PhotonView가 없습니다.");
                return;
            }

            chat_callback = new UnityEvent();
            chat_callback.AddListener(() => yapping = false);

            hurt = GetComponent<HurtCollider>();
            hurt.onImpactPlayer.AddListener(delegate ()
            {
                if (!GameManager.Multiplayer() || yapping) return;

                yapping = true;
                ChatManager.instance.PossessChatScheduleStart(0x7FFFFFFF);
                ChatManager.instance.PossessChat(ChatManager.PossessChatID.Ouch, hitMessages[UnityEngine.Random.Range(0, hitMessages.Length)], 4f, Color.red, eventExecutionAfterMessageIsDone: chat_callback);
                ChatManager.instance.PossessChatScheduleEnd();
            });

            if (GameManager.Multiplayer() && SemiFunc.IsMasterClient())
            {
                int familyfriendly = ChaosMod.Instance.ConfigFamilyFriendly.Value ? 1 : 0;

                yapping = true;
                ChatManager.instance.PossessChatScheduleStart(0x7FFFFFFF);
                ChatManager.instance.PossessChat(ChatManager.PossessChatID.None, startMessages[UnityEngine.Random.Range(0, startMessages.GetLength(0)), familyfriendly], 4f, Color.red, eventExecutionAfterMessageIsDone: chat_callback);
                ChatManager.instance.PossessChatScheduleEnd();
            }

            Transform[] shits = GetComponentsInChildren<Transform>();
            foreach (Transform tr in shits)
            {
                if (tr.name.Contains("Front Wheel"))
                    FrontWheels.Add(tr);
                else if (tr.name.Contains("Back Wheel"))
                    BackWheels.Add(tr);
                else if (tr.name.Contains("Front Light"))
                {
                    Light l = tr.GetComponent<Light>();
                    frontLights.Add(l);
                    allLights.Add(l);
                }
                else if (tr.name.Contains("Back Light"))
                {
                    Light l = tr.GetComponent<Light>();
                    backLights.Add(l);
                    allLights.Add(l);
                }
                else if (tr.name == "root")
                    root = tr;
                else if (tr.name.Contains("Spawn"))
                    spawnParticles = tr.GetComponent<ParticleSystem>();
                else if (tr.name.Contains("Explosion"))
                {
                    exp_src = tr.GetComponent<AudioSource>();
                    exp_rend = tr.GetComponent<SpriteRenderer>();
                }
            }

            exp_src.outputAudioMixerGroup = src.outputAudioMixerGroup = AudioManager.instance.SoundMasterGroup;

            // agent.agentTypeID = -334000983;
            Spawn();
        }

        internal bool isSpawned = false;
        [PunRPC]
        void SpawnRPC()
        {
            exp_src.Stop();
            playAnimation = false;
            exp_rend.sprite = null;
            curAnimFrame = 0;
            isSpawned = true;
            src.Play();
            startPos = transform.position;
            startEuler = transform.eulerAngles;
            spawnParticles.Play();
            root.gameObject.SetActive(true);
            agent.isStopped = false;
            GetComponent<BoxCollider>().enabled = true;
            GoToNextPoint();
        }

        public void Spawn()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (GameManager.Multiplayer())
                photonView.RPC("SpawnRPC", RpcTarget.All);
            else
                SpawnRPC();
        }

        [PunRPC]
        void DespawnRPC()
        {
            exp_src.Play();
            curAnimTime = 0f;
            playAnimation = true;
            isSpawned = false;
        }

        public void Despawn()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (GameManager.Multiplayer())
                photonView.RPC("DespawnRPC", RpcTarget.All);
            else
                DespawnRPC();
        }

        [PunRPC]
        void DeathRPC()
        {
            Despawn();
            if (SemiFunc.IsMasterClientOrSingleplayer())
                StartCoroutine(WaitForDestroy());
        }

        IEnumerator WaitForDestroy()
        {
            while (exp_src.isPlaying)
            {
                yield return new WaitForEndOfFrame();
            }
            if (GameManager.Multiplayer())
                PhotonNetwork.Destroy(gameObject);
            else
                Destroy(gameObject);
        }

        public void Death()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (GameManager.Multiplayer())
                photonView.RPC("DeathRPC", RpcTarget.All);
            else
                DeathRPC();
        }

        [PunRPC]
        void HonkRPC()
        {
            if (!isSpawned && honkTimer > 0f) return;

            honkTimer = 1f;
            exp_src.PlayOneShot(honk, 0.4f);
        }

        void Honk()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            if (GameManager.Multiplayer())
                photonView.RPC("HonkRPC", RpcTarget.All);
            else
                HonkRPC();
        }

        public void GoToNextPoint()
        {
            if (waypoints.Count == 0 && !isSpawned) return;
            if (SemiFunc.IsMasterClientOrSingleplayer())
                SetDestination(waypoints[UnityEngine.Random.Range(0, waypoints.Count)]);
        }
        void Hide()
        {
            src.Stop();
            root.gameObject.SetActive(false);
            agent.isStopped = true;
            GetComponent<BoxCollider>().enabled = false;
        }

        void OnDestroy()
        {
            if (!GameManager.Multiplayer() && !SemiFunc.IsMasterClient()) return;
            int familyfriendly = ChaosMod.Instance.ConfigFamilyFriendly.Value ? 1 : 0;

            ChatManager.instance.PossessChatScheduleStart(0x7FFFFFFF);
            ChatManager.instance.PossessChat(ChatManager.PossessChatID.None, deadMessages[UnityEngine.Random.Range(0, deadMessages.GetLength(0)), familyfriendly], 4f, Color.red);
            ChatManager.instance.PossessChatScheduleEnd();
        }

        [PunRPC]
        void SetDestinationRPC(float x, float y, float z)
        {
            agent.destination = new Vector3(x, y, z);
            lastYRotation = transform.eulerAngles.y;
        }

        private void SetDestination(Vector3 pos)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (GameManager.Multiplayer())
                photonView.RPC("SetDestinationRPC", RpcTarget.All, pos.x, pos.y, pos.z);
            else
                SetDestinationRPC(pos.x, pos.y, pos.z);
        }

        private float rotateTimer = 0f;
        private float lastYRotation = 0f;
        private float rotationSpeed = 0f;
        private float lastAgentSpeed = 0f;
        private float curSpeedDelta = 0f;
        private int curAnimFrame = 0;
        private float curAnimTime = 0;
        private bool playAnimation = false;
        private bool needsToDestroy = false;
        private float honkTimer = 0f;
        void Update()
        {
            if (honkTimer > 0f)
                honkTimer -= Time.deltaTime;

            exp_rend.transform.LookAt(Camera.main.transform, Vector3.up);
            if (spawnParticles.isPlaying)
            {
                spawnParticles.transform.position = startPos;
                spawnParticles.transform.eulerAngles = startEuler;
            }

            if (agent.isOnNavMesh && !agent.pathPending && SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (agent.remainingDistance < 5f && isSpawned)
                {
                    GoToNextPoint();
                }
            }

            if (playAnimation)
            {
                if (curAnimTime > 0f)
                    curAnimTime -= Time.unscaledDeltaTime;
                else
                {
                    curAnimTime = exp_anim_frame;
                    exp_rend.sprite = exp_sprites[curAnimFrame];
                    if (curAnimFrame == 5)
                        Hide();

                    curAnimFrame = Mathf.Clamp(curAnimFrame + 1, 0, exp_sprites.Count - 1);
                }
            }

            float speedRot = agent.velocity.magnitude * 4f;
            float speedDelta = speedRot - lastAgentSpeed;
            lastAgentSpeed = speedRot;

            curSpeedDelta = Mathf.Floor(speedDelta * 100f) / 100f;
            if (Mathf.Abs(curSpeedDelta) <= 0.01f)
                curSpeedDelta = 0f;

            src.pitch = speedRot / agent.speed + 1f;

            float sign = Mathf.Sign(curSpeedDelta);
            foreach (Light l in backLights)
            {
                if (sign >= 0)
                    l.intensity = 0.5f;
                else if (sign < 0)
                    l.intensity = 1f;
            }

            float currentYRotation = transform.eulerAngles.y;
            float deltaAngle = Mathf.DeltaAngle(lastYRotation, currentYRotation);
            rotationSpeed = deltaAngle / Time.deltaTime;
            lastYRotation = currentYRotation;

            rotateTimer += Time.deltaTime * speedRot;
            float what = transform.eulerAngles.y - 90f;
            foreach (Transform front in FrontWheels)
            {
                front.eulerAngles = new Vector3(front.eulerAngles.x, what + Mathf.Clamp(rotationSpeed / agent.angularSpeed * 45f, -45f, 45f), -rotateTimer);
            }

            foreach (Transform back in BackWheels)
            {
                back.eulerAngles = new Vector3(back.eulerAngles.x, back.eulerAngles.y, -rotateTimer);
            }
        }

        Transform GetParentTransformByNameContains(Transform start, string name)
        {
            Transform crashed = start.transform.parent;
            while (true)
            {
                if (crashed.name.Contains(name))
                    break;

                if (crashed.parent != null)
                    crashed = crashed.parent;
                else break;
            }
            return crashed;
        }

        void FixedUpdate()
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

            Vector3 origin = transform.position + (transform.forward * 7.5f) + (Vector3.up * 3f);
            Vector3 halfExtents = new Vector3(1.25f, 1.25f, 0.5f); // 차 앞 범위
            Vector3 direction = transform.forward;
            float maxDistance = 20f;
            bool isHit = Physics.BoxCast(origin, halfExtents, direction, out RaycastHit hit, transform.rotation, maxDistance);
            if (isHit)
            {
                
                Transform crashed = GetParentTransformByNameContains(hit.transform, "Player");
                if (crashed.name.Contains("Player"))
                {
                    SetDestination(hit.transform.position);
                    if (honkTimer <= 0f)
                        Honk();
                }
            }
        }
    }
}