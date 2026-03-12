using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Starborn.InputSystem;
using Starborn.Trojan;
using System.IO;

namespace Starborn.Trojan
{
    public class Trojan : Minigame
    {
        [Header("Trojan")]
        public Vector3 center = Vector3.zero;
        public float spawnRadius = 4f;
        public float revRadius = 5f;

        [Header("Sprites")]
        public GameObject malwormPrefab;
        public GameObject turbotPrefab;
        public GameObject hairsplitPrefab;

        [Header("Particles")]
        public ParticleSystem forcefield;
        public AudioSource forcefieldSFX;
        public ParticleSystem border;
        public float speed = 1f;

        public static Trojan game;
        protected List<float> timestamps = new List<float>();

        public Transform virusParent;

        List<Virus> existingViruses = new List<Virus>();

        int clicks = 0;
        bool canClick = false;
        string tagName = "";

        [Header("Additional Assets")]
        public ProgressBar downloadBar;
        public GameObject head;
        public float rotateSpeed = 1f;
        public Vector3 rotateAxis;
        public Camera countdownCamera;

        public static Virus SpawnVirus(string tag, out AudioSource audio)
        {
            System.Random random = new System.Random();
            float degree = random.Next(0, 361);

            Vector3 spawnPosition = PositionFromRadius(game.center, game.spawnRadius, degree);
            Vector3 startPosition = PositionFromRadius(game.center, game.revRadius, degree);
            Virus virus = null;

            bool inBound = false;
            foreach(Virus v in game.existingViruses)
            {
                if (Vector3.Distance(spawnPosition, v.spawnPosition) <= v.radius)
                {
                    inBound = true;
                    break;
                }
            }

            while(inBound)
            {
                degree = random.Next(0, 361);
                spawnPosition = PositionFromRadius(game.center, game.spawnRadius, degree);
                startPosition = PositionFromRadius(game.center, game.revRadius, degree);
                foreach (Virus v in game.existingViruses)
                {
                    if (Vector3.Distance(spawnPosition, v.spawnPosition) <= v.radius)
                    {
                        inBound = true;
                        break;
                    }
                    else
                        inBound = false;
                }
            }

            switch(tag.ToLower())
            {
                case "malworm":
                    Vector3 malSpawn = PositionFromRadius(game.center, game.spawnRadius + 3, degree);
                    while(!InCameraBound(malSpawn))
                    {
                        degree = random.Next(0, 361);
                        spawnPosition = PositionFromRadius(game.center, game.spawnRadius, degree);
                        startPosition = PositionFromRadius(game.center, game.revRadius, degree);
                        malSpawn = PositionFromRadius(game.center, game.spawnRadius + 3, degree);
                    }
                    MalwormVirus worm = Instantiate(game.malwormPrefab, malSpawn, Quaternion.identity).GetComponent<MalwormVirus>();
                    worm.SetVirus(game.center, spawnPosition, startPosition, malSpawn, degree);
                    worm.transform.parent = game.virusParent;
                    if (game.virusParent != null)
                        worm.gameObject.layer = game.virusParent.gameObject.layer;
                    GameObject wormAudio = new GameObject("SFX");
                    audio = wormAudio.AddComponent<AudioSource>();
                    audio.playOnAwake = false;
                    audio.loop = false;
                    MixerSettings.SetAudioGroup(audio, "SongSFX");
                    wormAudio.transform.parent = worm.gameObject.transform;
                    return worm;
                case "turbot":
                    virus = Instantiate(game.turbotPrefab, spawnPosition, Quaternion.identity).GetComponent<Virus>();
                    virus.SetVirus("turbot", game.center, spawnPosition, startPosition, degree);
                    break;
                case "hairsplit":
                    virus = Instantiate(game.hairsplitPrefab, spawnPosition, Quaternion.identity).GetComponent<Virus>();
                    virus.SetVirus("hairsplit", game.center, spawnPosition, startPosition, degree);
                    break;
            }

            virus.transform.parent = game.virusParent;
            if (game.virusParent != null)
                virus.gameObject.layer = game.virusParent.gameObject.layer;
            GameObject obj = new GameObject("SFX");
            audio = obj.AddComponent<AudioSource>();
            audio.transform.parent = virus.gameObject.transform;
            audio.playOnAwake = false;
            audio.loop = false;
            MixerSettings.SetAudioGroup(audio, "SongSFX");
            return virus;
        }

        public static Vector3 PositionFromRadius(Vector3 center, float radius, float angle)
        {
            float radian = angle * Mathf.Deg2Rad;

            float x = center.x + radius * Mathf.Cos(radian);
            float y = center.y + radius * Mathf.Sin(radian);

            return new Vector3(x, y, center.z);
        }

