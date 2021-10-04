using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : Obstacle
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerParent>()?.addPoint(1);
            wallDestruction();
        }
    }
}
