using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace ChaosMod
{
    public class CrazyCarAIScript : MonoBehaviour, IOnEventCallback
    {
        public PhotonView photonView;
        public List<Vector3> waypoints = new List<Vector3>();
        public List<Sprite> exp_sprites = new List<Sprite>();
        public float exp_anim_frame = 0.05f;
        public AudioClip honk;

        private MapCustom mapCustom;
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
            "You're a real pro driver?",                                               // 진짜 프로 운전자시네?
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
        private bool yapping = false;
        private UnityEvent chat_callback;

        void SetupComponents()
        {
            if (agent == null)
                agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                ChaosMod.Logger.LogError("NavMeshAgent가 없습니다.");
                return;
            }

            if (src == null)
                src = GetComponent<AudioSource>();
            if (src == null)
            {
                ChaosMod.Logger.LogError("AudioSource가 없습니다.");
                return;
            }
            src.outputAudioMixerGroup = AudioManager.instance.SoundMasterGroup;

            if (photonView == null)
                photonView = GetComponent<PhotonView>();
            if (photonView == null)
            {
                ChaosMod.Logger.LogError("PhotonView가 없습니다.");
                return;
            }

            if (chat_callback == null)
            {
                chat_callback = new UnityEvent();
                chat_callback.AddListener(() => yapping = false);
            }

            if (hurt == null)
            {
                hurt = GetComponent<HurtCollider>();
                hurt.onImpactPlayer.AddListener(delegate ()
                {
                    if (!GameManager.Multiplayer() || yapping) return;

                    yapping = true;
                    ChatManager.instance.PossessChatScheduleStart(0x7FFFFFFF);
                    ChatManager.instance.PossessChat(ChatManager.PossessChatID.Ouch, hitMessages[UnityEngine.Random.Range(0, hitMessages.Length)], 4f, Color.red, eventExecutionAfterMessageIsDone: chat_callback);
                    ChatManager.instance.PossessChatScheduleEnd();
                });
            }

            if (mapCustom == null)
                mapCustom = GetComponent<MapCustom>();
            if (mapCustom == null)
            {
                ChaosMod.Logger.LogError("MapCustom가 없습니다.");
                return;
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
                    exp_src.outputAudioMixerGroup = AudioManager.instance.SoundMasterGroup;
                    exp_rend = tr.GetComponent<SpriteRenderer>();
                }
            }
        }

        internal bool setupDone = false;
        internal void Start()
        {
            SetupComponents();
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                foreach (var lp in GameObject.FindObjectsByType<LevelPoint>(0))
                {
                    waypoints.Add(lp.transform.position);
                }
            }

            // agent.agentTypeID = -334000983;
            GetComponent<BoxCollider>().enabled = false;
            hurt.enabled = false;
            isSpawned = false;
            agent.isStopped = false;
            root.gameObject.SetActive(false);

            agent.enabled = SemiFunc.IsMasterClientOrSingleplayer();
            if (SemiFunc.IsMultiplayer())
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
                }
            }

            print("agent.enabled: " + agent.enabled + ", Transform View: " + GetComponent<PhotonTransformView>());
            setupDone = true;
        }

        internal bool isSpawned = false;
        [PunRPC]
        void SpawnRPC()
        {
            if (GameManager.Multiplayer())
            {
                int familyfriendly = ChaosMod.Instance.ConfigFamilyFriendly.Value ? 1 : 0;

                /*yapping = true;
                ChatManager.instance.PossessChatScheduleStart(0x7FFFFFFF);
                ChatManager.instance.PossessChat(ChatManager.PossessChatID.None, startMessages[UnityEngine.Random.Range(0, startMessages.GetLength(0)), familyfriendly], 4f, Color.red, eventExecutionAfterMessageIsDone: chat_callback);
                ChatManager.instance.PossessChatScheduleEnd();*/
            }

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
            if (SemiFunc.IsMasterClientOrSingleplayer())
                agent.isStopped = false;
            GetComponent<BoxCollider>().enabled = true;
            hurt.enabled = true;
            GoToNextPoint();
            PhotonNetwork.AddCallbackTarget(this);
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
        void DespawnRPC(bool slient = false)
        {
            if (!slient)
            {
                exp_src.Play();
                curAnimTime = 0f;
                playAnimation = true;
            } else PhotonNetwork.RemoveCallbackTarget(this);
            hurt.enabled = false;
            isSpawned = false;
        }

        public void Despawn(bool slient = false)
        {
            if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
            if (GameManager.Multiplayer())
                photonView.RPC("DespawnRPC", RpcTarget.All, slient);
            else
                DespawnRPC(slient);
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
            if (SemiFunc.IsMasterClientOrSingleplayer()) {
                // sometimes just go to player to ragebait
                if (UnityEngine.Random.Range(0, 100) <= 25) {
                    var players = SemiFunc.PlayerGetAll();
                    SetDestination(players[UnityEngine.Random.Range(0, players.Count)].transform.position);
                } else { 
                    SetDestination(waypoints[UnityEngine.Random.Range(0, waypoints.Count)]);
                }
            }
        }

        void Hide()
        {
            src.Stop();
            root.gameObject.SetActive(false);
            if (SemiFunc.IsMasterClientOrSingleplayer())
                agent.isStopped = true;
            GetComponent<BoxCollider>().enabled = false;
        }

        void OnDestroy()
        {
            if (!GameManager.Multiplayer() && !SemiFunc.IsMasterClient()) return;
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        [PunRPC]
        void SetDestinationRPC(float x, float y, float z)
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
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
            if (exp_rend == null)
                SetupComponents();

            exp_rend.transform.LookAt(Camera.main.transform, Vector3.up);
            if (spawnParticles.isPlaying)
            {
                spawnParticles.transform.position = startPos;
                spawnParticles.transform.eulerAngles = startEuler;
            }

            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                if (agent.isOnNavMesh && !agent.pathPending)
                {
                    if (agent.remainingDistance <= 1f && isSpawned)
                    {
                        GoToNextPoint();
                    }
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

            if (!isSpawned && mapCustom != null)
                mapCustom.Hide();

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

            if (SemiFunc.IsNotMasterClient())
            {
                if (hasPosData) {
                    transform.position = targetPos;
                    transform.eulerAngles = targetRot;
                }
                hasPosData = false;
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
            if (!SemiFunc.IsMasterClientOrSingleplayer() || !isSpawned) return;

            Vector3 origin = transform.position + (transform.forward * 7.5f) + (Vector3.up * 3f);
            Vector3 halfExtents = new Vector3(1.25f, 1.25f, 0.5f); // 차 앞 범위
            Vector3 direction = transform.forward;
            float maxDistance = 20f;
            bool isHit = Physics.BoxCast(origin, halfExtents, direction, out RaycastHit hit, transform.rotation, maxDistance);
            if (isHit)
            {
                Transform crashed = GetParentTransformByNameContains(hit.transform, "Player");
                if (crashed != null && crashed.name.Contains("Player"))
                {
                    SetDestination(hit.transform.position);
                    if (honkTimer <= 0f)
                        Honk();
                }
            }

            if (SemiFunc.IsMultiplayer()) {
                PhotonNetwork.RaiseEvent(1, agent.transform.position, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendUnreliable);
                PhotonNetwork.RaiseEvent(2, agent.transform.eulerAngles, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendUnreliable);
            }
        }

        private Vector3 targetPos;
        private Vector3 targetRot;
        private bool hasPosData = false;
        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == 1)
            {
                targetPos = (Vector3)photonEvent.CustomData;
                hasPosData = true;
            }

            if (photonEvent.Code == 2)
            {
                targetRot = (Vector3)photonEvent.CustomData;
                hasPosData = true;
            }
        }

    }
}

// maybe the car will have 1000 hp and killing it drops 15,000~23,000$ money bag
// it can have multiple cars in session, so killing it keep drops the price. (mininum 3,500~5,000$)