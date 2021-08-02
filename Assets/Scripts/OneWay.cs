using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWay : Obstacle
{
    [SerializeField] private Material _material;
    


    void OnTriggerExit(Collider other)
    {
        Debug.Log(other.tag);
        if (other.tag == "Player")
        {
            //todo 
            changeToNormallWall(_material);
        }
    }

    
}
