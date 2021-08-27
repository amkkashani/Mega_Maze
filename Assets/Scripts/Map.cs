using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[Serializable]
public class Map : MonoBehaviour
{
    //never change id in inspector
    public int
        id = 0; //when maps request to say if have already a id use that and if dont have id it will get id from Game manager 

    //zero id means id is not define yet
    [SerializeField] private float blockSizeOfMap = 1.0f;
    [SerializeField] private float chanceOfEmptyWall; //0
    [SerializeField] private GameObject EmptyWall;
    [SerializeField] private float chanceOfNormallWall; //1
    [SerializeField] private GameObject normallWall;
    [SerializeField] private float chanceOfOnWayWall; //2
    [SerializeField] private GameObject oneWayWall;
    [SerializeField] private int numberOfGoals;
    [SerializeField] private GameObject _goal; //3

    [SerializeField] private GameObject parentObj;

    [SerializeField] private GameObject playerGameObject;

    [SerializeField] private bool isRandomMap = true;

    private int XSize, zSize;
    private int[] playerStartPos = null;
    private List<int[]> emptyList;
    private Vector3 originPivot;
    private MapDataStruct intiDataStruct;
    private Transform myPlayeTransform;

    [SerializeField] private Obstacle[,] map;
    [SerializeField] private bool loadedMap = false;

    public void Awake()
    {
        originPivot = transform.position;
        emptyList = new List<int[]>();
        // Debug.Log("debuged");
        XSize = (int) ((int) this.transform.localScale.x / blockSizeOfMap);
        zSize = (int) ((int) this.transform.localScale.z / blockSizeOfMap);
        map = new Obstacle[XSize, zSize];
    }

    //this function destroy  walls and goals and palyers
    public void destroyElements()
    {
        Transform[] childs = parentObj.GetComponentsInChildren<Transform>();
        for (int i = 1; i < childs.Length; i++)
        {
            Destroy(childs[i].gameObject);
        }
    }

    public void Start()
    {
        if (loadedMap)
        {
            
            return;
        }

        if (isRandomMap)
        {
            makeRandomMap();
        }
        else
        {
            //custom map
            if (XSize == 0 || zSize == 0)
            {
                Debug.LogError(" one of dimensions in map is zero");
                return;
            }

            Obstacle[] obstacles = parentObj.GetComponentsInChildren<Obstacle>();
            for (int i = 0; i < obstacles.Length; i++)
            {
                int[] res = findNearestPoint(obstacles[i].transform);
                Transform obj = obstacles[i].transform;
                int best_X = res[0];
                int best_Z = res[1];

                obj.position = safeCalculatePosInPMap(best_X, best_Z, obj);
                obstacles[i].setterXZ(res[0], res[1], this);
                if (map[best_X, best_Z] == null)
                {
                    // if the cell was empty , can add obstacle to the cell and 
                    //      and if not the cell will remove from the game map
                    map[best_X, best_Z] = obstacles[i];
                }
                else
                {
                    Destroy(obstacles[i].gameObject);
                }
            }

            //make empty object in null places
            for (int i = 0; i < XSize; i++)
            {
                for (int j = 0; j < zSize; j++)
                {
                    if (map[i, j] == null)
                    {
                        GameObject newObj = instanceInMap(EmptyWall, i, j);
                        map[i, j] = newObj.GetComponent<Obstacle>();
                        newObj.GetComponent<Obstacle>().setterXZ(i, j, this);
                    }
                }
            }

            //find the player
            Player customPlayer = parentObj.GetComponentInChildren<Player>();
            myPlayeTransform = customPlayer.transform;
            int[] start = findNearestPoint(customPlayer.transform);
            customPlayer.setPos(safeCalculatePosInPMap(start[0], start[1], customPlayer.transform), start, map: this,
                fast: true);
            playerStartPos = start;
        }
        // GameManager.Instance.saveMap(1); //todo must remove 
        // GameManager.Instance.saveMap(2);
    }

