using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

namespace Starborn.GlassPass
{
    public class GlassPass : Minigame
    {
        [Header("Elements")]
        // public ShotGlass testGlass; //The glass that is passed around
        public ShotGlass waterGlass; //The tutorial glass
        public BugzGP bugz;
        public Animator indicator;
        public Transform glassParent;
        public GameObject glassPrefab;

        [Header("PostProcess")]
        public Volume volume;
        DepthOfField depthOfField;
        public float defaultFocalLength = 0;
        public float defaultAperature = 0f;
        bool specialHand;

        protected ParallaxTest parallax;

        bool specialBody;
        protected AudioSource _tick;
        protected AudioSource _catchGlass;
        protected AudioSource _spill;
        public AudioSource tick => _tick;
        public AudioSource catchGlass => _catchGlass;

        public AudioSource spill => _spill;

        [Header("Tutorial")]
        public AudioSource one;
        public AudioSource two;
        public AudioSource go;

        int catchCount = 0;

        #region Tweens
        Tween<float> focalTween;
        Tween<float> aperatureTween;
        #endregion

        [Header("Numericals")]
        public int tipsyTotal = 0;
        int beerCount = 0;
        protected bool tipsy = false;
        protected bool isTipsy
        {
            set
            {
                tipsy = value;
                if(value)
                {
                    if(focalTween != null) focalTween.FullKill();
                    focalTween = TweenManager.NumTween(() => depthOfField.focalLength.value, (value) => { depthOfField.focalLength.value = value; }, 300, 2.5f, Eases.EaseInOutSine);
                    if(aperatureTween != null) aperatureTween.FullKill();
                    aperatureTween = TweenManager.NumTween(() => depthOfField.aperture.value, (value) => { depthOfField.aperture.value = value; }, 10, 5f, Eases.EaseInOutSine).SetPingPong(1000);
                    parallax.PerlinMagnitudeTransition(2.5f, true);
                }
                else
                {
                    if(focalTween != null) focalTween.FullKill();
                    focalTween = TweenManager.NumTween(() => depthOfField.focalLength.value, (value) => { depthOfField.focalLength.value = value; }, defaultFocalLength, 2.5f, Eases.EaseInOutSine);
                    if(aperatureTween != null) aperatureTween.FullKill();
                    aperatureTween = TweenManager.NumTween(() => depthOfField.aperture.value, (value) => { depthOfField.aperture.value = value; }, defaultAperature, 5f, Eases.EaseInOutSine);
                    parallax.PerlinMagnitudeTransition(2.5f, false);
                }
            }
        }

        protected List<ShotGlass> drinkOrders = new List<ShotGlass>();
        protected List<ShotGlass> passedOrders = new List<ShotGlass>();
        public void AddDrinkOrder(ShotGlass glass) =>
            drinkOrders.Add(glass);
        public DrinkType currentDrink => drinkOrders.Count > 0 ? drinkOrders[0].type : DrinkType.None;

        protected List<ShotGlass> emptyGlasses = new List<ShotGlass>();

        bool isCounting = false;

        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();
            // if(FindObjectOfType<PauseMenu>(true) == null)
            // {
            //     StartCoroutine(PlayMusic());
            //     IEnumerator PlayMusic()
            //     {
            //         yield return new WaitForSeconds(1);
            //         SetUpSong();
            //     }
            // }
            parallax = FindObjectOfType<ParallaxTest>();

            if(waterGlass != null) waterGlass.SetShotGlass(DrinkType.Water);

            // if(testGlass != null) {
            //     testGlass.SetShotGlass(DrinkType.Beer);
            //     ColorUtils.SetAlpha(testGlass.transform.Find("Shadow").gameObject, 0);
            //     TweenManager.XTween(testGlass.gameObject, testGlass.transform.position.x, 10f, 3f,Eases.EaseOutQuad);
            //     TweenManager.YTween(testGlass.gameObject, testGlass.transform.position.y, -3f, 2f,Eases.EaseOutBounce);
            //     TweenManager.RollTween(testGlass.gameObject, 0, -360, 2.25f);
            // }

            if (GameObject.Find("Pass") == null)
            {
                GameObject gameObject = new GameObject("Pass");
                _tick = gameObject.AddComponent<AudioSource>();
                _tick.clip = Resources.Load<AudioClip>("Audio/Tosstail/long_toss");
            }
            else
                _tick = GameObject.Find("Pass").GetComponent<AudioSource>();

