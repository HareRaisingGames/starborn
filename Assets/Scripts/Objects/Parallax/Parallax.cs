using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float xMultiplier = 1;
     public float yMultiplier = 1;
    public Camera baseCamera;
    private Vector3 position;
    private Vector3 cameraPosition;
    // Start is called before the first frame update
    void Start()
    {
        position = transform.position;
        cameraPosition = baseCamera.transform.position;
        CalculateStartPosition();
    }

    public void CalculateStartPosition()
    {
        float xDist = (baseCamera.transform.position.x - transform.position.x) * xMultiplier;
        float yDist = (baseCamera.transform.position.y - transform.position.y) * yMultiplier;

        Vector3 tmp = new Vector3(position.x, position.y);
        // tmp.x = transform.position.x + xDist;
        // tmp.y = transform.position.y + yDist;

        position = tmp;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        /*transform.position = 
        new Vector3(position.x + (xMultiplier * baseCamera.transform.position.x),
            position.y + (yMultiplier * baseCamera.transform.position.y),
            transform.position.z);*/

        Vector3 pos = position;
        pos.x += xMultiplier * (baseCamera.transform.position.x - cameraPosition.x);
        pos.y += yMultiplier * (baseCamera.transform.position.y - cameraPosition.y);
        transform.position = pos;
    }
}
