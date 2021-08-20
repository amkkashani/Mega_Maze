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
            other.GetComponent<Player>()?.addPoint(1);
            wallDestruction();
            if (other.GetComponent<Player>().getPoint() == map.getNumberOfGoals())
            {
                //todo finish
            }
        }
    }
   
    
    public void Start()
    {
        Debug.Log("goal is started");
    }
}
