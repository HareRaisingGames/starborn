using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Starborn.InputSystem;

namespace Starborn.LemonDrop
{
    public class Lemon : MonoBehaviour
    {
        public Transform lemonParent;
        public Rigidbody skin;
        public Rigidbody front;
        public Rigidbody slice1;
        public Rigidbody slice2;
        public Rigidbody back;

        Vector3 skinPos;
        Vector3 frontPos;
        Vector3 slice1Pos;
        Vector3 slice2Pos;
        Vector3 backPos;

        Quaternion skinRot;
        Quaternion frontRot;
        Quaternion slice1Rot;
        Quaternion slice2Rot;
        Quaternion backRot;

        FixedJoint frontToSlice;
        FixedJoint sliceToSlice;
        FixedJoint sliceToBack;

        Tween<float> xRotating;
        Tween<float> yRotating;

        LemonDrop game;

        bool isLime = false;
        public bool lime => isLime;
        [Header("Lemon Type")]
        public Texture2D lemonTexture;
        public Texture2D limeTexture;
        protected readonly Color lemonColor = Color.yellow;
        protected readonly Color limeColor = Color.green;

        protected readonly Color lemonCitrusColor = new Color(255,237,86);
        protected readonly Color limeCitrusColor = new Color(174,255,86);

        protected Gradient lemonGradient = new Gradient();
        protected Gradient limeGradient = new Gradient();

        protected Quaternion defaultRotation;

        [Header("Physics Settings")]
        public Vector3 torqueAxis = Vector3.up; 
        public float inertiaTensor = 1.0f;     // Simulates the object's resistance to rotation
        public float angularDrag = 0.5f;
        private Vector3 angularVelocity = Vector3.zero;
        bool missed = false;

        AudioSource _squeeze;

        [Header("Particles")]
        public ParticleSystem splash1;
        public ParticleSystem splash2;
        void Awake()
        {
            // Seeds the generator using the current time so it is completely unique every play
            int dynamicSeed = (int)System.DateTime.Now.Ticks;
            Random.InitState(dynamicSeed);

            lemonGradient.SetKeys(
                new GradientColorKey[]{
                    new GradientColorKey(lemonCitrusColor, 0.0f), new GradientColorKey(lemonCitrusColor,1.0f)},
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 0.5f)
                }
            );

