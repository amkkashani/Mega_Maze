using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField]protected GameObject afterDestroyed;
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

    protected void changeToNormallWall()
    {
        MapCreator.Instance.changeMap(x,z,1  );
    }

    public void wallDestruction()
    {
        MapCreator.Instance.changeMap(x,z,0);
        Destroy(Instantiate(afterDestroyed, transform.position,
            Quaternion.identity), 2);
    }
}
