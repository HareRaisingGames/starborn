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

        StartCode codeEvent;

        //string saveInstructions;

        public override void Awake()
        {
            safeCracker = this;
            base.Awake();
        }

        // Start is called before the first frame update
        public override void Start()
        {
            codeEvent = new StartCode(8842);
            if (instructions != null) instructions.gameObject.SetActive(false);
            //saveInstructions = instructions.text;
            base.Start();
            Conductor.instance.SetUpBPM();
            codeEvent.AddToChart(0);
            StartCoroutine(PlayMusic());
            IEnumerator PlayMusic()
            {
                yield return new WaitForSeconds(1);
                //Debug.Log("Go!");
                Conductor.instance.music.Play();
            }
        }

        // Update is called once per frame
        public override void Update()
        {
            base.Update();
        }

        public void StartSequence(int code)
        {
            if(!startSequence)
            {
                char[] codeString = code.ToString().ToCharArray();
                if (instructions != null)
                {
                    instructions.gameObject.SetActive(true);
                    instructions.text = StringUtils.Replace(instructions.text, "{code}", code.ToString());
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
                ChangeNumber();
            }
        }

        void ChangeNumber()
        {
            if (startSequence)
            {
                curCode[curKey]++;
                if (curCode[curKey] >= 10) curCode[curKey] = 0;
                textList[curKey].text = curCode[curKey].ToString();

                if (blip != null) blip.Play();
            }
        }

        public override void OnBeatChange()
        {
            base.OnBeatChange();
            ChangeNumber();
        }

        bool CorrectCode()
        {
            for(int i = 0; i < code.Length; i++)
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

            if(startSequence)
            {
                //Debug.Log("Hey!");
                if (button != null) button.Play();
                curKey++;
                if (curKey >= code.Length)
                {
                    startSequence = false;
                    TweenManager.NumTween(()=> { return Conductor.instance.music.volume; }, (value) => {
                        Conductor.instance.music.volume = value;
                    }, 0, Conductor.instance.crochet * 2, Eases.EaseInOutQuad, () => {

                        Conductor.instance.music.Stop();
                        foreach(TMP_Text text in textList)
                        {
                            text.color = CorrectCode() ? Color.green : Color.red;
                            instructions.text = CorrectCode() ? "Correct!" : "Wrong!";
                        }

                    });
                }
            }
        }
    }

    public class StartCode : RhythmEvent
    {
        public StartCode(int code)
        {
            actions = new List<CallForAction>()
            {
                new CallForAction(()=> {SafeCracker.safeCracker.StartSequence(code); }, 1)
            };
        }
    }
}
