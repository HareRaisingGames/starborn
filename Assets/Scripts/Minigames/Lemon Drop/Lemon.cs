using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;

public class Lemon : MonoBehaviour
{
    public Rigidbody skin;
    public Rigidbody front;
    public Rigidbody slice1;
    public Rigidbody slice2;
    public Rigidbody back;

    FixedJoint frontToSlice;
    FixedJoint sliceToSlice;
    FixedJoint sliceToBack;

    Tween<float> xRotating;
    Tween<float> yRotating;

    // Start is called before the first frame update
    void Start()
    {
        if (front.gameObject.GetComponent<FixedJoint>() == null)
            front.gameObject.AddComponent<FixedJoint>();
        if (slice1.gameObject.GetComponent<FixedJoint>() == null)
            slice1.gameObject.AddComponent<FixedJoint>();
        if (slice2.gameObject.GetComponent<FixedJoint>() == null)
            slice2.gameObject.AddComponent<FixedJoint>();

        frontToSlice = front.gameObject.GetComponent<FixedJoint>();
        sliceToSlice = slice1.gameObject.GetComponent<FixedJoint>();
        sliceToBack = slice2.gameObject.GetComponent<FixedJoint>();

        skin.isKinematic = true;
        front.isKinematic = true;
        slice1.isKinematic = true;
        slice2.isKinematic = true;
        back.isKinematic = true;

        frontToSlice.connectedBody = slice1;
        sliceToSlice.connectedBody = slice2;
        sliceToBack.connectedBody = back;

        xRotating = TweenManager.PitchTween(gameObject, -15, 15, 3.5f, Eases.EaseInOutQuad).SetPingPong(1000);
        yRotating = TweenManager.YawTween(gameObject, 60, 120, 3, Eases.EaseInOutQuad).SetPingPong(1000);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward * 100 * Time.deltaTime);
    }

    public void Cut(float state)
    {
        skin.isKinematic = false;
        skin.gameObject.SetActive(false);

        if(state >= 1)
        {
            front.isKinematic = false;
            front.transform.parent = null;
        }
        if(state >= 2)
        {
            slice1.isKinematic = false;
            slice1.transform.parent = null;
        }
        if (state >= 3)
        {
            slice2.isKinematic = false;
            back.isKinematic = false;
            slice2.transform.parent = null;
            back.transform.parent = null;
        }

        switch(state)
        {
            case 1:
                Destroy(frontToSlice);
                front.AddForce(transform.up * 10);
                front.AddForce(-transform.forward * 100);
                break;
            case 2:
                Destroy(sliceToSlice);
                slice1.AddForce(transform.up * 10);
                slice1.AddForce(-transform.forward * 100);
                break;
            case 3:
                Destroy(sliceToBack);
                slice2.AddForce(-transform.forward * 100);
                //back.AddForce(transform.forward * 100);

                slice2.AddForce(transform.up * 10);
                //back.AddForce(transform.up * 10);
                break;
        }
    }
}