        public static bool InCameraBound(Vector2 point)
        {
            Vector3 cameraPosition = Camera.main.transform.position;

            Vector2 halfCameraSize;
            //  Calcuale the half size of the Camera
            halfCameraSize.y = Camera.main.orthographicSize;
            halfCameraSize.x = halfCameraSize.y * Camera.main.aspect;

            return
                point.x <= cameraPosition.x + halfCameraSize.x && point.x >= cameraPosition.x - halfCameraSize.x &&
                point.y <= cameraPosition.y + halfCameraSize.y && point.y >= cameraPosition.y - halfCameraSize.y;
        }

        public override void onA(InputAction.CallbackContext context)
        {
            base.onA(context);
            if (paused) return;
            if (autoPlay && tagName != "activate") return;
            if (!canClick) return;

            ActivateForceField();
        }

        //int i = 0;
        public override void onPad(InputAction.CallbackContext context)
        {
            base.onPad(context);
            //while (System.IO.File.Exists($"{Application.dataPath}/trojan_{i}.png"))
                //i++;
            //ScreenCapture.CaptureScreenshot($"{Application.dataPath}/trojan_{i}.png");
            //i++;
        }

        public void ActivateForceField()
        {
            if (forcefield != null)
                forcefield.Play();
            if (forcefieldSFX != null)
                forcefieldSFX.Play();
            clicks++;
        }
        public override void Update()
        {
            base.Update();
            if(border != null)
            {
                border.transform.Rotate(Vector3.forward * speed * 10 * Time.deltaTime);
            }

            existingViruses = new List<Virus>(FindObjectsOfType<Virus>());

            if(head != null)
            {
                head.transform.Rotate(rotateAxis * rotateSpeed * Time.deltaTime);
            }
        }

        private ParticleSystem.Particle[] particles;

        public override void Awake()
        {
            base.Awake();
            game = this;
        }

        public override void Start()
        {
            Resources.Load<AudioClip>($"Prefabs/Managers/Trojan");
            Resources.Load<AudioClip>($"Audio/Trojan/malworm_1");
            Resources.Load<AudioClip>($"Audio/Trojan/malworm_2");
            Resources.Load<AudioClip>($"Audio/Trojan/malworm_3");
            Resources.Load<AudioClip>($"Audio/Trojan/turbot_1");
            Resources.Load<AudioClip>($"Audio/Trojan/turbot_2");
            if (Resources.Load<AudioClip>($"Audio/Trojan/hairsplit_1") != null)
                Resources.Load<AudioClip>($"Audio/Trojan/hairsplit_1");
            if (Resources.Load<AudioClip>($"Audio/Trojan/hairsplit_2") != null)
                Resources.Load<AudioClip>($"Audio/Trojan/hairsplit_2");

            Resources.LoadAll<AudioClip>("Audio/Trojan/death");

            base.Start();
            OnSongStart = () => {
                AddTimestamp(Conductor.instance.music.clip.length);
                downloadBar.activate = true;
            };

            if (downloadBar != null)
            {
                downloadBar.value = () => {
                    if (Conductor.instance.isFinished)
                        return 1;
                    else
                        return Conductor.instance.music.time / timestamps[0];
                };

                downloadBar.gameObject.SetActive(false);
            }

            OnGameOver = delegate ()
            {
                if (downloadBar != null) downloadBar.activate = false;
            };

            if (border != null)
                particles = new ParticleSystem.Particle[border.main.maxParticles];

            if(FindObjectOfType<PauseMenu>(true) == null)
            {
                StartCoroutine(PlayMusic());
                IEnumerator PlayMusic()
                {
                    yield return new WaitForSeconds(1);
                    SetUpSong();
                }
            }
        }

        public void AddTimestamp(float point) => timestamps.Add(point);

        public void FooledYou(float time)
        {
            if(timestamps.Count > 1)
            {
                float startPoint = timestamps[0];
                float endPoint = timestamps[1];

                timestamps.RemoveAt(0);
                timestamps[0] = startPoint;

                TweenManager.NumTween(() => timestamps[0], (value) => { timestamps[0] = value; }, endPoint, time, Eases.EaseOutSine);
            }
        }

        void LateUpdate()
        {
            if (center == null || border == null) return;

            int numParticles = border.GetParticles(particles);
            for (int i = 0; i < numParticles; i++)
            {
                Vector3 directionToTarget = new Vector3(center.x, center.y, center.z) - particles[i].position;
                // Use Quaternion.LookRotation to create a rotation that faces the target direction
                float angle = Mathf.Atan2(directionToTarget.x, directionToTarget.y) * Mathf.Rad2Deg;
                // Unity's 2D rotation generally aligns the X-axis (forward in 2D context) with the direction
                // We set the rotation in 3D space.
                // Note: You might need to adjust the angle offset (+/- 90 degrees) depending on your sprite's default orientation
                particles[i].rotation3D = new Vector3(0, 0, angle) - border.transform.eulerAngles;
            }

            border.SetParticles(particles, numParticles);
        }

