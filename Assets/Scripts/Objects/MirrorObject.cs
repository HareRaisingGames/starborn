using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorObject : MonoBehaviour
{
    public GameObject original;
    public Vector3 offset;
    public enum MirrorDirection
    {
        X,
        Y,
        Z
    }
    public MirrorDirection direction;
    Vector3 _vectorDirection
    {
        get
        {
            switch(direction)
            {
                case MirrorDirection.X:
                    return new Vector3(-1, 1, 1);
                case MirrorDirection.Y:
                    return new Vector3(1, -1, 1);
                case MirrorDirection.Z:
                    return new Vector3(1, 1, -1);
                default:
                    return Vector3.one;
            }
        }
    }
    public bool lerp = true;
    public float lerpAmount = 25;
    // Start is called before the first frame update
    void Start()
    {
        if (original != null)
        {
            transform.position = Vector3.Scale(original.transform.position, _vectorDirection) + offset;
            transform.rotation = original.transform.rotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(original != null)
        {
            if(lerp)
            {
                transform.position = Vector3.Lerp(transform.position, Vector3.Scale(original.transform.position, _vectorDirection) + offset, Time.deltaTime * lerpAmount);
                transform.rotation = Quaternion.Lerp(transform.rotation, original.transform.rotation, Time.deltaTime * lerpAmount);
            }
            else
            {
                transform.position = Vector3.Scale(original.transform.position, _vectorDirection) + offset;
                transform.rotation = original.transform.rotation;
            }

        }
    }
}
