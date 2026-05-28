using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LoopingBackground : MonoBehaviour
{
    protected SpriteRenderer _renderer;
    protected float startPoint;

    protected float repeatWidth;
    public float speed = 1;

    public bool moveLeft;
    //protected float runSpeed => 60/speed;

    public Vector2 direction
    {
        get
        {
            if(moveLeft)
                return Vector2.right;
            else
                return Vector2.left;
            
        }
    }

    public bool hasHitLoopPoint
    {
        get
        {
            if(repeatWidth == 0)
                return false;
            
            if(direction == Vector2.left)
                if(transform.position.x < startPoint - repeatWidth)
                    return true;
                else
                    return false;

            else if(direction == Vector2.right)
                if(transform.position.x > startPoint + repeatWidth)
                    return true;
                else
                    return false;
            
            return false;
        }
    }
    // Start is called before the first frame update
    void Awake()
    {
        //_renderer = GetComponent<SpriteRenderer>();
        startPoint = transform.position.x;
        repeatWidth = GetComponent<Renderer>().bounds.size.x/2;
    }
    void Start()
    {
        transform.position = new Vector3(startPoint, transform.position.y, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if(hasHitLoopPoint)
        {
            transform.position = new Vector3(startPoint, transform.position.y, transform.position.z);
        }
        transform.Translate(direction * Time.deltaTime * speed);  
    }
}
