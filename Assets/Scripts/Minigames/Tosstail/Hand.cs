using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starborn.Tosstail {
    public class Hand : MonoBehaviour
    {
        public bool rightHand;
        Niko niko;
        // Start is called before the first frame update
        void Start()
        {
            niko = FindObjectOfType<Niko>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void UpdateHand()
        {
            if(rightHand)
            {
                niko.rightArm.Update();
            }
            else
            {
                niko.leftArm.Update();
            }
        }
    }
}