    private void makeRandomMap()
    {
        // random map
        float sum = chanceOfEmptyWall + chanceOfNormallWall + chanceOfOnWayWall;
        float probEmptyWall = chanceOfEmptyWall / sum;
        float probNormallWall = chanceOfNormallWall / sum;
        float probOneWayWall = chanceOfOnWayWall / sum;

        for (int i = 0; i < XSize; i++)
        {
            for (int j = 0; j < zSize; j++)
            {
                GameObject newObj = null;
                float rnd = Random.Range(0, 1.0f);
                // Debug.Log(rnd +"___"+ probEmptyWall);
                if (rnd < probEmptyWall)
                {
                    newObj = instanceInMap(EmptyWall, i, j);
                    map[i, j] = newObj.GetComponent<Obstacle>();
                    emptyList.Add(new[] {i, j});
                }
                else if (rnd < probEmptyWall + probNormallWall)
                {
                    newObj = instanceInMap(normallWall, i, j);
                    map[i, j] = newObj.GetComponent<Obstacle>();
                }
                else if (rnd < probEmptyWall + probNormallWall + probOneWayWall)
                {
                    newObj = instanceInMap(oneWayWall, i, j);
                    map[i, j] = newObj.GetComponent<Obstacle>();
                }

                newObj.GetComponent<Obstacle>().setterXZ(i, j, this);
            }
        }
        choosePlayerStartPointAndGoals(numberOfGoals);
    }

    //just for test 
    public int checkCell(int x , int z)
    {
        if (map[x,z] is Goal )
        {
            return 3;
        }else if (map[x, z] is NormalWall)
        {
            return 1;
        }

        return 0;

    }

    //entirly check map array and find zero values with index
    //when we need have empty list
    //this function must use in loaded map with different spawn player position
    private void updateEmptyList()
    {
        emptyList = new List<int[]>();
        for (int i = 0; i < XSize; i++)
        {
            for (int j = 0; j < zSize; j++)
            {
                if (map[i,j] is Empty)
                {
                    emptyList.Add(new int[]{i,j});
                }
            }
            
        }
    }
    //this function must called when instance map from save system
    public void setupMapByStruct(MapDataStruct structData)
    {
        numberOfGoals = 0;
        intiDataStruct = structData;
        GameManager.checkMaxId(structData.id);
        this.id = structData.id;
        XSize = structData.XSize;
        zSize = structData.ZSize;
        blockSizeOfMap = structData.blockSize;
        transform.localScale = new Vector3(XSize * blockSizeOfMap, 0.3f, zSize * blockSizeOfMap);


        //make all blocks
        for (int i = 0; i < structData.blocksStates.Count; i++)
        {
            int x = i / XSize;
            int z = i % XSize;
            changeMap(x, z, structData.blocksStates[i]);
            if (structData.blocksStates[i] == 3)
            {
                numberOfGoals++;
            }
        }

        //make the player
        int[] start = structData.playerPos.ToArray();
        GameObject _playerObj = instanceInMap(playerGameObject, start[0], start[1]);
        myPlayeTransform = _playerObj.transform;
        playerStartPos = start;
        Player tempPlayer = _playerObj.GetComponent<Player>();
        // _playerObj.transform.parent = null; //player is not child of wall in inspector of engine 
        tempPlayer
            .setPos((Vector3) calculatePosInMap(start[0], start[1], _playerObj.transform) + Vector3.up, start, true,
                this);
        tempPlayer.setUltimateAndBombNumber(structData.ultimateNumber, structData.bombNumber);
    }

    //reset just use for maps that loaded from save file 
    public void resetMap()
    {
        if (isRandomMap)
        {
            destroyElements();
            makeRandomMap();
            return;
        }
        Collider collider = myPlayeTransform.GetComponent<CapsuleCollider>();
        collider.enabled = false;
        
        
        if (intiDataStruct.Equals(default(MapDataStruct)))
        {
            Debug.Log("this map cant be reset it is custom map you must save it :)");
            collider.enabled = true;
            return;
        }

        
        int[] start = intiDataStruct.playerPos.ToArray();
        
        
        Player _player = myPlayeTransform.GetComponent<Player>();
        _player.setUltimateAndBombNumber(intiDataStruct.ultimateNumber, intiDataStruct.bombNumber);
        _player.setPos((Vector3) calculatePosInMap(start[0], start[1], _player.transform) + Vector3.up, start, true,
            this);


        StartCoroutine(addWalls());

        collider.enabled = true;
        
        //this part help to have better random start pos for player
        //better learn and better resualts
        if (GameManager.Instance.isRandomStart())
        {
            updateEmptyList();
            int[] newPos = emptyList[Random.Range(0, emptyList.Count)];
            _player.GetComponent<Player>()
                .setPos((Vector3) calculatePosInMap(newPos[0], newPos[1], _player.transform) + Vector3.up, newPos, true,
                    this);

        }

        if (GameManager.Instance.isRandomTarget())
        {

            StartCoroutine(makeRandomGoals());
        }
    }