            if (GameObject.Find("Catch") == null)
            {
                GameObject gameObject = new GameObject("Catch");
                _catchGlass = gameObject.AddComponent<AudioSource>();
                _catchGlass.clip = Resources.Load<AudioClip>("Audio/Tosstail/catch_shaker");
            }
            else
                _catchGlass = GameObject.Find("Catch").GetComponent<AudioSource>();

            if (GameObject.Find("Spill") == null)
            {
                GameObject gameObject = new GameObject("Spill");
                _spill = gameObject.AddComponent<AudioSource>();
                _spill.clip = Resources.Load<AudioClip>("Audio/Tosstail/miss");
            }
            else
                _spill = GameObject.Find("Spill").GetComponent<AudioSource>();

            MixerSettings.SetAudioGroup(_tick, "SongSFX");
            MixerSettings.SetAudioGroup(_catchGlass, "SongSFX");
            MixerSettings.SetAudioGroup(_spill, "SongSFX");

            OnSongStart = ()=>{ 
                bugz.PlayBody("idle", Conductor.instance.songBpm/120);
                bugz.PlayHand("idle", Conductor.instance.songBpm/120);
                };
            OnBeatChange = Bounce;

            if(volume.profile.TryGet<DepthOfField>( out depthOfField ) )
            {
                depthOfField.mode.value = DepthOfFieldMode.Bokeh;
                depthOfField.focalLength.value = defaultFocalLength;
                depthOfField.aperture.value = defaultAperature;
            }
            amountJudger = () => catchCount;

        }

        public override void StartSong()
        {
            base.StartSong();
            //Weird bug
            if (song != null) Conductor.instance.music.clip = song;
            if(indicator != null)
                indicator.speed = Conductor.instance.songBpm/120;
        }

        public override void AdditionalSongSetup(string tag = "")
        {
            base.AdditionalSongSetup(tag);
            if(!MinigameManager.instance.tutorial)
            {
                if(drinkOrders.Count > 0)
                    Pour(drinkOrders[0].type);
            }
        }

        public override void TutorialOnComplete(int amount, string tag = "")
        {
            hasCompleted = null;
            _turnOnGuide = false;
            catchCount = 0;
            base.TutorialOnComplete(amount, tag);
            hasCompleted = delegate ()
            {
                return catchCount >= amount;
            };

            if(tag == "guide")
            {
                _turnOnGuide = true;
            }
        }
        bool _turnOnGuide = false;
        public bool turnOnGuide => _turnOnGuide;

        public override void OnTutorialStop()
        {
            base.OnTutorialStop();
            AnimatorStateInfo indicatorInfo = indicator.GetCurrentAnimatorStateInfo(0);
            if(indicatorInfo.IsName("Water Pour"))
            {
                StartCoroutine(WaitForIndicator());
                IEnumerator WaitForIndicator()
                {
                    yield return null;
                    while(indicatorInfo.normalizedTime < 1f)
                    {
                        yield return null;
                        indicatorInfo = indicator.GetCurrentAnimatorStateInfo(0);
                    }

                    indicator.Play("Skip", 0, 0);
                }
            }
        }

        // Update is called once per frame
        public override void Update()
        {
            base.Update();
        }

        void Bounce(int i)
        {
            if(!Conductor.isPlayingSound)
                return;
            
            if(!specialBody)
                // if(i % 1 == 0 || i == 0)
                    bugz.PlayBody("idle", Conductor.instance.songBpm/120);

            if(!specialHand)
                if(i % 2 == 0 || i == 0)
                    bugz.PlayHand("idle", Conductor.instance.songBpm/120);
        }

        public override void onA(InputAction.CallbackContext context)
        {
            base.onA(context);

            if(autoPlay || !_startSong)
                return;

            specialHand = true;
            bugz.PlayHand("catch", Conductor.instance.songBpm/120, () =>
            {
                specialHand = false;
            });
        }