        public override void SetUpSong()
        {
            MinigameManager.managerType = "Trojan";
            base.SetUpSong();
            foreach(Transform inst in MinigameManager.instance.transform)
            {
                if (inst == null)
                    continue;

                if(inst.GetComponent<Canvas>() != null)
                {
                    if(inst.GetComponent<Canvas>().renderMode == RenderMode.ScreenSpaceCamera)
                    {
                        inst.GetComponent<Canvas>().worldCamera = Camera.main;
                    }
                }
            }
        }

        public override void AdditionalSongSetup(string tag = "")
        {
            base.AdditionalSongSetup(tag);
            if (tag == "activate")
            {
                tagName = "activate";
                amountJudger = () => clicks;
                canClick = true;
            }
        }

        public override void TutorialAdditionals()
        {
            base.TutorialAdditionals();

            Countdown.folder = "bitcrush";
            Countdown.mode = CountdownMode.Camera;
            Countdown.cam = countdownCamera;
            canClick = true;

            if (tagName == "activate")
            {
                if (MinigameManager.instance.remainingText != null && amountJudger != null)
                {
                    MinigameManager.instance.remainingText.text = MinigameManager.instance.requiredText;
                }

                if (hasCompleted != null && hasCompleted.Invoke())
                    OnBeatTutorial(0);

                //Debug.Log(hasCompleted.Invoke());
            }
            //Debug.Log(amountJudger.Invoke());
        }

        public override void TutorialOnComplete(int amount, string tag = "")
        {
            hasCompleted = null;
            base.TutorialOnComplete(amount, tag);
            if(tag == "activate")
            {
                hasCompleted = delegate ()
                {
                    return clicks >= amount;
                };
            }
        }

        public override void TutorialReset()
        {
            base.TutorialReset();
            tagName = "";
            clicks = 0;
            canClick = false;
            hasCompleted = null;
        }

        public override void StartSong()
        {
            Countdown.folder = "bitcrush";
            Countdown.mode = CountdownMode.Camera;
            Countdown.cam = countdownCamera;
            canClick = true;
            hasCompleted = delegate ()
            {
                return Conductor.instance.isFinished;
            };

            if(downloadBar != null)
            {
                downloadBar.gameObject.SetActive(true);
                GameObject[] hiddenStuff = { downloadBar.text.gameObject, 
                    downloadBar._slider.transform.Find("Background").gameObject,
                downloadBar._slider.transform.Find("Fill Area").transform.Find("Fill").gameObject};

                Vector3 barPos = downloadBar.GetComponent<RectTransform>().anchoredPosition3D;

                foreach(GameObject stuff in hiddenStuff)
                {
                    ColorUtils.SetAlpha(stuff, 0);
                    TweenManager.AlphaTween(stuff, 0, 1, 3f, Eases.EaseOutCubic);
                }

                TweenManager.YTween(downloadBar.gameObject, barPos.y + 2, barPos.y, 1.5f, Eases.EaseInOutQuad);
            }


            base.StartSong();
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, border.shape.radius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, spawnRadius);

