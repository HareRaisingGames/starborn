using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartReader : MonoBehaviour
{
    public LineRenderer line;
    float[] startAndEnd = new float[2];
    // Start is called before the first frame update
    void Start()
    {
        //Set a start and end point
        if(line != null)
        {
            startAndEnd[0] = line.GetPosition(0).x;
            startAndEnd[1] = line.GetPosition(line.positionCount - 1).x;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