    private IEnumerator makeRandomGoals()
    {
        yield return null;
        yield return null;
        
        //clean the goals
        for (int i = 0; i < XSize; i++)
        {
            for (int j = 0; j < zSize; j++)
            {
                if (map[i,j] is Goal)
                {
                    changeMap(i,j,0);
                }
            }
        }
        updateEmptyList();
        for (int i = 0; i < numberOfGoals; i++)
        {
            int[] newPos = emptyList[Random.Range(0, emptyList.Count)];
            Player player = myPlayeTransform.GetComponent<Player>();
            int[] playerPos = player.getPosIndex();
            if (newPos[0] == playerPos[0] && newPos[1] == playerPos[1])
            {
                //if colided with player try again
                i--;
                continue;
            }
            changeMap(newPos[0],newPos[1],3);
        }
    }
    

    public IEnumerator addWalls()
    {
        yield return null;
        for (int i = 0; i < intiDataStruct.blocksStates.Count; i++)
        {
            int x = i / XSize;
            int z = i % XSize;
            changeMap(x, z, intiDataStruct.blocksStates[i]);
            
        }
    }


    private void choosePlayerStartPointAndGoals(int goalNumbers)
    {
        List<int[]> tempEmptyList = new List<int[]>(emptyList);
        int rnd = Random.Range(0, tempEmptyList.Count);
        int[] start = tempEmptyList[rnd];
        tempEmptyList.RemoveAt(rnd);
        GameObject _playerObj = instanceInMap(playerGameObject, start[0], start[1]);
        myPlayeTransform = _playerObj.transform;
        playerStartPos = start;
        Debug.Log("started at" + playerStartPos[0] + " -- " + playerStartPos[1]);

        // _playerObj.transform.parent = null; //player is not child of wall in inspector of engine 
        _playerObj.GetComponent<Player>()
            .setPos((Vector3) calculatePosInMap(start[0], start[1], _playerObj.transform) + Vector3.up, start, true,
                this);
        
        for (int i = 0; i < goalNumbers && tempEmptyList.Count > 1; i++)
        {
            //handle goals setup
            rnd = Random.Range(0, tempEmptyList.Count);
            int[] goalIndex = tempEmptyList[rnd];
            // GameObject newWall = instanceInMap(_goal, goalIndex[0], goalIndex[1], 0.5f); //todo good for making bugs
            // newWall.GetComponent<Obstacle>()?.setterXZ(goalIndex[0], goalIndex[1]);
            changeMap(goalIndex[0], goalIndex[1], 3); //convert empty space to goal point
            tempEmptyList.RemoveAt(rnd);
        }
    }

    public GameObject instanceInMap(GameObject obj, int i, int j, float Y_offset = 0f)
    {
        GameObject result = Instantiate(obj,
            originPivot + new Vector3(i * blockSizeOfMap - XSize * blockSizeOfMap / 2 + blockSizeOfMap / 2,
                obj.transform.localScale.y / 2 + Y_offset,
                j * blockSizeOfMap - zSize * blockSizeOfMap / 2 + blockSizeOfMap / 2), Quaternion.identity);
        result.transform.parent = parentObj.transform;
        
        return result;
    }

    //this function will remove the latest state off the cell for new cell incoming
    //if you dont want to replace you must call functions that starts with "safe" word
    public Vector3? calculatePosInMap(int i, int j, Transform obj, bool isUltimateActive = false)
    {
        if (i >= XSize || j >= zSize || i < 0 || j < 0)
        {
            // Debug.Log("outOfBand");
            return null;
        }

        // Debug.Log(map[i,j] +"***" + i +"-" + j);
        if (map[i, j] is NormalWall && !isUltimateActive) //it must be empty wall or one way wall
        {
            // Debug.Log("wall");
            return null;
        }

        if ((map[i, j] is NormalWall || map[i, j] is OneWay) && isUltimateActive)
        {
            map[i, j].wallDestruction();
        }

        return originPivot + new Vector3(i * blockSizeOfMap - XSize * blockSizeOfMap / 2 + blockSizeOfMap / 2,
            0.15f + obj.transform.localScale.y / 2,
            j * blockSizeOfMap - zSize * blockSizeOfMap / 2 + blockSizeOfMap / 2);
    }