            Gizmos.DrawWireSphere(center, revRadius);
            Gizmos.DrawWireSphere(center, spawnRadius + 3);
        }
    }

    public class Malworm : TrojanEvent
    {
        MalwormVirus virus;
        public Malworm()
        {
            actions = new List<CallForAction>() {
                new CallForAction(()=>{
                    virus = Trojan.SpawnVirus("malworm", out audio).GetComponent<MalwormVirus>();
                    SetAudio("malworm_1");
                }, 1f),
                new CallForAction(()=>{
                    virus.Move();
                    SetAudio("malworm_2");
                }, 2f),
                new CallForAction(()=>{
                    virus.Move();
                    virus.Rev(Conductor.instance.crochet * 0.5f);
                    SetAudio("malworm_3");
                }, 3f),
                new CallForAction(()=>{
                    virus.Attack(Conductor.instance.crochet);
                }, 3.5f, RhythmInputs.A),
                new CallForAction(()=>{
                    if(game.autoPlay) game.ActivateForceField();
                }, 4f, RhythmInputs.A, 1f, 1f, ()=>{
                    virus.Explode(Conductor.instance.crochet * 0.25f);
                }, (value) => { virus.isFried = true;
                //MinigameManager.instance.LoseALife(0.5f);
                }),
            };
        }
    }

    public class Turbot : TrojanEvent
    {
        Virus virus;
        public Turbot()
        {
            actions = new List<CallForAction>() {
                new CallForAction(()=>{
                    virus = Trojan.SpawnVirus("turbot", out audio);
                    if(game.curStep % 4 == 2 || game.curStep % 4 == 3)
                        SetAudio("turbot_2");
                    else if(game.curStep % 4 == 0 || game.curStep % 4 == 1)
                        SetAudio("turbot_1");
                    virus.Rev(Conductor.instance.crochet * 0.5f);
                }, 1f),
                new CallForAction(()=>{
                    virus.Attack(Conductor.instance.crochet);
                }, 1.5f),
                new CallForAction(()=>{
                    if(game.autoPlay) game.ActivateForceField();
                }, 2f, RhythmInputs.A, 1f, 1f, ()=>{
                    virus.Explode(Conductor.instance.crochet * 0.25f);
                }, (value) => { virus.isFried = true;
                //MinigameManager.instance.LoseALife(0.5f);
                }),
            };
        }
    }

    public class Hairsplit : TrojanEvent
    {
        HairsplitVirus spliter;
        Virus split1;
        Virus split2;
        public Hairsplit()
        {
            actions = new List<CallForAction>() {
                new CallForAction(()=>{
                    spliter = Trojan.SpawnVirus("hairsplit", out audio).GetComponent<HairsplitVirus>();
                    SetAudio("hairsplit_1");
                }, 1f),
                new CallForAction(()=>{
                    spliter.Split(Conductor.instance.crochet * 0.25f, out split1, out split2);
                    AftermathSound();
                }, 2f),
                new CallForAction(()=>{
                    split1.Rev(Conductor.instance.crochet * 0.5f);
                }, 2.25f),
                new CallForAction(()=>{
                    split1.Attack(Conductor.instance.crochet);
                }, 2.5f),
                new CallForAction(()=>{
                    if(game.autoPlay && game.forcefield != null) game.forcefield.Play();
                    split2.Rev(Conductor.instance.crochet * 0.5f);
                }, 3f, RhythmInputs.A, 1f, 1f, ()=>{
                    split1.Explode(Conductor.instance.crochet * 0.125f);
                }, (value) => { split1.isFried = true;
                //MinigameManager.instance.LoseALife(0.5f);
                }),
                new CallForAction(()=>{
                    split2.Attack(Conductor.instance.crochet);
                }, 3.5f),
                new CallForAction(()=>{
                    if(game.autoPlay) game.ActivateForceField();
                }, 4f, RhythmInputs.A, 1f, 1f, ()=>{
                    split2.Explode(Conductor.instance.crochet * 0.125f);
                }, (value) => { split2.isFried = true;
                //MinigameManager.instance.LoseALife(0.5f);
                }),
            };
        }

        public void AftermathSound()
        {
            if (Resources.Load<AudioClip>($"Audio/Trojan/hairsplit_2") != null)
            {
                GameObject sound = new GameObject("Boom");
                SoundByte sfx = sound.AddComponent<SoundByte>();
                sfx.type = "SongSFX";
                sound.GetComponent<AudioSource>().playOnAwake = false;
                sound.GetComponent<AudioSource>().clip = Resources.Load<AudioClip>($"Audio/Trojan/hairsplit_2");
                sfx.timeSamples = sound.GetComponent<AudioSource>().timeSamples;
                sound.GetComponent<AudioSource>().Play();
            }
        }
    }

    public class ProgressMarker : TrojanEvent
    {
        public override void SetUp()
        {
            base.SetUp();
            timeCallback = game.AddTimestamp;
        }
    }

    public class TimerFakeOut : TrojanEvent
    {
        public TimerFakeOut()
        {
            actions = new List<CallForAction>() {
                new CallForAction(()=>{
                    game.FooledYou(Conductor.instance.crochet * 5);
                }, 1f)
            };
        }
    }
}

public class TrojanEvent : RhythmEvent
{
    protected Trojan game;
    protected AudioSource audio;
    protected AudioClip miss;
    public override void SetUp()
    {
        base.SetUp();
        game = Object.FindObjectOfType<Trojan>();

        miss = Resources.Load<AudioClip>($"Audio/blip");
    }

    public void SetMissAudio()
    {
        Debug.Log("Hey!");
        audio.clip = miss;
        audio.Play();
    }
    public void SetAudio(string name)
    {
        if(Resources.Load<AudioClip>($"Audio/Trojan/{name}") != null)
        {
            audio.clip = Resources.Load<AudioClip>($"Audio/Trojan/{name}");
            audio.Play();
        }
    }
}