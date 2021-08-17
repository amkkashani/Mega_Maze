using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class Player : Agent
{
    [SerializeField] private float minmumAcceptableDistance = 0.1f;
    [SerializeField] private float reachSpeedFactor = 0.05f;
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private int points = 0;
    [SerializeField] private int bombNumber = 5;
    [SerializeField] private int ultimateNumber = 5;
    [SerializeField] private GameObject ultimateEffect;
    
    private Vector3 finalTarget;
    private int[] posIndex;
    private bool ultimateIsActive = false;

    private Map map;

    void Start()
    {
        ultimateEffect.SetActive(false);
    }

    public void getUltimateAndBombNumber(ref int ultimate , ref int bombNumber)
    {
        ultimate = this.ultimateNumber;
        bombNumber = this.bombNumber;
    }

    public void setUltimateAndBombNumber(int ultimate, int bombNumber)
    {
        this.ultimateNumber = ultimate;
        this.bombNumber = bombNumber;
    }
    
    
    // when use fast == true you want to setup firs location of player and also you say player plays in which map 
    public void setPos(Vector3 target, int[] posIndex, bool fast = false , Map map =null)
    {
        this.posIndex = posIndex;
        finalTarget = target;
        if (fast)
        {
            this.map = map; //first time must pass the map object to function
            this.transform.position = target;
        }

        // Debug.Log(posIndex[0] + "---" + posIndex[1]);
    }

    private void setPos(Vector3? target, bool fast = false)
    {
        if (target == null)
            return;
        finalTarget = (Vector3) target;
        if (fast)
        {
            this.transform.position = (Vector3) target;
        }
    }

    public void Update()
    {
        //try to reach at target  point 
        if (Vector3.Distance(transform.position, finalTarget) > minmumAcceptableDistance)
        {
            transform.Translate((finalTarget - transform.position) * reachSpeedFactor);
        }
    }

    // public void Update()
    // {
        // if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        // {
        //     if (moveToIndex(posIndex[0], posIndex[1] + 1, ultimateIsActive))
        //     {
        //         posIndex[1] += 1;
        //         ultimateIsActive = false;
        //         ultimateEffect.SetActive(false);
        //     }
        // }
        // else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        // {
        //     if (moveToIndex(posIndex[0], posIndex[1] - 1,ultimateIsActive))
        //     {
        //         ultimateIsActive = false;
        //         ultimateEffect.SetActive(false);
        //         posIndex[1] -= 1;
        //     }
        // }
        // else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        // {
        //     if (moveToIndex(posIndex[0] - 1, posIndex[1],ultimateIsActive))
        //     {
        //         ultimateIsActive = false;
        //         ultimateEffect.SetActive(false);
        //         posIndex[0] -= 1;
        //     }
        // }
        // else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        // {
        //     if (moveToIndex(posIndex[0] + 1, posIndex[1],ultimateIsActive))
        //     {
        //         ultimateIsActive = false;
        //         ultimateEffect.SetActive(false);
        //         posIndex[0] += 1;
        //     }
        // }
        //
        // if (Vector3.Distance(transform.position, finalTarget) > minmumAcceptableDistance)
        // {
        //     transform.Translate((finalTarget - transform.position) * reachSpeedFactor);
        // }
        //
        // if (Input.GetKeyDown(KeyCode.Keypad0) && bombNumber != 0)
        // {
        //     
        //     destroyEnvBomb();
        // }
        //
        // if (Input.GetKeyDown(KeyCode.Keypad1))
        // {
        //     activeUltimate();
        // }
    // }

    public override void OnActionReceived(float[] vectorAction)
    {
        switch (vectorAction[0])
        {
            case 0:  //do nothing
                break;
            case 1:  //go up
                if (moveToIndex(posIndex[0], posIndex[1] + 1, ultimateIsActive))
                {
                    posIndex[1] += 1;
                    ultimateIsActive = false;
                    ultimateEffect.SetActive(false);
                }    
                break;
            
            case 2: // go right
                if (moveToIndex(posIndex[0] + 1, posIndex[1],ultimateIsActive))
                {
                    ultimateIsActive = false;
                    ultimateEffect.SetActive(false);
                    posIndex[0] += 1;
                }    
                break;
            case 3: // go down
                if (moveToIndex(posIndex[0], posIndex[1] - 1,ultimateIsActive))
                {
                    ultimateIsActive = false;
                    ultimateEffect.SetActive(false);
                    posIndex[1] -= 1;
                }
                break;
            case 4: // go left
                if (moveToIndex(posIndex[0] - 1, posIndex[1],ultimateIsActive))
                {
                    ultimateIsActive = false;
                    ultimateEffect.SetActive(false);
                    posIndex[0] -= 1;
                }    
                break;
        }

        if (vectorAction[1] == 1)
        {
            activeUltimate();
        }

        if (vectorAction[2] == 1)
        {
            destroyEnvBomb();
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        //action
        // index [0]=>  0: stay , up:1 , right : 2 ,down :3 , left :4 
        // index [1]=> 0:not to do , 1: activate the ultimate
        // index [2]=> 0:not to do , 1: activate the bomb
        // by default we do nothing
        actionsOut[0] = 0;
        actionsOut[1] = 0;
        actionsOut[2] = 0;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            actionsOut[0] = 1;
            
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            actionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            actionsOut[0] = 4;
            
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            actionsOut[0] = 2;
            
        }
        if (Input.GetKey(KeyCode.Keypad0) && bombNumber != 0)
        {
            actionsOut[2] = 1;
            
        }

        if (Input.GetKey(KeyCode.Keypad1))
        {
            actionsOut[1] = 1;
        }
    }

    private void activeUltimate()
    {
        if (ultimateIsActive || ultimateNumber ==0)
            return; // if ultimate is already active no need to turn it on
        ultimateIsActive = true;
        ultimateNumber--;
        ultimateEffect.SetActive(true);
    }
    
    
    //return true if possible
    //return false in case of index out of bound or collision with walls
    private bool moveToIndex(int x, int z , bool isUltimateActive)
    {
        // Debug.Log("i want go "+ x + " - "+ z);
        Vector3? nextPos = map.calculatePosInMap(x, z, this.transform,isUltimateActive) + Vector3.up;
        if (nextPos != null)
        {
            setPos(nextPos);
            return true;
        }

        return false;
    }

    private void destroyEnvBomb()
    {
        bombNumber--;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.tag == "Wall")
            {
                hitCollider.GetComponent<Obstacle>()?.wallDestruction();
            }
        }
    }

    public void addPoint(int value)
    {
        this.points += value;
    }

    public int getPoint()
    {
        return points;
    }
}