    private Vector3 safeCalculatePosInPMap(int i, int j, Transform obj)
    {
        return originPivot + new Vector3(i * blockSizeOfMap - XSize * blockSizeOfMap / 2 + blockSizeOfMap / 2,
            0.4f + obj.transform.localScale.y / 2,
            j * blockSizeOfMap - zSize * blockSizeOfMap / 2 + blockSizeOfMap / 2);
    }

    private int[] findNearestPoint(Transform obj)
    {
        int best_X = 0;
        int best_Z = 0;
        float best_distance = Mathf.Infinity; //todo
        // find best x index
        for (int j = 0; j < XSize; j++)
        {
            float distance = Vector3.Distance(safeCalculatePosInPMap(j, 0, obj), obj.position);
            if (distance < best_distance)
            {
                best_distance = distance; //best distance is shortest distance
                best_X = j;
            }
        }

        best_distance = Mathf.Infinity;

        //find best z index
        for (int j = 0; j < zSize; j++)
        {
            float distance = Vector3.Distance(safeCalculatePosInPMap(best_X, j, obj), obj.position);
            if (distance < best_distance)
            {
                best_distance = distance; //best distance is shortest distance
                best_Z = j;
            }
        }

        return new int[] {best_X, best_Z};
    }

    public void changeMap(int oldX, int oldY, int result)
    {
        GameObject newObj = null;
        if (map[oldX, oldY] != null)
            Destroy(map[oldX, oldY].gameObject);
        // Debug.Log(result + "-- " + result.GetType());
        switch (result)
        {
            case 0:
                newObj = instanceInMap(EmptyWall, oldX, oldY); //empty walls does not need to know where are they
                break;
            case 1:
                newObj = instanceInMap(normallWall, oldX, oldY);
                newObj.GetComponent<Obstacle>().setterXZ(oldX, oldY, this);
                break;
            case 2:
                newObj = instanceInMap(oneWayWall, oldX, oldY);
                newObj.GetComponent<Obstacle>().setterXZ(oldX, oldY, this);
                break;
            case 3:
                Debug.Log("goal created");
                newObj = instanceInMap(_goal, oldX, oldY, 0.5f);
                newObj.GetComponent<Obstacle>().setterXZ(oldX, oldY, this);
                Debug.Log(newObj.transform.position);
                break;
        }

        map[oldX, oldY] = newObj.GetComponent<Obstacle>();
    }

    public int getNumberOfGoals()
    {
        return numberOfGoals;
    }

    public MapDataStruct GetMapDataStruct()
    {
        MapDataStruct res = new MapDataStruct();
        res.id = this.id;
        res.blockSize = blockSizeOfMap;
        res.XSize = this.XSize;
        res.ZSize = this.zSize;
        myPlayeTransform.GetComponent<Player>().getUltimateAndBombNumber(ref res.ultimateNumber, ref res.bombNumber);
        List<int> states = mapStatesAsInt();
        
        res.blocksStates = states;
        res.playerPos = playerStartPos.ToList();
        return res;
    }

    public List<int> mapStatesAsInt()
    {
        List<int> states = new List<int>();
        for (int i = 0; i < XSize; i++)
        {
            for (int j = 0; j < zSize; j++)
            {
                if (map[i, j] != null)
                {
                    if (map[i, j] is Empty)
                    {
                        states.Add(0);
                    }
                    else if (map[i, j] is NormalWall)
                    {
                        states.Add(1);
                    }
                    else if (map[i, j] is OneWay)
                    {
                        states.Add(2);
                    }
                    else if (map[i, j] is Goal)
                    {
                        states.Add(3);
                    }
                }
            }
        }

        return states;
    }

    public void saveMap()
    {
        if (id == 0)
        {
            //it means we need to get id from game manager
            this.id = GameManager.getId();
        }

        GameManager.Instance.saveMap(id);
    }

    public void saveAsNew()
    {
        this.id = GameManager.getId();
        GameManager.Instance.saveMap(id);
    }
}