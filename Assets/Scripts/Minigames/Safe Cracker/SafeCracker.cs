using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;
using UnityEngine.InputSystem;
using TMPro;
using System.Text.RegularExpressions;

namespace Starborn.SafeCracker
{
    public class SafeCracker : Minigame
    {
        protected int[] curCode = new int[4];
        protected int[] code = new int[4];

        bool startSequence = false;

        int curKey = -1;

        public static SafeCracker safeCracker;

        public List<TMP_Text> textList = new List<TMP_Text>();
        public AudioSource blip;
        public AudioSource button;
        public TMP_Text instructions;
        protected string instructionsText = "Enter {code}\nPress Space to select a number";

        [Header("Modes")]
        public bool weirdNumbers;
        public bool blackout;

        // StartCode codeEvent;
        // StartCode practiceCode = new StartCode(8842);

        //string saveInstructions;

        public override void Awake()
        {
            safeCracker = this;
            base.Awake();
            // OnBeatChange = ChangeNumber;
        }

        // Start is called before the first frame update
        public override void Start()
        {
            int dynamicSeed = (int)System.DateTime.Now.Ticks;
            Random.InitState(dynamicSeed);
            // codeEvent = new StartCode(8842);
            if (instructions != null) instructions.gameObject.SetActive(false);
            //saveInstructions = instructions.text;
            base.Start();
            Conductor.instance.SetUpBPM();
            // codeEvent.AddToChart(0);

            hasCompleted = delegate ()
            {
                return CorrectCode() && isCorrect;
            };
            StartCoroutine(PlayMusic());
            IEnumerator PlayMusic()
            {
                yield return new WaitForSeconds(1);
                //Debug.Log("Go!");
                SetUpSong();
            }
        }

        public override void SetUpSong(string component = "")
        {
            // int number;
            // base.SetUpSong(component);
            // if(component == "random")
            // {
            //     codeEvent = new StartCode(Random.Range(1000,9999));
            // }
            // else if(int.TryParse(component, out number))
            // {
            //     codeEvent = new StartCode(number);
            // }
            base.SetUpSong(component);
        }
        public override void StartSong()
        {
            StartSequence(8842);
            base.StartSong();
        }
        // Update is called once per frame
        public override void Update()
        {
            base.Update();
            if (Conductor.instance.music.isPlaying)
            {
                if (Conductor.instance.music.timeSamples < previousTimeSamples && previousTimeSamples > 0)
                {
                    MinigameManager.LoopClear();
                    selectedCharting.AddCharting(Conductor.instance.crochet, minigameName);
                }
                previousTimeSamples = Conductor.instance.music.timeSamples;
            }
        }
        private int previousTimeSamples;
        public void StartSequence(int code)
        {
            if (!startSequence)
            {
                char[] codeString = code.ToString().ToCharArray();
                if (instructions != null)
                {
                    instructions.gameObject.SetActive(true);
                    instructions.text = StringUtils.Replace(instructionsText, "{code}", code.ToString());
                }
                for (int i = 0; i < this.code.Length; i++)
                {
                    if (i >= codeString.Length)
                        this.code[i] = 0;
                    else
                        this.code[i] = int.Parse(codeString[i].ToString());

                    curCode[i] = 0;

                    textList[i].text = "0";
                }

                curKey++;
                startSequence = true;
                // ChangeNumber();
            }
        }
        int prevNum = 0;
        public void ChangeNumber(int b = 0)
        {
            if (startSequence)
            {
                curCode[curKey]++;
                if (curCode[curKey] >= 10) curCode[curKey] = 0;
                if(weirdNumbers)
                {
                    int num = RandomNumber(prevNum);
                    textList[curKey].text = num.ToString();
                    prevNum = num;
                }
                    
                else
                    textList[curKey].text = curCode[curKey].ToString();
                if (blip != null) blip.Play();
            }
        }

        int RandomNumber(int prevNum = 0)
        {
            int num = Random.Range(0, 10);
            while (num == prevNum)
            {
                num = Random.Range(0, 10);
            }
            return num;
        }

        /*public override void OnBeatChange()
        {
            base.OnBeatChange();
            ChangeNumber();
        }*/

        bool CorrectCode()
        {
            for (int i = 0; i < code.Length; i++)
            {
                //Debug.Log(code[i]);
                //Debug.Log(curCode[i]);
                if (code[i] != curCode[i]) return false;
            }
            return true;
        }

        public override void onA(InputAction.CallbackContext context)
        {
            base.onA(context);

            if (startSequence)
            {
                //Debug.Log("Hey!");
                if (button != null) button.Play();
                curKey++;
                prevNum = 0;
                if (curKey >= code.Length)
                {
                    startSequence = false;
                    checker = true;
                    TweenManager.NumTween(() => { return Conductor.instance.music.volume; }, (value) =>
                    {
                        Conductor.instance.music.volume = value;
                    }, 0, Conductor.instance.crochet * 2, Eases.EaseInOutQuad, () =>
                    {
                        Conductor.instance.music.Stop();
                        foreach (TMP_Text text in textList)
                        {
                            text.color = CorrectCode() ? Color.green : Color.red;
                            text.text = curCode[textList.IndexOf(text)].ToString();
                        }
                        instructions.text = CorrectCode() ? "Correct!" : "Wrong!";

                        StartCoroutine(PlayMusic());
                        IEnumerator PlayMusic()
                        {
                            yield return new WaitForSeconds(0.5f);
                            isCorrect = true;
                            // //Debug.Log("Go!");
                            // SetUpSong();
                            if (!CorrectCode())
                            {
                                // Handle incorrect code logic
                                isCorrect = false;
                                checker = false;
                                curKey = -1;
                                Conductor.instance.music.volume = 1;
                                MinigameManager.instance.LoseALife(1f);
                                StartCoroutine(Reset());
                                IEnumerator Reset()
                                {
                                    yield return new WaitForSeconds(0.5f);
                                    foreach (TMP_Text text in textList)
                                    {
                                        text.color = Color.white;
                                        text.text = "0";
                                    }
                                    yield return new WaitForSeconds(0.5f);
                                    StartSong();
                                    // Reset logic here
                                }
                            }
                        }
                    });
                }
            }
        }

        bool checker = false;
        bool isCorrect = false;
    }

    public class ChangeNumber : RhythmEvent
    {
        public ChangeNumber()
        {
            actions = new List<CallForAction>()
            {
                new CallForAction(()=> { game.ChangeNumber(); }, 1)
            };
        }

        SafeCracker game;

        public override void SetUp()
        {
            base.SetUp();
            game = Object.FindObjectOfType<SafeCracker>();
        }
    }
}
