using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starborn.GlassPass
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class EasterEgg : MonoBehaviour
    {
        protected string characterName;
        public string character => characterName;
        protected bool flipped;
        public bool isFlipped => flipped;
        protected float moveTime;
        protected float defaultHeight = 816;
        protected float leftX;
        protected float rightX;

        protected SpriteRenderer sprite;
        void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
        }
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public static EasterEgg CreateCharacter(Texture2D texture, float moveTime, Transform parent = null)
        {
            GameObject obj = new GameObject(texture.name);
            obj.transform.parent = parent;
            EasterEgg egg = obj.AddComponent<EasterEgg>();
            egg.characterName = texture.name;
            egg.sprite.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            egg.sprite.sprite.name = texture.name;
            egg.sprite.sortingOrder = -1;
            egg.moveTime = moveTime;
            egg.ResizeSprite(0, 960);
            egg.leftX = MiscUtils.GetWorldBoundsFromScreen().min.x - egg.sprite.bounds.extents.x;
            egg.rightX = MiscUtils.GetWorldBoundsFromScreen().max.x + egg.sprite.bounds.extents.x;
            egg.transform.localPosition = new Vector3(egg.leftX,0,0);
            return egg;
        }

        public void SetFlip(bool flip)
        {
            flipped = flip;
            sprite.flipX = flip;
        }

        public void ResizeSprite(int width = 0, int height = 0)
        {
            if (width <= 0 && height <= 0)
			    return;

            float xScale = width/(float)sprite.sprite.texture.width;
            float yScale = height/(float)sprite.sprite.texture.height;

            transform.localScale = new Vector3(xScale, yScale);
            if (width <= 0)
			    transform.localScale = Vector3.one * yScale;
		    else if (height <= 0)
			    transform.localScale = Vector3.one * xScale;
        }

        public void ResizeSprite(float x = 0, float y = 0)
        {
            if (x <= 0 && y <= 0)
			    return;

            transform.localScale = new Vector3(x, y);

            if (x <= 0)
			    transform.localScale = Vector3.one * y;
		    else if (y <= 0)
			    transform.localScale = Vector3.one * x;
        }

        public void StartMoving(System.Action callback = null)
        {
            if(flipped) transform.localPosition = new Vector3(rightX,0,0);
            TweenManager.LocalXTween(gameObject, 
                flipped ? rightX : leftX, flipped ? leftX : rightX, moveTime, Eases.Linear, callback);
        }
    }
}