            limeGradient.SetKeys(
                new GradientColorKey[]{
                    new GradientColorKey(limeCitrusColor, 0.0f), new GradientColorKey(limeCitrusColor, 1.0f)},
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 0.5f)
                }
            );
        }

        // protected float minX = -15f;
        // protected float maxX = 15f;
        // protected float middleX = Mathf.Lerp(-15f, 15f, 0.5f);
        // protected float minY = 60f;
        // protected float maxY = 120f;
        // protected float middleY = Mathf.Lerp(60f, 120f, 0.5f);
        // protected bool reverseX = false;
        // protected bool reverseY = false;

        // Start is called before the first frame update
        void Start()
        {
            defaultRotation = transform.rotation;
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

            skinPos = skin.gameObject.transform.localPosition;
            frontPos = front.gameObject.transform.localPosition;
            slice1Pos = slice1.gameObject.transform.localPosition;
            slice2Pos = slice2.gameObject.transform.localPosition;
            backPos = back.gameObject.transform.localPosition;

            skinRot = skin.transform.localRotation;
            frontRot = front.transform.localRotation;
            slice1Rot = slice1.transform.localRotation;
            slice2Rot = slice2.transform.localRotation;
            backRot = back.transform.localRotation;

            game = FindObjectOfType<LemonDrop>();

            frontToSlice.connectedBody = slice1;
            sliceToSlice.connectedBody = slice2;
            sliceToBack.connectedBody = back;

            if (GameObject.Find("Squeeze") == null)
            {
                GameObject gameObject = new GameObject("Squeeze");
                _squeeze = gameObject.AddComponent<AudioSource>();
                _squeeze.clip = Resources.Load<AudioClip>("Audio/LemonDrop/squeeze");
            }
            else
                _squeeze = GameObject.Find("Squeeze").GetComponent<AudioSource>();

        }
        public void Reassemble()
        {
            skin.gameObject.SetActive(true);

            skin.transform.parent = lemonParent;
            front.transform.parent = lemonParent;
            slice1.transform.parent = lemonParent;
            slice2.transform.parent = lemonParent;
            back.transform.parent = lemonParent;

            skin.transform.localPosition = skinPos;
            front.transform.localPosition = frontPos;
            slice1.transform.localPosition = slice1Pos;
            slice2.transform.localPosition = slice2Pos;
            back.transform.localPosition = backPos;

            skin.transform.localRotation = skinRot;
            front.transform.localRotation = frontRot;
            slice1.transform.localRotation = slice1Rot;
            slice2.transform.localRotation = slice2Rot;
            back.transform.localRotation = backRot;

            skin.isKinematic = true;
            front.isKinematic = true;
            slice1.isKinematic = true;
            slice2.isKinematic = true;
            back.isKinematic = true;

            if (front.gameObject.GetComponent<FixedJoint>() == null)
                front.gameObject.AddComponent<FixedJoint>();
            if (slice1.gameObject.GetComponent<FixedJoint>() == null)
                slice1.gameObject.AddComponent<FixedJoint>();
            if (slice2.gameObject.GetComponent<FixedJoint>() == null)
                slice2.gameObject.AddComponent<FixedJoint>();

            frontToSlice = front.gameObject.GetComponent<FixedJoint>();
            sliceToSlice = slice1.gameObject.GetComponent<FixedJoint>();
            sliceToBack = slice2.gameObject.GetComponent<FixedJoint>();

            // Random.InitState(42);
            float random = Random.Range(0f, 1f);
            bool lime = random <= 0.1f;
            ChangeType(skin.gameObject, lime);
            ChangeType(front.gameObject, lime);
            ChangeType(slice1.gameObject, lime);
            ChangeType(slice2.gameObject, lime);
            ChangeType(back.gameObject, lime);

            missed = false;
            transform.rotation = defaultRotation;

            if(game != null)
                game.cutCount = 0;

            // if(splash1 != null)
                // splash1.colorOverLifetime.
        }
        protected float xTime = 3.5f;
        protected float yTime = 3f;
        // Update is called once per frame
        void Update()
        {
            if(!missed)
            {
                //Created a swing motion for the lemon, using PingPong to oscillate between two values over time. 
                // The rotation is applied to the transform of the lemon object.
                // The absolute value of the PingPong is to make sure it forms a penduilum motion.
                float x = Mathf.Abs(Mathf.PingPong(Time.time / xTime, 2) - 1)/5f;
                float y = Mathf.Abs(Mathf.PingPong(Time.time / yTime, 2) - 1)/5f;
                transform.Rotate(new Vector3(x,y,1) * 100 * Time.deltaTime);

            }
            else
            {
                angularVelocity -= angularVelocity * angularDrag * Time.deltaTime;
                if (angularVelocity != Vector3.zero)
                {
                    // Convert angular velocity vector to a rotation step
                    float angle = angularVelocity.magnitude * Time.deltaTime * Mathf.Rad2Deg;
                    Vector3 axis = angularVelocity.normalized;

                    transform.Rotate(axis, angle, Space.World);
                }
            }
            
        }

        public void Cut(float state)
        {
            skin.isKinematic = false;
            skin.gameObject.SetActive(false);

            if(game != null)
                game.cutCount++;

            _squeeze.Play();

            if (state >= 1)
            {
                front.isKinematic = false;
                front.transform.parent = null;
            }
            if (state >= 2)
            {
                
            }
            if (state >= 3)
            {
                slice1.transform.parent = null;
                slice1.isKinematic = false;
                slice2.isKinematic = false;
                back.isKinematic = false;
                slice2.transform.parent = null;
                back.transform.parent = null;
            }

            switch (state)
            {
                case 1:
                    Destroy(frontToSlice);
                    front.AddForce(transform.up * 10);
                    front.AddForce(-transform.forward * 100);
                    if (splash1 != null)
                        splash1.Play();
                    break;
                case 2:
                    Destroy(sliceToSlice);
                    slice1.AddForce(transform.up * 10);
                    slice1.AddForce(-transform.forward * 100);
                    break;
                case 3:
                    Destroy(sliceToBack);
                    slice2.AddForce(-transform.forward * 100);

                    slice2.AddForce(transform.up * 10);

                    back.AddForce(transform.forward * 100);
                    if (splash2 != null)
                        splash2.Play();
                    break;
            }
        }

        public void Missed()
        {
            missed = true;
            Debug.Log("Whamp");
            AddTorqueCustom(torqueAxis * 10f);
        }

        public void Fall()
        {
            skin.isKinematic = false;
            front.isKinematic = false;
            slice1.isKinematic = false;
            slice2.isKinematic = false;
            back.isKinematic = false;
        }

        public void AddThrow(float beat, float x = 0.5f, float y = 0.5f)
        {
            RhythmInput input = new RhythmInput(RhythmInputs.A)
                .SetDestination(beat)
                    .SetRange(x, y);
            input.Enable();
        }

        public void ChangeType(GameObject lemonObject, bool isLime)
        {
            this.isLime = isLime;
            Material[] materials = lemonObject.GetComponent<MeshRenderer>().materials;
            foreach (Material material in materials)
            {
                if(material.name.Contains("Inner"))
                {
                    material.mainTexture = isLime ? limeTexture : lemonTexture;
                }
                else if(material.name.Contains("Cut_Skin") || material.name.Contains("Skin"))
                {
                    material.color = isLime ? limeColor : lemonColor;
                }
                else if(material.name.Contains("OutlineFill"))
                {
                    material.SetColor("_OutlineColor", Color.black);
                    material.SetFloat("_OutlineWidth", 10f);
                    material.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                }
            }
            var col1 = splash1.colorOverLifetime;
            col1.color = isLime ? limeGradient : lemonGradient;

            var col2 = splash2.colorOverLifetime;
            col2.color = isLime ? limeGradient : lemonGradient;
        }

        public void AddTorqueCustom(Vector3 torque)
        {
            // Physics formula: Torque = Inertia * Angular Acceleration
            // Therefore: Angular Acceleration = Torque / Inertia
            Vector3 angularAcceleration = torque / Mathf.Max(0.01f, inertiaTensor);

            // Accumulate acceleration into velocity
            angularVelocity += angularAcceleration * Time.deltaTime;
        }
    }
}

