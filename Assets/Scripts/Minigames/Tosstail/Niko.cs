using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Starborn.Tosstail
{
    public class Niko : MonoBehaviour
    {
        [Header("Body")]
        public GameObject body;
        public SpriteRenderer head;
        public Animator headAnimator;
        bool _canBlink = true;
        public bool canBlink
        {
            set
            {
                _canBlink = value;
            }
        }

        public Dictionary<string, Sprite> expressionDictionary = new Dictionary<string, Sprite>();
        public List<Expression> expressions = new List<Expression>();

        [HideInInspector]
        public Expression defaultExpression;
        Expression curExpression;


        //Will be upgraded to bouncing up and down
        [System.Serializable]
        public class Forearm
        {
            public SpriteRenderer sprite;
            public SpriteRenderer hand;
            [System.Serializable]
            public enum ArmAction
            {
                open,
                close,
                n
            }

            [System.Serializable]
            public class ArmData
            {
                public ArmAction name;
                public Sprite texture;
                public Vector3 offset;
            }

            public ArmData[] data = new ArmData[2];
            //Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
            //Dictionary<string, Vector3> positions = new Dictionary<string, Vector3>();
            Dictionary<Sprite, Vector3> spriteToPositions = new Dictionary<Sprite, Vector3>();

            public Animator animator;
            //public List<ArmData> armData = new List<ArmData>();
            //Speed = BPM/120

            public void SetUpDictionary(bool isOpen = false)
            {
                /*foreach (ArmData d in data)
                {
                    if (!sprites.ContainsKey(d.name.ToString().ToLower()))
                        sprites.Add(d.name.ToString().ToLower(), d.texture);
                    if (!positions.ContainsKey(d.name.ToString().ToLower()))
                        positions.Add(d.name.ToString().ToLower(), d.offset);
                }

                foreach(ArmData data in armData)
                {
                    if(!spriteToPositions.ContainsKey(data.texture))
                        spriteToPositions.Add(data.texture, data.offset);
                }*/

                _isOpen = isOpen;

                if(isOpen)
                {
                    hand.gameObject.SetActive(false);
                    animator.Play("Catch");
                    Update();
                }
                else
                {
                    hand.gameObject.SetActive(true);
                    animator.Play("Idle");
                    Update();
                }
            }

            public void SetArm(string data, bool input = false)
            {
                switch (data.ToLower())
                {
                    case "open":
                        hand.gameObject.SetActive(false);
                        if (!input)
                            animator.Play("Toss");
                        else
                            animator.Play("Catch");
                        Update();
                        //sprite.sprite = sprites["open"];
                        //sprite.transform.localPosition = positions["open"];
                        break;
                    case "close":
                        hand.gameObject.SetActive(true);
                        animator.Play("Idle");
                        Update();
                        //sprite.sprite = sprites["close"];
                        //sprite.transform.localPosition = positions["close"];
                        break;
                }
            }
            bool _isOpen;
            public bool isOpen
            {
                get
                {
                    return _isOpen;
                }
                set
                {
                    SetArm(value ? "open" : "close");
                    _isOpen = value;
                }
            }
            bool _isFree;
            public bool isFree
            {
                get
                {
                    return _isFree;
                }
                set
                {
                    _isFree = value;
                }
            }

            public void Update()
            {
                if(spriteToPositions.ContainsKey(sprite.sprite))
                    sprite.transform.localPosition = spriteToPositions[sprite.sprite];
            }

            public void SetSpeed(float bpm = 120)
            {
                animator.speed = bpm / 120;
            }

            public void SetHand(bool open)
            {
                SetArm(open ? "open" : "close", true);
                _isOpen = open;
            }
        }

        public Forearm leftArm;
        public Forearm rightArm;

        [HideInInspector]
        public Forearm curArm;

        public static Niko instance;

        bool _reaction;
        public bool reaction
        {
            get
            {
                return _reaction;
            }
            set
            {
                _reaction = value;
            }
        }

        [Header("Data")]
        public string catchName;
        public float y = 0;
        public float yOffset = -1;
        bool bounce = true;

        [HideInInspector]
        public bool handSetter;
        void Awake()
        {
            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            foreach (Expression expression in expressions)
            {
                expressionDictionary.Add(expression.name, expression.texture);
            }

            if (expressions.Count != 0)
                defaultExpression = expressions[0];

            StartBlink();
        }

        // Update is called once per frame
        void Update()
        {
            if (body != null)
            {
                body.transform.position =
                    Vector3.LerpUnclamped(body.transform.position, new Vector3(body.transform.position.x, y, body.transform.position.z), Time.deltaTime * 25);
            }

            if(handSetter)
            {
                if (leftArm != null)
                {
                    AnimatorClipInfo[] clipInfo = leftArm.animator.GetCurrentAnimatorClipInfo(0);
                    if (clipInfo.Length > 0)
                    {
                        // The name of the first clip in the list
                        string currentAnimationName = clipInfo[0].clip.name;
                        leftArm.hand.gameObject.SetActive(currentAnimationName == "Idle");
                    }
                }

                if (rightArm != null)
                {
                    AnimatorClipInfo[] clipInfo = rightArm.animator.GetCurrentAnimatorClipInfo(0);
                    if (clipInfo.Length > 0)
                    {
                        // The name of the first clip in the list
                        string currentAnimationName = clipInfo[0].clip.name;
                        rightArm.hand.gameObject.SetActive(currentAnimationName == "Idle");
                    }
                }
            }


            if (headAnimator != null && headAnimator.enabled)
            {
                if (headAnimator.GetCurrentAnimatorStateInfo(0).IsName("Blink"))
                {
                    if(headAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f)
                    {
                        if(!_canBlink)
                        {
                            StopCoroutine("Blink");
                            headAnimator.enabled = false;
                        }
                    }
                }
                    
            }

            //Code for when the blinking animation is playing

        }

        public void StartBlink()
        {
            _canBlink = true;
            headAnimator.enabled = true;
            StartCoroutine("Blink");
        }

        IEnumerator Blink()
        {
            float randomWait = Random.Range(2, 5);
            yield return new WaitForSeconds(randomWait);
            headAnimator.Play(MiscUtils.Random("Blink", "DoubleBlink"));
            StartCoroutine("Blink");
        }

        public void SetExpression(string expression)
        {
            if (expressionDictionary.ContainsKey(expression))
            {
                head.sprite = expressionDictionary[expression];
                foreach (Expression express in expressions)
                {
                    if (express.name == expression)
                        curExpression = express;
                }
            }

        }

        public void Bounce()
        {
            if (body != null && bounce)
            {
                body.transform.position =
                    new Vector3(body.transform.position.x, yOffset, body.transform.position.z);
            }

            if (!_reaction)
                SetExpression(defaultExpression.name);

            if (curExpression.name == catchName)
                _reaction = false;
        }

        public void StopBouncing() => bounce = false;
        public void StartBouncing(){
            Bounce();
            bounce = true;
        }
    }
}

[System.Serializable]
public struct Expression
{
    public string name;
    public Sprite texture;
}
