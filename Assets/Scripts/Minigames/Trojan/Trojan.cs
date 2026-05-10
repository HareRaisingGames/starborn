using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Starborn.InputSystem;
using Starborn.Trojan;
using TMPro;
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

        [HideInInspector]
        public int malwormKill = 0;
        [HideInInspector]
        public int turbotKill = 0;
        [HideInInspector]
        public int hairsplitterKill = 0;

        int clicks = 0;
        bool canClick = false;
        string tagName = "";

        [Header("Additional Assets")]
        public ProgressBar downloadBar;
        public TMP_Text uploadingTxt;
        public GameObject head;
        public MeshRenderer headRenderer;
        Material headMaterial;
        public Texture2D neutralExpression;
        public Texture2D deadExpression;
        public float rotateSpeed = 1f;
        public Vector3 rotateAxis;
        public Camera countdownCamera;

        float defaultCamSize;

        public static Virus SpawnVirus(string tag, out AudioSource audio)
        {
            audio = null;
            if (game.existingViruses.Count >= 6) return null;

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

                    foreach(Transform child in worm.gameObject.transform)
                    {
                        child.gameObject.layer = worm.gameObject.layer;
                    }

                    GameObject wormAudio = new GameObject("SFX");
                    audio = wormAudio.AddComponent<AudioSource>();
                    audio.playOnAwake = false;
                    audio.loop = false;
                    MixerSettings.SetAudioGroup(audio, "SongSFX");
                    wormAudio.transform.parent = worm.gameObject.transform;

                    game.existingViruses.Add(worm);
                    inBound = false;
                        foreach (Virus r in game.existingViruses)
                        {
                            if (worm == r) continue;
                            if (Vector3.Distance(r.transform.position, worm.transform.position) <= worm.radius && !worm.isAttacking)
                            {
                                inBound = true;
                                Debug.Log("Collision Dectected");
                                break;
                            }
                        }
                    //This might still be a little risky
                    while (inBound)
                    {
                        degree = new System.Random().Next(0, 361);
                        spawnPosition = PositionFromRadius(game.center, game.spawnRadius, degree);
                        startPosition = PositionFromRadius(game.center, game.revRadius, degree);
                        malSpawn = PositionFromRadius(game.center, game.spawnRadius + 3, degree);
                        while (!InCameraBound(malSpawn))
                        {
                            degree = random.Next(0, 361);
                            spawnPosition = PositionFromRadius(game.center, game.spawnRadius, degree);
                            startPosition = PositionFromRadius(game.center, game.revRadius, degree);
                            malSpawn = PositionFromRadius(game.center, game.spawnRadius + 3, degree);
                        }

                        worm.SetVirus(game.center, spawnPosition, startPosition, malSpawn, degree, false);

                        foreach (Virus r in game.existingViruses)
                        {
                            if (worm == r) continue;
                            if (Vector3.Distance(r.transform.position, worm.transform.position) <= worm.radius && !worm.isAttacking)
                            {
                                inBound = true;
                                Debug.Log("Still has collisions");
                                break;
                            }
                            else
                            {
                                inBound = false;
                            }
                                
                        }
                    }
                    //Debug.Log("All Clear!");
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

            foreach (Transform child in virus.gameObject.transform)
            {
                child.gameObject.layer = virus.gameObject.layer;
            }

            GameObject obj = new GameObject("SFX");
            audio = obj.AddComponent<AudioSource>();
            audio.transform.parent = virus.gameObject.transform;
            audio.playOnAwake = false;
            audio.loop = false;
            MixerSettings.SetAudioGroup(audio, "SongSFX");

            game.existingViruses.Add(virus);
            //Debug.Log(game.existingViruses.Count);
            inBound = false;
                if (virus.GetComponent<MalwormVirus>() == null)
                {
                    foreach (Virus r in game.existingViruses)
                    {
                        if (virus == r) continue;
                        if (Vector3.Distance(r.transform.position, virus.transform.position) <= virus.radius && !virus.isAttacking)
                        {
                            inBound = true;
                            Debug.Log("Collision Dectected");
                            break;
                        }
                    }

                    while (inBound)
                    {
                        degree = new System.Random().Next(0, 361);
                        spawnPosition = PositionFromRadius(game.center, game.spawnRadius, degree);
                        startPosition = PositionFromRadius(game.center, game.revRadius, degree);
                        virus.SetVirus(virus.virusName, game.center, spawnPosition, startPosition, degree);
                        foreach (Virus r in game.existingViruses)
                        {
                            if (virus == r) continue;
                            if (Vector3.Distance(r.transform.position, virus.transform.position) <= virus.radius)
                            {
                                inBound = true;
                                Debug.Log("Still has collisions");
                                break;
                            }
                            else
                            {
                                inBound = false;
                            }
                                
                        }
                }
            }
            //Debug.Log("All Clear!");
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

        public void SetHead(string status = "")
        {
            switch(status.ToLower())
            {
                case "dead":
                    headMaterial.SetTexture("_Base_Texture", deadExpression);
                    break;
                default:
                    headMaterial.SetTexture("_Base_Texture", neutralExpression);
                    break;
            }
        }

        public override void onA(InputAction.CallbackContext context)
        {
            base.onA(context);
            if (completed)
                return;

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
        public void OnDestroy()
        {
            SetHead();
        }

        public void OnApplicationQuit()
        {
            SetHead();
        }

        public override void Update()
        {
            base.Update();
            if(border != null)
            {
                border.transform.Rotate(Vector3.forward * speed * 10 * Time.deltaTime);
            }

            existingViruses = new List<Virus>(FindObjectsOfType<Virus>());
            /*foreach (Virus v in existingViruses)
            {
                bool inBound = false;
                if(v.GetComponent<MalwormVirus>())
                {

                }
                else
                {
                    foreach (Virus r in existingViruses)
                    {
                        if (v == r) continue;
                        if (Vector3.Distance(r.spawnPosition, v.spawnPosition) <= v.radius && !v.isAttacking)
                        {
                            inBound = true;
                            break;
                        }
                    }

                    while (inBound)
                    {
                        float degree = new System.Random().Next(0, 361);
                        Vector2 spawnPosition = PositionFromRadius(center, spawnRadius, degree);
                        Vector2 startPosition = PositionFromRadius(center, revRadius, degree);
                        v.SetVirus(v.virusName, center, spawnPosition, startPosition, degree);
                        foreach (Virus r in game.existingViruses)
                        {
                            if (v == r) continue;
                            if (Vector3.Distance(r.spawnPosition, v.spawnPosition) <= v.radius)
                            {
                                inBound = true;
                                break;
                            }
                            else
                                inBound = false;
                        }
                    }
                }


            }
            */
            if (head != null)
            {
                head.transform.Rotate(rotateAxis * rotateSpeed * Time.deltaTime);
            }

            if (uploadingTxt != null && 
                downloadBar != null && 
                downloadBar.activate &&
                Mathf.Clamp01(downloadBar.value.Invoke()) == 1) uploadingTxt.text = "Completed!";

            //Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, defaultCamSize, Time.deltaTime * 5);


        }

        public void GameOverExplosion()
        {
            if(explosionPack == null) return;
            StartCoroutine(Explode());
            IEnumerator Explode()
            {
                foreach(Transform explosion in explosionPack)
                {
                    if(explosion.GetComponent<Animator>())
                        explosion.GetComponent<Animator>().Play("Explode");

                    foreach(Camera camera in Object.FindObjectsOfType<Camera>())
                    {
                        LuaMethods.ShakeScreen(Conductor.instance.crochet/4f, 0.25f, camera.gameObject);
                    }
                    ExplosionSound();
                    yield return new WaitForSeconds(Conductor.instance.crochet/2f);
                }
            }

        }

        [Header("Game Over")]
        public Transform explosionPack;
        public AudioClip explosionSFX;

        protected void ExplosionSound()
        {
            if(explosionSFX == null) return;
            GameObject sound = new GameObject("Boom");
            SoundByte sfx = sound.AddComponent<SoundByte>();
            sfx.type = "SongSFX";
            sound.GetComponent<AudioSource>().playOnAwake = false;
            sound.GetComponent<AudioSource>().clip = explosionSFX;
            sfx.timeSamples = sound.GetComponent<AudioSource>().timeSamples;
            sound.GetComponent<AudioSource>().Play();
        }

        private ParticleSystem.Particle[] particles;

        public override void Awake()
        {
            base.Awake();
            game = this;
            headMaterial = headRenderer.material;
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

            Resources.Load<AudioClip>($"Audio/Trojan/burst");

            Resources.LoadAll<AudioClip>("Audio/Trojan/death");

            base.Start();
            OnSongStart = () => {
                if (MinigameManager.instance.tutorial)
                    return;

                AddTimestamp(Conductor.instance.music.clip.length);
                downloadBar.activate = true;
                if (uploadingTxt != null) uploadingTxt.text = "Uploading.";
            };

            OnBeatChange = Uploading;

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
                if (uploadingTxt != null) uploadingTxt.text = "ERROR";
                if (downloadBar != null) downloadBar.activate = false;
            };

            OnPreGameOver = GameOverExplosion;

            if(explosionPack != null) explosionPack.gameObject.SetActive(true);
            if (border != null)
                particles = new ParticleSystem.Particle[border.main.maxParticles];

            defaultCamSize = Camera.main.orthographicSize;

            /*if(FindObjectOfType<PauseMenu>(true) == null)
            {
                StartCoroutine(PlayMusic());
                IEnumerator PlayMusic()
                {
                    yield return new WaitForSeconds(1);
                    SetUpSong();
                }
            }*/
        }

        Tween<float> camTween;
        public void AddTimestamp(float point) => timestamps.Add(point);

        public void FooledYou(float time)
        {
            if(timestamps.Count > 1)
            {
                float startPoint = timestamps[0];
                float endPoint = timestamps[1];

                timestamps.RemoveAt(0);
                timestamps[0] = startPoint;

                TweenManager.NumTween(() => timestamps[0], (value) => { timestamps[0] = value; }, endPoint, time, Eases.EaseOutSine, delegate(){
                    if (uploadingTxt.text != null) uploadingTxt.text = "Uploading";
                }).SetOnUpdate(delegate() {
                    if (uploadingTxt.text != null) uploadingTxt.text = "JK! ;)";
                });
            }
        }

        void Uploading(int i)
        {
            if (MinigameManager.instance.gameOver)
                return;

            SetHead();

            if (MinigameManager.instance.tutorial)
                return;

            if (uploadingTxt != null)
            {
                switch(uploadingTxt.text)
                {
                    case "Uploading":
                        uploadingTxt.text = "Uploading.";
                        break;
                    case "Uploading.":
                        uploadingTxt.text = "Uploading..";
                        break;
                    case "Uploading..":
                        uploadingTxt.text = "Uploading...";
                        break;
                    case "Uploading...":
                        uploadingTxt.text = "Uploading";
                        break;
                    case "ERROR":
                        uploadingTxt.text = "Uploading";
                        break;
                }
            }

            if (turnOnShaking)
            {
                if (camTween != null) camTween.FullKill();
                Camera.main.orthographicSize = defaultCamSize - 0.25f;
                camTween = TweenManager.NumTween(() => Camera.main.orthographicSize, (value) => { Camera.main.orthographicSize = value; }, defaultCamSize, Conductor.instance.crochet * 0.5f, Eases.Linear);
            }

            
        }
        protected bool turnOnShaking;
        public bool shaking
        {
            get
            {
                return turnOnShaking;
            }
            set
            {
                turnOnShaking = value;
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

        public override void SetUpSong(string component = "")
        {
            MinigameManager.managerType = "Trojan";
            base.SetUpSong(component);
            foreach(Transform inst in MinigameManager.instance.transform)
            {
                if (inst == null)
                    continue;

                if(inst.GetComponent<Canvas>() != null)
                {
                    if(inst.GetComponent<Canvas>().renderMode == RenderMode.ScreenSpaceCamera)
                    {
                        if(countdownCamera != null) inst.GetComponent<Canvas>().worldCamera = countdownCamera;
                        else inst.GetComponent<Canvas>().worldCamera = Camera.main;
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
            else if (tag == "malworm")
            {
                amountJudger = () => malwormKill;
            }
            else if (tag == "turbot")
            {
                amountJudger = () => turbotKill;
            }
            else if (tag == "hairsplitter")
            {
                amountJudger = () => hairsplitterKill;
            }
            else
                amountJudger = () => 0;
        }

        public override void TutorialAdditionals()
        {
            base.TutorialAdditionals();
            canClick = true;

            if (tagName == "activate")
            {
                Countdown.alt = true;
                Countdown.prefabName = "TrojanCountdown";
                Countdown.folder = "computer";
                Countdown.mode = CountdownMode.Camera;
                Countdown.cam = countdownCamera;

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
            Countdown.alt = false;
            base.TutorialOnComplete(amount, tag);
            if(tag == "activate")
            {
                hasCompleted = delegate ()
                {
                    return clicks >= amount;
                };
            }
            else if(tag == "malworm")
            {
                Countdown.alt = true;
                Countdown.prefabName = "TrojanCountdown";
                Countdown.folder = "computer";
                Countdown.mode = CountdownMode.Camera;
                Countdown.cam = countdownCamera;
                if (displayMalworm != null) displayMalworm.Explode();
                hasCompleted = delegate ()
                {
                    return malwormKill >= amount;
                };
            }
            else if (tag == "turbot")
            {
                Countdown.alt = true;
                Countdown.prefabName = "TrojanCountdown";
                Countdown.folder = "computer";
                Countdown.mode = CountdownMode.Camera;
                Countdown.cam = countdownCamera;
                if (displayTurbot != null) displayTurbot.Explode();
                hasCompleted = delegate ()
                {
                    return turbotKill >= amount;
                };
            }
            else if (tag == "hairsplitter")
            {
                Countdown.alt = true;
                Countdown.prefabName = "TrojanCountdown";
                Countdown.folder = "computer";
                Countdown.mode = CountdownMode.Camera;
                Countdown.cam = countdownCamera;
                if(displayHairSplitter != null) displayHairSplitter.Explode();
                hasCompleted = delegate ()
                {
                    return hairsplitterKill >= amount;
                };
            }

            malwormKill = 0;
            turbotKill = 0;
            hairsplitterKill = 0;
        }

        Virus displayMalworm;
        TurbotVirus displayTurbot;
        Virus displayHairSplitter;
        public override void TutorialCallback(string tag = "")
        {
            base.TutorialCallback(tag);
            if (tag == "spawnMalworm")
            {
                displayMalworm = Instantiate(game.malwormPrefab, Vector3.left * 5, Quaternion.identity).GetComponent<MalwormVirus>();
                displayMalworm.GetComponent<SpriteRenderer>().flipX = true;
            }
            else if (tag == "spawnTurbot")
            {
                displayTurbot = Instantiate(game.turbotPrefab, Vector3.left * 5, Quaternion.identity).GetComponent<TurbotVirus>();
                displayTurbot.animator.Play(displayTurbot.eye.name);
            }
            else if (tag == "spawnHairsplit")
            {
                displayHairSplitter = Instantiate(game.hairsplitPrefab, Vector3.left * 5, Quaternion.identity).GetComponent<HairsplitVirus>();
            }

        }

        public override void OnTutorialStop()
        {
            base.OnTutorialStop();
            foreach (Virus virus in existingViruses)
                virus.Explode();

            existingViruses.Clear();
            tagName = "";
            Countdown.alt = false;
            Countdown.prefabName = "TrojanCountdown";
            Countdown.folder = "computer";
            Countdown.mode = CountdownMode.Camera;
            Countdown.cam = countdownCamera;
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
            Countdown.prefabName = "TrojanCountdown";
            Countdown.folder = "computer";
            Countdown.mode = CountdownMode.Camera;
            Countdown.cam = countdownCamera;
            Countdown.alt = false;
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

            //Debug.Log(Object.FindObjectsOfType<RhythmInput>().Length);


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
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    virus = Trojan.SpawnVirus("malworm", out audio).GetComponent<MalwormVirus>();
                    if(virus != null) virus.onHitAddtional = delegate(){
                        if(game.uploadingTxt != null && !MinigameManager.instance.tutorial) 
                            game.uploadingTxt.text = "ERROR";
                    game.SetHead("dead"); };
                    if(virus != null) SetAudio("malworm_1");
                    if(virus != null) virus.PlayAnimation("inch");
                }, 1f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(virus != null) virus.Move();
                    if(virus != null) SetAudio("malworm_2");
                    if(virus != null) virus.PlayAnimation("inch");
                }, 2f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(virus != null) virus.Move();
                    if(virus != null) virus.Rev(Conductor.instance.crochet * 0.5f);
                    if(virus != null) SetAudio("malworm_3");
                    if(virus != null) virus.PlayAnimation("rev");
                }, 3f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(virus != null) virus.Attack(Conductor.instance.crochet);
                    if(virus != null) virus.PlayAnimation("attack");
                }, 3.5f, RhythmInputs.A),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(game.autoPlay) game.ActivateForceField();
                }, 4f, RhythmInputs.A, 1f, 1f, ()=>{
                    if(virus != null) virus.Explode(Conductor.instance.crochet * 0.25f, delegate(){ game.malwormKill++; });
                }, (value) => { if(virus != null) virus.Charred();
                //MinigameManager.instance.LoseALife(0.5f);
                }),
            };
        }
    }

    public class Turbot : TrojanEvent
    {
        TurbotVirus virus;
        public Turbot()
        {
            actions = new List<CallForAction>() {
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    virus = Trojan.SpawnVirus("turbot", out audio).GetComponent<TurbotVirus>();
                    if(virus != null) virus.onHitAddtional = delegate(){if(game.uploadingTxt != null && !MinigameManager.instance.tutorial) 
                            game.uploadingTxt.text = "ERROR";
                            game.SetHead("dead");};
                    if(virus != null)
                    {
                        if(game.curStep % 4 == 2 || game.curStep % 4 == 3)
                            SetAudio("turbot_2");
                        else if(game.curStep % 4 == 0 || game.curStep % 4 == 1)
                            SetAudio("turbot_1");
                    }

                    if(virus != null) virus.Rev(Conductor.instance.crochet * 0.5f);
                }, 1f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(virus != null) virus.Attack(Conductor.instance.crochet);
                }, 1.5f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(game.autoPlay) game.ActivateForceField();
                }, 2f, RhythmInputs.A, 1f, 1f, ()=>{
                    if(virus != null) virus.Explode(Conductor.instance.crochet * 0.25f, delegate(){ game.turbotKill++; });
                }, (value) => { if(virus != null) virus.Charred();
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
        int count = 0;
        public Hairsplit()
        {
            actions = new List<CallForAction>() {
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    spliter = Trojan.SpawnVirus("hairsplit", out audio).GetComponent<HairsplitVirus>();
                    if(spliter != null) SetAudio("hairsplit_1");
                    if(spliter != null) spliter.Angry();
                }, 1f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(spliter != null) spliter.Split(Conductor.instance.crochet * 0.25f, out split1, out split2);
                    if(split1 != null) split1.onHitAddtional = delegate(){if(game.uploadingTxt != null && !MinigameManager.instance.tutorial) game.uploadingTxt.text = "ERROR"; game.SetHead("dead");};
                    if(split2 != null) split2.onHitAddtional = delegate(){if(game.uploadingTxt != null && !MinigameManager.instance.tutorial) game.uploadingTxt.text = "ERROR"; game.SetHead("dead");};
                    if(spliter != null) AftermathSound();
                }, 2f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(split1 != null)
                        split1.Rev(Conductor.instance.crochet * 0.5f);
                }, 2.25f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(split1 != null)
                        split1.Attack(Conductor.instance.crochet);
                }, 2.5f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(game.autoPlay && game.forcefield != null) game.forcefield.Play();
                    if(split2 != null)
                        split2.Rev(Conductor.instance.crochet * 0.5f);
                }, 3f, RhythmInputs.A, 1f, 1f, ()=>{
                    if(split1 != null) split1.Explode(Conductor.instance.crochet * 0.125f, delegate(){ count++; });
                }, (value) => { if(split1 != null) split1.Charred();
                //MinigameManager.instance.LoseALife(0.5f);
                }),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(split2 != null)
                        split2.Attack(Conductor.instance.crochet);
                }, 3.5f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(game.autoPlay) game.ActivateForceField();
                }, 4f, RhythmInputs.A, 1f, 1f, ()=>{
                    if(split2 != null) split2.Explode(Conductor.instance.crochet * 0.125f, delegate(){
                        count++;
                    if(count >= 2) game.hairsplitterKill++; });

                }, (value) => { if(split2 != null) split2.Charred();
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

    public class MalwormHeal : TrojanEvent
    {
        MalwormVirus virus;
        public MalwormHeal()
        {
            actions = new List<CallForAction>() {
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    virus = Trojan.SpawnVirus("malworm", out audio).GetComponent<MalwormVirus>();
                    if(virus != null) virus.onHitAddtional = delegate(){
                        if(game.uploadingTxt != null && !MinigameManager.instance.tutorial) 
                            game.uploadingTxt.text = "ERROR";
                    game.SetHead("dead"); };
                    if(virus != null) SetAudio("malworm_1");
                    if(virus != null) virus.PlayAnimation("inch");
                }, 1f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(virus != null) virus.Move();
                    if(virus != null) SetAudio("malworm_2");
                    if(virus != null) virus.PlayAnimation("inch");
                }, 2f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(virus != null) virus.Move();
                    if(virus != null) virus.Rev(Conductor.instance.crochet * 0.5f);
                    if(virus != null) SetAudio("malworm_3");
                    if(virus != null) virus.PlayAnimation("rev");
                }, 3f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(virus != null) virus.Attack(Conductor.instance.crochet);
                    if(virus != null) virus.PlayAnimation("attack");
                }, 3.5f, RhythmInputs.A),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(game.autoPlay) game.ActivateForceField();
                }, 4f, RhythmInputs.A, 1f, 1f, ()=>{
                    if(virus != null) virus.Explode(Conductor.instance.crochet * 0.25f, delegate(){ game.malwormKill++; }, true);
                }, (value) => { if(virus != null) virus.Charred();
                //MinigameManager.instance.LoseALife(0.5f);
                }),
            };
        }
    }

    public class TurbotHeal : TrojanEvent
    {
        TurbotVirus virus;
        public TurbotHeal()
        {
            actions = new List<CallForAction>() {
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    virus = Trojan.SpawnVirus("turbot", out audio).GetComponent<TurbotVirus>();
                    if(virus != null) virus.onHitAddtional = delegate(){if(game.uploadingTxt != null && !MinigameManager.instance.tutorial) 
                            game.uploadingTxt.text = "ERROR";
                            game.SetHead("dead");};
                    if(virus != null)
                    {
                        if(game.curStep % 4 == 2 || game.curStep % 4 == 3)
                            SetAudio("turbot_2");
                        else if(game.curStep % 4 == 0 || game.curStep % 4 == 1)
                            SetAudio("turbot_1");
                    }

                    if(virus != null) virus.Rev(Conductor.instance.crochet * 0.5f);
                }, 1f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(virus != null) virus.Attack(Conductor.instance.crochet);
                }, 1.5f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(game.autoPlay) game.ActivateForceField();
                }, 2f, RhythmInputs.A, 1f, 1f, ()=>{
                    if(virus != null) virus.Explode(Conductor.instance.crochet * 0.25f, delegate(){ game.turbotKill++; }, true);
                }, (value) => { if(virus != null) virus.Charred();
                //MinigameManager.instance.LoseALife(0.5f);
                }),
            };
        }
    }

    public class HairsplitHeal : TrojanEvent
    {
        HairsplitVirus spliter;
        Virus split1;
        Virus split2;
        int count = 0;
        public HairsplitHeal()
        {
            actions = new List<CallForAction>() {
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    spliter = Trojan.SpawnVirus("hairsplit", out audio).GetComponent<HairsplitVirus>();
                    if(spliter != null) SetAudio("hairsplit_1");
                    if(spliter != null) spliter.Angry();
                }, 1f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(spliter != null) spliter.Split(Conductor.instance.crochet * 0.25f, out split1, out split2);
                    if(split1 != null) split1.onHitAddtional = delegate(){if(game.uploadingTxt != null && !MinigameManager.instance.tutorial) game.uploadingTxt.text = "ERROR"; game.SetHead("dead");};
                    if(split2 != null) split2.onHitAddtional = delegate(){if(game.uploadingTxt != null && !MinigameManager.instance.tutorial) game.uploadingTxt.text = "ERROR"; game.SetHead("dead");};
                    if(spliter != null) AftermathSound();
                }, 2f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(split1 != null)
                        split1.Rev(Conductor.instance.crochet * 0.5f);
                }, 2.25f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(split1 != null)
                        split1.Attack(Conductor.instance.crochet);
                }, 2.5f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(game.autoPlay && game.forcefield != null) game.forcefield.Play();
                    if(split2 != null)
                        split2.Rev(Conductor.instance.crochet * 0.5f);
                }, 3f, RhythmInputs.A, 1f, 1f, ()=>{
                    if(split1 != null) split1.Explode(Conductor.instance.crochet * 0.125f, delegate(){ count++; }, true);
                }, (value) => { if(split1 != null) split1.Charred();
                //MinigameManager.instance.LoseALife(0.5f);
                }),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(split2 != null)
                        split2.Attack(Conductor.instance.crochet);
                }, 3.5f),
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    if(game.autoPlay) game.ActivateForceField();
                }, 4f, RhythmInputs.A, 1f, 1f, ()=>{
                    if(split2 != null) split2.Explode(Conductor.instance.crochet * 0.125f, delegate(){
                        count++;
                    if(count >= 2) game.hairsplitterKill++; }, true);

                }, (value) => { if(split2 != null) split2.Charred();
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
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    game.FooledYou(Conductor.instance.crochet * 5);
                }, 1f)
            };
        }
    }

    public class TurnOnCameraBounce : TrojanEvent
    {
        public TurnOnCameraBounce()
        {
            actions = new List<CallForAction>() {
                new CallForAction(()=>{
                    game.shaking = !game.shaking;
                }, 1f)
            };
        }
    }

    public class BigShake : RhythmEvent
    {
        public BigShake()
        {
            actions = new List<CallForAction>()
            {
                new CallForAction(()=>{
                    if(MinigameManager.instance != null && MinigameManager.instance.gameOver) return;
                    foreach(Camera camera in Object.FindObjectsOfType<Camera>())
                    {
                        LuaMethods.ShakeScreen(Conductor.instance.crochet, 1f, camera.gameObject);
                    }
                    //LuaMethods.ShakeCamera(Conductor.instance.crochet, 1f);
                },1f)
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