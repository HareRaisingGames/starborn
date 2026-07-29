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

        #region Tweens
        Tween<float> focalTween;
        Tween<float> aperatureTween;
        #endregion

        [Header("Numericals")]
        public int tipsyTotal = 0;
        int beerCount = 0;

        protected bool isTispy
        {
            set
            {
                if(value)
                {
                    if(focalTween != null) focalTween.FullKill();
                    focalTween = TweenManager.NumTween(() => depthOfField.focalLength.value, (value) => { depthOfField.focalLength.value = value; }, 300, 2.5f, Eases.EaseInOutSine);
                    if(aperatureTween != null) aperatureTween.FullKill();
                    aperatureTween = TweenManager.NumTween(() => depthOfField.aperture.value, (value) => { depthOfField.aperture.value = value; }, 32, 5f, Eases.EaseInOutSine).SetPingPong(1000);
                    parallax.PerlinMagnitudeTransition(2.5f, true);
                }
                else
                {
                    if(focalTween != null) focalTween.FullKill();
                    focalTween = TweenManager.NumTween(() => depthOfField.focalLength.value, (value) => { depthOfField.focalLength.value = value; }, defaultFocalLength, 2.5f, Eases.EaseInOutSine);
                    if(aperatureTween != null) aperatureTween.FullKill();
                    TweenManager.NumTween(() => depthOfField.aperture.value, (value) => { depthOfField.aperture.value = value; }, defaultAperature, 5f, Eases.EaseInOutSine).SetPingPong(1000);
                    parallax.PerlinMagnitudeTransition(2.5f, false);
                }
            }
        }

        protected List<ShotGlass> drinkOrders = new List<ShotGlass>();
        public void AddDrinkOrder(ShotGlass glass) =>
            drinkOrders.Add(glass);
        public DrinkType currentDrink => drinkOrders.Count > 0 ? drinkOrders[0].type : DrinkType.None;

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

        public void SuccessfulCatch(ShotGlass glass = null, bool isTutorial = false)
        {
            specialBody = true;
            bugz.handAnimator.gameObject.SetActive(false);
            bugz.PlayBody("catch", Conductor.instance.songBpm/120, () =>
            {
                specialBody = false;
                bugz.handAnimator.gameObject.SetActive(true);

                if(curStep % 4 == 0)
                {
                    bugz.PlayBody("idle", Conductor.instance.songBpm/120);
                    bugz.PlayHand("idle", Conductor.instance.songBpm/120);
                }

            });

            if(glass != null)
            {
                glass.Stop();
                glass.SetGlassAlpha(0);
                if(!isTutorial)
                {
                    if(glass.type == DrinkType.Beer)
                    {
                        beerCount++;
                        if(beerCount >= tipsyTotal)
                        {
                            isTispy = true;
                        }
                    }
                    else if(glass.type == DrinkType.Coffee)
                    {
                        isTispy = false;
                        beerCount = 0;
                    }
                }
            }
        }

        public void UnsuccessfulCatch(ShotGlass glass = null, bool isTutorial = false)
        {
            specialBody = true;
            bugz.PlayBody("half", Conductor.instance.songBpm/120, () =>
            {
                specialBody = false;

                if(curStep % 4 == 0)
                {
                    bugz.PlayBody("idle", Conductor.instance.songBpm/120);
                    bugz.PlayHand("idle", Conductor.instance.songBpm/120);
                }

            });


            if(glass != null)
            {
                glass.Spill();
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
        public override void SetUp()
        {
            base.SetUp();
            game = Object.FindObjectOfType<GlassPass>();
            preCallback = () => {
            /*
                Institate the drink orders for the minigame. Then push them onto the list
            */
                glass = GameObject.Instantiate(game.glassPrefab).GetComponent<ShotGlass>();
                glass.transform.SetParent(game.glassParent);
                glass.transform.localPosition = glass.startPoint;
                glass.SetShotGlass(isCoffee ? DrinkType.Coffee : DrinkType.Beer);
                game.AddDrinkOrder(glass);
                glass.gameObject.name = glass.type.ToString();
            };

        }

        public Slide()
        {
            parameters = new List<Parameter>()
            {
                new Parameter("isCoffee", false, "Coffee")
            };

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
                }, 2.5f),
                new CallForAction(()=>{
                    // tick.Play();
                }, 3, RhythmInputs.A, 0.5f, 0.5f, ()=>{
                    // game.testGlass.Stop();
                    game.SuccessfulCatch(glass);
                    game.catchGlass.Play();
                }, (value) =>
                {
                    game.UnsuccessfulCatch(glass);
                    game.spill.Play();
                })
            };
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
                    game.waterGlass.ResetPosition();
                    game.Pour(DrinkType.Water);
                }, 1),
                new CallForAction(() => { 
                    // tick.Play();
                    game.tick.Play();
                    game.Pass(DrinkType.Water);
                    // game.testGlass.Slide(Conductor.instance.crochet);
                }, 4),
                new CallForAction(() => { 
                    // tick.Play();
                    // game.waterGlass.Slide(Conductor.instance.crochet);
                }, 5),
                new CallForAction(() => { 
                    // game.waterGlass.DefaultSlide(Conductor.instance.crochet);
                    game.waterGlass.Slide(Conductor.instance.crochet);
                }, 5.5f),
                new CallForAction(()=>{
                    // tick.Play();
                }, 6, RhythmInputs.A, 0.5f, 0.5f, ()=>{
                    // game.testGlass.Stop();
                    game.SuccessfulCatch(game.waterGlass);
                    game.catchGlass.Play();

                }, (value) =>
                {
                    game.UnsuccessfulCatch(game.waterGlass);
                    game.spill.Play();
                })
            };
        }
    }
}

