using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField]protected GameObject afterDestroyed;
    protected int x;
    protected int z;
    protected Map map;

    // public Walls(int x , int z)
    // {
    //     this.x = x;
    //     this.z = z;
    // }

    public void setterXZ(int x, int z,Map map)
    {
        this.x = x;
        this.z = z;
        this.map = map;
    }

    protected void changeToNormallWall()
    {
        map.changeMap(x,z,1  );
    }

    
    
    public void wallDestruction()
    {
        map.changeMap(x,z,0);
        Destroy(Instantiate(afterDestroyed, transform.position,
            Quaternion.identity), 2);
        // Debug.Log(x +" --" + z);
    }
}