        public void SuccessfulCatch(ShotGlass glass = null, bool isTutorial = false, float time = 1)
        {
            specialBody = true;
            bugz.handAnimator.gameObject.SetActive(false);
            catchCount++;
            // bool hasTossed = false;
            bugz.PlayBody("catch", Conductor.instance.songBpm/120, () =>
            {
                specialBody = false;
                bugz.handAnimator.gameObject.SetActive(true);

                if(curStep % 4 == 0 && !autoPlay)
                {
                    bugz.PlayBody("idle", Conductor.instance.songBpm/120);
                    bugz.PlayHand("idle", Conductor.instance.songBpm/120);
                }

            },(value) =>
            {
                if(value >= 0.9f)
                {
                    if(glass != null)
                    {
                        #if NET_4_6
                        glass.Toss(time, new Vector2(MiscUtils.Random(3f,3.5f,4f),3f), new Vector2(11f,-3f));
                        #else
                        glass.Toss(time, new Vector2(3.5f,3f), new Vector2(11f,-3f));
                        #endif
                    }
                }
            }, () =>
            {
                if(passedOrders.Contains(glass))
                {
                    if(passedOrders.IndexOf(glass) - 1 >= 0)
                    {
                        ShotGlass previousGlass = passedOrders[passedOrders.IndexOf(glass) - 1];
                        if(previousGlass != null)
                        {
                            if(previousGlass.successful && !previousGlass.tossed)
                            {
                                #if NET_4_6
                                previousGlass.Toss(time, new Vector2(MiscUtils.Random(3f,3.5f,4f),3f), new Vector2(11f,-3f));
                                #else
                                previousGlass.Toss(time, new Vector2(3.5f,3f), new Vector2(11f,-3f));
                                #endif
                            }
                        }
                    }
                }
            });

            if(glass != null)
            {
                glass.Stop();
                glass.SetGlassAlpha(0);
                glass.successful = true;
                if(!isTutorial)
                {
                    if(glass.type == DrinkType.Beer)
                    {
                        beerCount++;
                        if(beerCount >= tipsyTotal && !tipsy)
                        {
                            isTipsy = true;
                        }
                    }
                    else if(glass.type == DrinkType.Coffee)
                    {
                        isTipsy = false;
                        beerCount = 0;
                    }
                }
            }
        }

        public void UnsuccessfulCatch(ShotGlass glass = null, bool isTutorial = false, float time = 1)
        {
            specialBody = true;
            MinigameManager.instance.LoseALife(0.5f);
            bugz.PlayBody("half", Conductor.instance.songBpm/120, () =>
            {
                specialBody = false;

                if(curStep % 4 == 0)
                {
                    bugz.PlayBody("idle", Conductor.instance.songBpm/120);
                    bugz.PlayHand("idle", Conductor.instance.songBpm/120);
                }

            },(value) =>{},() =>
            {
                if(passedOrders.Contains(glass))
                {
                    if(passedOrders.IndexOf(glass) - 1 >= 0)
                    {
                        ShotGlass previousGlass = passedOrders[passedOrders.IndexOf(glass) - 1];
                        if(previousGlass != null)
                        {
                            if(previousGlass.successful && !previousGlass.tossed)
                            {
                                #if NET_4_6
                                previousGlass.Toss(time, new Vector2(MiscUtils.Random(3f,3.5f,4f),3f), new Vector2(11f,-3f));
                                #else
                                previousGlass.Toss(time, new Vector2(3.5f,3f), new Vector2(11f,-3f));
                                #endif
                            }
                        }
                    }
                }
            });


            if(glass != null)
            {
                glass.Spill();
                if(!isTutorial)
                {
                    StartCoroutine(Abrakaglassa());
                    IEnumerator Abrakaglassa()
                    {
                        yield return new WaitForSeconds(Conductor.instance.crochet * 1.5f);
                        glass.SetGlassAlpha(0);
                    }
                }
            }
        }

        public void Pour(DrinkType type)
        {
            string drinkName = type.ToString();
            IndicatorAnimator(drinkName, "Pour");
        }

        public void Pass(DrinkType type)
        {
            string drinkName = type.ToString();
            IndicatorAnimator(drinkName, "Pass");
        }

        void IndicatorAnimator(string drink, string action) => indicator.Play($"{drink} {action}");

