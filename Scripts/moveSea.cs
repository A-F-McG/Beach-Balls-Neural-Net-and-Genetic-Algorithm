using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveSea : MonoBehaviour

{
    //Script to move the 'sea' slowly backwards and forwards to make the scene look more interesting


    Vector3 leftPos;
    Vector3 rightPos;
    bool reachedLeft = false;
    public float moveSeaSpeed;
  

    void Start()
    {
        leftPos = transform.position + Vector3.forward;
        rightPos = transform.position + -Vector3.forward;
    }


    void Update()
    {

        //alternate between moving the sea forwards and backwards

        if (transform.position.z <= leftPos.z && reachedLeft == false)
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + 2*Vector3.forward, moveSeaSpeed*Time.deltaTime);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + -2*Vector3.forward, moveSeaSpeed*Time.deltaTime);

        }
        if (transform.position.z > leftPos.z - 0.05f)
        {
            reachedLeft = true;
        }
        if (transform.position.z < rightPos.z + 0.05f)
        {
            reachedLeft = false;
        }


    }
}
