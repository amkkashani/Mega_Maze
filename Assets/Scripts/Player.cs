using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float minmumAcceptableDistance = 0.1f;
    [SerializeField] private float reachSpeedFactor = 0.05f;
    [SerializeField] private float explosionRadius = 1f;
    [SerializeField] private int points = 0;
    [SerializeField] private int bombNumber = 5;
    
    private Vector3 finalTarget;
    private int[] posIndex;
    


    public void setPos(Vector3 target, int[] posIndex, bool fast = false)
    {
        this.posIndex = posIndex;
        finalTarget = target;
        if (fast)
        {
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
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (moveToIndex(posIndex[0], posIndex[1] + 1))
            {
                posIndex[1] += 1;
            }
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (moveToIndex(posIndex[0], posIndex[1] - 1))
            {
                posIndex[1] -= 1;
            }
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (moveToIndex(posIndex[0] - 1, posIndex[1]))
            {
                posIndex[0] -= 1;
            }
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (moveToIndex(posIndex[0] + 1, posIndex[1]))
            {
                posIndex[0] += 1;
            }
        }

        if (Vector3.Distance(transform.position, finalTarget) > minmumAcceptableDistance)
        {
            transform.Translate((finalTarget - transform.position) * reachSpeedFactor);
        }

        if (Input.GetKeyDown(KeyCode.Keypad0) && bombNumber != 0)
        {
            bombNumber--;
            destroyEnv();
        }
    }

    //
    //return true if possible
    //return false in case of index out of bound or collision with walls
    private bool moveToIndex(int x, int z)
    {
        // Debug.Log("i want go "+ x + " - "+ z);
        Vector3? nextPos = MapCreator.Instance.calculatePosInMap(x, z, this.transform) + Vector3.up;
        if (nextPos != null)
        {
            setPos(nextPos);
            return true;
        }

        return false;
    }

    private void destroyEnv()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.tag == "Wall")
            {
                hitCollider.GetComponent<Obstacle>()?.wallDestruction();
            }
        }
    }

    public void getPoint(int value)
    {
        this.points += value;
    }
}