        public void NextPass(DrinkType type)
        {
            if(type == DrinkType.None)
                return;

            string drinkName = type.ToString();
            StartCoroutine(AnimationUtils.OnAnimationFinish(indicator, $"{drinkName} Pass", () => {
                ShotGlass glass = drinkOrders[0];
                passedOrders.Add(glass);
                drinkOrders.RemoveAt(0);
                if(drinkOrders.Count > 0)
                {
                    Pour(drinkOrders[0].type);
                }
            }, -1));
        }

    }

    public class Slide : RhythmEvent
    {
        public GlassPass game;
        bool isCoffee = false;
        public ShotGlass glass;

        public override List<Parameter> parameters { get; set; } = new List<Parameter>()
        {
            new Parameter("isCoffee", false, "Coffee")
        };

        public override void SetUp()
        {
            base.SetUp();
            game = Object.FindObjectOfType<GlassPass>();
            preCallback = () => {
                glass = GameObject.Instantiate(game.glassPrefab).GetComponent<ShotGlass>();
                glass.transform.SetParent(game.glassParent);
                glass.transform.localPosition = glass.startPoint;
                glass.transform.localRotation = Quaternion.identity;
                glass.SetShotGlass(isCoffee ? DrinkType.Coffee : DrinkType.Beer);
                glass.SetSpeed(Conductor.instance.songBpm * 1.5f);
                game.AddDrinkOrder(glass);
                glass.gameObject.name = glass.type.ToString();
            };

        }

        public Slide(bool isCoffee = false)
        {
            this.isCoffee = isCoffee;
            actions = new List<CallForAction>()
            {
                new CallForAction(() => { 
                    // game.waterGlass.ResetPosition();
                    game.tick.Play();
                    game.NextPass(game.currentDrink);
                }, 1),
                new CallForAction(() => { 
                    // tick.Play();
                    // game.testGlass.Slide(Conductor.instance.crochet);
                }, 2),
                new CallForAction(() => { 
                    // game.waterGlass.DefaultSlide(Conductor.instance.crochet);
                    glass.Slide(Conductor.instance.crochet);
                }, 2.4f),
                new CallForAction(()=>{
                    // tick.Play();
                }, 3, RhythmInputs.A, 0.5f, 0.5f, ()=>{
                    // game.testGlass.Stop();
                    game.SuccessfulCatch(glass, false, Conductor.instance.crochet);
                    game.catchGlass.Play();
                }, (value) =>
                {
                    game.UnsuccessfulCatch(glass, false, Conductor.instance.crochet);
                    game.spill.Play();
                },
                () => {
                    MinigameManager.instance.LoseALife(1f);
                })
            };
        }
    }

    public class Coffee : Slide
    {
        public Coffee() : base(true)
        {
            
        }
    }
    public class TutorialSlide : RhythmEvent
    {
        public GlassPass game;
        public override void SetUp()
        {
            base.SetUp();
            game = Object.FindObjectOfType<GlassPass>();

        }

        public TutorialSlide()
        {
            actions = new List<CallForAction>()
            {
                new CallForAction(() => { 
                    game.Pour(DrinkType.Water);
                }, 2),
                new CallForAction(() => { 
                    game.waterGlass.ResetPosition();
                    game.waterGlass.tossed = false;
                }, 3),
                new CallForAction(() => { 
                    // tick.Play();
                    game.tick.Play();
                    game.Pass(DrinkType.Water);
                    if(game.turnOnGuide)
                        game.one.Play();
                    // game.testGlass.Slide(Conductor.instance.crochet);
                }, 4),
                new CallForAction(() => { 
                    if(game.turnOnGuide)
                        game.two.Play();
                    // tick.Play();
                    // game.waterGlass.Slide(Conductor.instance.crochet);
                }, 5),
                new CallForAction(() => { 
                    // game.waterGlass.DefaultSlide(Conductor.instance.crochet);
                    game.waterGlass.Slide(Conductor.instance.crochet);
                }, 5.4f),
                new CallForAction(()=>{
                    // tick.Play();
                }, 6, RhythmInputs.A, 0.5f, 0.5f, ()=>{
                    // game.testGlass.Stop();
                    game.SuccessfulCatch(game.waterGlass, true, Conductor.instance.crochet);
                    game.catchGlass.Play();
                    if(game.turnOnGuide)
                        game.go.Play();

                }, (value) =>
                {
                    game.UnsuccessfulCatch(game.waterGlass, true, Conductor.instance.crochet);
                    game.spill.Play();
                })
            };
        }
    }
}

