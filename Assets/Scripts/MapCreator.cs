using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapCreator : Singleton<MapCreator>
{
    [SerializeField] private float blockSizeOfMap = 1.0f;
    [SerializeField] private float chanceOfEmptyWall; //0
    [SerializeField] private GameObject EmptyWall;
    [SerializeField] private float chanceOfNormallWall; //1
    [SerializeField] private GameObject normallWall;
    [SerializeField] private float chanceOfOnWayWall; //2
    [SerializeField] private GameObject oneWayWall;
    [SerializeField] private int numberOfGoals;
    [SerializeField] private GameObject _goal;

    [SerializeField] private GameObject parentObj;

    [SerializeField] private GameObject player;

    private int x, z;
    private int[,] map;
    private int[] _playerPos = new int[2];
    private List<int[]> emptyList;

    public void Awake()
    {
        emptyList = new List<int[]>();
        // Debug.Log("debuged");
        x = (int) ((int) this.transform.localScale.x / blockSizeOfMap);
        z = (int) ((int) this.transform.localScale.z / blockSizeOfMap);
        map = new int[x, z];
    }

    public void Start()
    {
        float sum = chanceOfEmptyWall + chanceOfNormallWall + chanceOfOnWayWall;
        float probEmptyWall = chanceOfEmptyWall / sum;
        float probNormallWall = chanceOfNormallWall / sum;
        float probOneWayWall = chanceOfOnWayWall / sum;

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < z; j++)
            {
                GameObject newObj = null;
                float rnd = Random.Range(0, 1.0f);
                // Debug.Log(rnd +"___"+ probEmptyWall);
                if (rnd < probEmptyWall)
                {
                    map[i, j] = 0;
                    newObj = instanceInMap(EmptyWall, i, j);
                    emptyList.Add(new[] {i, j});
                }
                else if (rnd < probEmptyWall + probNormallWall)
                {
                    map[i, j] = 1;
                    newObj = instanceInMap(normallWall, i, j);
                }
                else if (rnd < probEmptyWall + probNormallWall + probOneWayWall)
                {
                    map[i, j] = 2;
                    newObj = instanceInMap(oneWayWall, i, j);
                }

                newObj.GetComponent<Walls>().setterXZ(i, j);
            }
        }

        choosePlayerStartPointAndGoals(numberOfGoals);
    }

    private void choosePlayerStartPointAndGoals(int goalNumbers)
    {
        List<int[]> tempEmptyList = new List<int[]>(emptyList);
        int rnd = Random.Range(0, tempEmptyList.Count);
        int[] start = tempEmptyList[rnd];
        tempEmptyList.RemoveAt(rnd);
        GameObject _playerObj = instanceInMap(player, start[0], start[1]);
        _playerObj.GetComponent<Player>()
            .setPos((Vector3) calculatePosInMap(start[0], start[1], _playerObj.transform) + Vector3.up, start, true);

        for (int i = 0; i < goalNumbers && tempEmptyList.Count > 1; i++)
        {
            //handle goals setup
            rnd = Random.Range(0, tempEmptyList.Count);
            // Debug.Log(tempEmptyList.Count);
            int[] goalIndex = tempEmptyList[rnd];
            GameObject newWall = instanceInMap(_goal, goalIndex[0], goalIndex[1], 0.5f);
            newWall.GetComponent<Walls>()?.setterXZ(goalIndex[0], goalIndex[1]);
            changeMap(goalIndex[0], goalIndex[1], 3); //convert empty space to goal point
            tempEmptyList.RemoveAt(rnd);
        }
    }

    public GameObject instanceInMap(GameObject obj, int i, int j, float Y_offset = 0f)
    {
        GameObject result = Instantiate(obj,
            transform.position + new Vector3(i * blockSizeOfMap - x * blockSizeOfMap / 2 + blockSizeOfMap / 2,
                obj.transform.localScale.y / 2 + Y_offset,
                j * blockSizeOfMap - z * blockSizeOfMap / 2 + blockSizeOfMap / 2), Quaternion.identity,
            parentObj.transform);
        return result;
    }

    public int[] getPlayerPos()
    {
        return _playerPos;
    }

    //this function dont care about size of your object you must add it to position after you get result
    public Vector3 calculatePosInMap(int i, int j)
    {
        float y = 1.5f;
        return new Vector3(i - x / 2, y, j - z / 2);
    }

    //this function must be called before each movement 
    public Vector3? calculatePosInMap(int i, int j, Transform obj)
    {
        if (i >= x || j >= z || i < 0 || j < 0)
        {
            Debug.Log("outOfBand");
            return null;
        }

        // Debug.Log(map[i,j] +"***" + i +"-" + j);
        if (map[i, j] == 1) //it must be empty wall or one way wall
        {
            Debug.Log("wall");
            return null;
        }

        return new Vector3(i * blockSizeOfMap - x * blockSizeOfMap / 2 + blockSizeOfMap / 2,
            0.15f + obj.transform.localScale.y / 2,
            j * blockSizeOfMap - z * blockSizeOfMap / 2 + blockSizeOfMap / 2);
    }

    public void changeMap(int oldX, int oldY, int result)
    {
        this.map[oldX, oldY] = result;
        Debug.Log(this.map[oldX, oldY]);
        switch (result)
        {
            case 0:
                instanceInMap(EmptyWall, oldX, oldY);
                break;
            case 1:
                instanceInMap(normallWall, oldX, oldY);
                break;
            case 2:
                instanceInMap(oneWayWall, oldX, oldY);
                break;
            case 3:
                instanceInMap(_goal, oldX, oldY);
                break;

        }
    }
}