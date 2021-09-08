using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : Obstacle
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("i catched by pleayer");
            other.GetComponent<PlayerParent>()?.addPoint(1);
            wallDestruction();
            // if (other.GetComponent<PlayerParent>().getPoint() == map.getNumberOfGoals())
            // {
            //     //todo finish
            // }
        }
    }
}
