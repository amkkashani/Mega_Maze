using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : Obstacle
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<Player>()?.addPoint(1);
            wallDestruction();
            if (other.GetComponent<Player>().getPoint() == MapCreator.Instance.getNumberOfGoals())
            {
                //todo finish
            }
        }
    }
}
