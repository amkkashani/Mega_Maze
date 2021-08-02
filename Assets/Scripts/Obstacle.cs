using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    protected int x;
    protected int z;

    // public Walls(int x , int z)
    // {
    //     this.x = x;
    //     this.z = z;
    // }

    public void setterXZ(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    protected void changeToNormallWall(Material material)
    {
        MapCreator.Instance.changeMap(x,z,1 );
    }
}
