using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWay : Obstacle
{
    void OnTriggerExit(Collider other)
    {
        // Debug.Log(other.tag);
        if (other.tag == "Player")
        {
            changeToNormallWall();
        }
    }

    private void changeToNormallWall()
    {
        map.changeMap(x,z,1  );
    }

}
