using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
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
    [SerializeField] private int maxUltimate=3;
    [SerializeField] private int maxBoomb=5;
    [SerializeField] private GameObject _goal; //3
    [SerializeField] private GameObject checkpointObj; //this object use for notify the object reached to specific point
    [SerializeField] private GameObject parentObj;
    
    


    [SerializeField] private bool isRandomMap = true;

    private int XSize, zSize;
    private int[] playerStartPos = null;
    private List<int[]> emptyList;
    private Vector3 originPivot;
    private MapDataStruct intialDataStruct;
    private Transform myPlayerTransform;
    private bool isFirstTime = true;
    private TestResultSolver _testResultSolver = new TestResultSolver();

    [SerializeField] private Obstacle[,] map;
    [SerializeField] private bool loadedMap = false;
    [SerializeField] private int repeatNumber = -1;
    public void Awake()
    {
        originPivot = transform.position;
        emptyList = new List<int[]>();
        // Debug.Log("debuged");
        XSize = (int) ((int) this.transform.localScale.x / blockSizeOfMap);
        zSize = (int) ((int) this.transform.localScale.z / blockSizeOfMap);
        map = new Obstacle[XSize, zSize];
        createCheckpoints();
    }

    //this function destroy  walls and goals and palyers
    private void destroyElements()
    {
        Transform[] childs = parentObj.GetComponentsInChildren<Transform>();
        for (int i = 1; i < childs.Length; i++)
        {
            Destroy(childs[i].gameObject);
        }
    }

    
    //this function dont destroy the user
    private void destroyAllObstacles()
    {
        Obstacle[] childs = parentObj.GetComponentsInChildren<Obstacle>();
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
            PlayerParent customPlayerParent = parentObj.GetComponentInChildren<PlayerParent>();
            myPlayerTransform = customPlayerParent.getTransform();
            int[] start = findNearestPoint(customPlayerParent.getTransform());
            customPlayerParent.setPos(safeCalculatePosInPMap(start[0], start[1], customPlayerParent.getTransform()),
                start, map: this,
                fast: true);
            playerStartPos = start;
        }
        
        
    }

    private List<GameObject> checkpointList = new List<GameObject>();
    private void createCheckpoints()
    {
        // create checkpoint object
        if (checkpointObj != null)
        {
            for (int i = 0; i < XSize; i++)
            {
                for (int j = 0; j < zSize; j++)
                {
                    Vector3 pos = safeCalculatePosInPMap(i, j, checkpointObj.transform);
                    GameObject newObj = Instantiate(checkpointObj, pos, quaternion.identity);
                    checkpointList.Add(newObj);
                    newObj.transform.parent = parentObj.transform;
                }
            }
        }
    }
    
    // private void refreshCheckpoints()
    // {
    //     //activate all checkpoint that is made in create checkpoint function
    //     for (int i = 0; i < checkpointList.Count; i++)
    //     {
    //         checkpointList[i].SetActive(true);
    //     }
    // }

    private IEnumerator refreshCheckpoints()
    {
        //activate all checkpoint that is made in create checkpoint function
        yield return null;
        for (int i = 0; i < checkpointList.Count; i++)
        {
            checkpointList[i].SetActive(true);
        }

        yield return null;
    }

    public bool checkAllCheckpointsCatched(int checkPoint)
    {

        if ( checkPoint == checkpointList.Count )
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public List<GameObject> getCheckPointsState()
    {
        return checkpointList;
    }
    

    private void makeRandomMap(bool savePlayer = false)
    {
        // random map
        if (savePlayer)
        {
            //just walls and goals
            destroyAllObstacles();
        }
        else
        {
            //destroy walls goals and player
            destroyElements();
        }

        float sum = chanceOfEmptyWall + chanceOfNormallWall + chanceOfOnWayWall;
        float probEmptyWall = chanceOfEmptyWall / sum;
        float probNormallWall = chanceOfNormallWall / sum;
        float probOneWayWall = chanceOfOnWayWall / sum;

        emptyList = new List<int[]>();
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
        choosePlayerStartPointAndGoals(numberOfGoals , savePlayer);
    }

    //just for test 
    public int checkCell(int x, int z)
    {
        if (map[x, z] is Goal)
        {
            return 3;
        }
        else if (map[x, z] is NormalWall)
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
                if (map[i, j] is Empty)
                {
                    emptyList.Add(new int[] {i, j});
                }
            }

        }
    }

    //this function must called when instance map from save system
    public void setupMapByStruct(MapDataStruct structData)
    {
        //set up struct test
        _testResultSolver.id = structData.id;
        _testResultSolver.numberOfrepeat = GameManager.Instance.getNumberOfRepeat();
        _testResultSolver.avgOfpoints = 0;
        _testResultSolver.avgStepNumber = 0;
        _testResultSolver.maxOfGoalReach = 0;
        // _testResultSolver.checkPointIsActive = clearCheckPointIsActive();
        


        //

        numberOfGoals = 0;
        if (isFirstTime)
        {
            repeatNumber = -1;
            isFirstTime = false;
        }
        else
        {
            repeatNumber = 0;    
        }
        
        intialDataStruct = structData;
        GameManager.checkMaxId(structData.id);
        this.id = structData.id;
        XSize = structData.XSize;
        zSize = structData.ZSize;
        blockSizeOfMap = structData.blockSize;
        transform.localScale = new Vector3(XSize * blockSizeOfMap, 0.3f, zSize * blockSizeOfMap);


        

        //make the player
        int[] start = structData.playerPos.ToArray();
        GameObject _playerObj = instanceInMap(GameManager.Instance.getPlayerGameObject(), start[0], start[1]);
        myPlayerTransform = _playerObj.transform;
        playerStartPos = start;
        PlayerParent tempPlayerParent = _playerObj.GetComponent<PlayerParent>();
        // _playerObj.transform.parent = null; //player is not child of wall in inspector of engine 
        tempPlayerParent
            .setPos((Vector3) calculatePosInMap(start[0], start[1], _playerObj.transform) + Vector3.up, start, true,
                this);
        tempPlayerParent.setUltimateAndBombNumber(structData.ultimateNumber, structData.bombNumber);
        
        
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
    }

    public void remakeMapByStruct(MapDataStruct structData , Transform myPlayerTransform)
    {
        _testResultSolver.id = structData.id;
        _testResultSolver.numberOfrepeat = GameManager.Instance.getNumberOfRepeat();
        _testResultSolver.avgOfpoints = 0;
        _testResultSolver.avgStepNumber = 0;
        _testResultSolver.maxOfGoalReach = 0;
        _testResultSolver.checkPointIsActive = clearCheckPointIsActive();
        


        //

        numberOfGoals = 0;
        if (isFirstTime)
        {
            repeatNumber = -1;
            isFirstTime = false;
        }
        else
        {
            repeatNumber = 0;    
        }
        intialDataStruct = structData;
        this.id = structData.id;
        XSize = structData.XSize;
        zSize = structData.ZSize;
        blockSizeOfMap = structData.blockSize;
        transform.localScale = new Vector3(XSize * blockSizeOfMap, 0.3f, zSize * blockSizeOfMap);

        updateEmptyList();
        //make all blocks
        StartCoroutine(makeWallsAndGoalsByDelay(structData));
        // for (int i = 0; i < structData.blocksStates.Count; i++)
        // {
        //     int x = i / XSize;
        //     int z = i % XSize;
        //     changeMap(x, z, structData.blocksStates[i]);
        //     if (structData.blocksStates[i] == 3)
        //     {
        //         numberOfGoals++;
        //     }
        // }

        //find all users
        int[] start = structData.playerPos.ToArray();
        this.myPlayerTransform = myPlayerTransform;
        playerStartPos = start;
        PlayerParent tempPlayerParent = myPlayerTransform.GetComponent<PlayerParent>();
        // _playerObj.transform.parent = null; //player is not child of wall in inspector of engine 
        tempPlayerParent
            .setPos((Vector3) calculatePosInMap(start[0], start[1], myPlayerTransform.transform) + Vector3.up, start, true,
                this);
        tempPlayerParent.setUltimateAndBombNumber(structData.ultimateNumber, structData.bombNumber);
    }

    private IEnumerator makeWallsAndGoalsByDelay(MapDataStruct structData)
    {
        yield return null;
        numberOfGoals = 0;
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
    }
    
    
    //reset just use for maps that loaded from save file 
    //if return false its not reset. this map is removed and replaced by another map
    //true show normal reset for class
    public bool resetMap(int points = -1 , int steps = -1, List<int> checkPoints = null)
    {
        if (isRandomMap)
        {
            Debug.Log("i destroy all things");
            makeRandomMap(true);
            StartCoroutine(refreshCheckpoints()) ; // reset all checkpoints;
            return false;
        }
        
        int[] lastPos = myPlayerTransform.GetComponent<PlayerParent>().getPosIndex();
        if (GameManager.Instance.getManagerState() == ManagerState.heuristicTraining)
        {
            Debug.Log("repeatNumber : " + repeatNumber);
            updateSolverStruct(points , steps );
            if (repeatNumber + 1 >= GameManager.Instance.getNumberOfRepeat())
            {
                GameManager.Instance.writetoFileSolverStruct(_testResultSolver);
                // GameManager.Instance.loadNextMap(this.gameObject, this.id);
                MapDataStruct temp = GameManager.Instance.getNextMapStruct(this.id);
                remakeMapByStruct(temp,myPlayerTransform);
                return false;
            }
        }
        
        if (GameManager.Instance.getManagerState() == ManagerState.testFromFile)
        {
            // CheckArray(checkPoints);
            updateSolverStruct(points , steps,checkPoints);
            if (repeatNumber + 1 >= GameManager.Instance.getNumberOfRepeat())
            {
                GameManager.Instance.writetoFileSolverStruct(_testResultSolver);
                MapDataStruct temp = GameManager.Instance.getNextMapStruct(this.id);
                remakeMapByStruct(temp,myPlayerTransform);
                StartCoroutine(refreshCheckpoints()) ; // reset all checkpoints
                // _testResultSolver.checkPointIsActive = clearCheckPointIsActive();
                return false;
            }
        }

        repeatNumber++;
        
        

        Collider collider = myPlayerTransform.GetComponent<CapsuleCollider>();
        collider.enabled = false;


        if (intialDataStruct.Equals(default(MapDataStruct)))
        {
            Debug.Log("this map cant be reset it is custom map you must save it :)");
            collider.enabled = true;
            return true;
        }
        

        int[] start = intialDataStruct.playerPos.ToArray();


        PlayerParent playerParent = myPlayerTransform.GetComponent<PlayerParent>();
        playerParent.setUltimateAndBombNumber(intialDataStruct.ultimateNumber, intialDataStruct.bombNumber);
        playerParent.setPos((Vector3) calculatePosInMap(start[0], start[1], playerParent.getTransform()) + Vector3.up,
            start, true,
            this);


        StartCoroutine(addWalls(collider));

        // collider.enabled = true;

        //this part help to have better random start pos for player
        //better learn and better resualts
        if (GameManager.Instance.isRandomStart())
        {
            updateEmptyList();
            int[] newPos = emptyList[Random.Range(0, emptyList.Count)];
            playerParent.setPos(
                (Vector3) calculatePosInMap(newPos[0], newPos[1], playerParent.getTransform()) + Vector3.up, newPos,
                true,
                this);

        }

        if (GameManager.Instance.isRandomTarget())
        {

            StartCoroutine(makeRandomGoals(collider, lastPos));
        }

        StartCoroutine(refreshCheckpoints()); // reset all checkpoints;

        return true;
    }

    public static void CheckArray(List<int> ls)
    {
        String s = "";
        foreach (var VARIABLE in ls)
        {
            s += VARIABLE;
        }
        Debug.Log(s);
    }
    
    private IEnumerator makeRandomGoals(Collider collider,int[] lastPlayerPos)
    {
        yield return null;
        
        //clean the goals
        for (int i = 0; i < XSize; i++)
        {
            for (int j = 0; j < zSize; j++)
            {
                if (map[i, j] is Goal)
                {
                    changeMap(i, j, 0);
                }
            }
        }

        updateEmptyList();
        collider.enabled = false;
        for (int i = 0; i < numberOfGoals; i++)
        {
            int[] newPos = emptyList[Random.Range(0, emptyList.Count)];
            PlayerParent playerParent = myPlayerTransform.GetComponent<PlayerParent>();
            int[] playerPos = playerParent.getPosIndex();
            if ((newPos[0] == playerPos[0] && newPos[1] == playerPos[1]) || ! (map[newPos[0], newPos[1]] is Empty) || 
                (newPos[0] == lastPlayerPos[0] && newPos[1] == lastPlayerPos[1]))
            {
                //if colided with player try again
                i--;
                continue;
            }

            changeMap(newPos[0], newPos[1], 3);
        }

        yield return null;
        collider.enabled = true;
    }


    public IEnumerator addWalls(Collider collider)
    {
        yield return null;
        for (int i = 0; i < intialDataStruct.blocksStates.Count; i++)
        {
            int x = i / XSize;
            int z = i % XSize;
            changeMap(x, z, intialDataStruct.blocksStates[i]);

        }

        collider.enabled = true;
    }


    private void choosePlayerStartPointAndGoals(int goalNumbers , bool savePlayer)
    {
        List<int[]> tempEmptyList = new List<int[]>(emptyList);
        int rnd = Random.Range(0, tempEmptyList.Count);
        int[] start = tempEmptyList[rnd];
        tempEmptyList.RemoveAt(rnd);
        playerStartPos = start;
        Debug.Log("started at" + playerStartPos[0] + " -- " + playerStartPos[1]);
        if (savePlayer)
        {
            Debug.Log( "id of player" +myPlayerTransform.gameObject.GetInstanceID());
            PlayerParent tempPlayerParent = this.GetComponentInChildren<PlayerParent>();
            // _playerObj.transform.parent = null; //player is not child of wall in inspector of engine 
            tempPlayerParent
                .setPos((Vector3) calculatePosInMap(start[0], start[1], myPlayerTransform.transform) + Vector3.up, start, true,
                    this);
            tempPlayerParent.setUltimateAndBombNumber(Random.Range(0,maxUltimate),Random.Range(0,maxBoomb) );
        }
        else
        {
            GameObject _playerObj = instanceInMap(GameManager.Instance.getPlayerGameObject(), start[0], start[1]);
            myPlayerTransform = _playerObj.transform;
            // _playerObj.transform.parent = null; //player is not child of wall in inspector of engine 
            _playerObj.GetComponent<PlayerParent>()
                .setPos((Vector3) calculatePosInMap(start[0], start[1], _playerObj.transform) + Vector3.up, start, true,
                    this);
        }
        
        

        

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

    private GameObject instanceInMap(GameObject obj, int i, int j, float Y_offset = 0f)
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
                // Debug.Log("goal created");
                newObj = instanceInMap(_goal, oldX, oldY, 0.5f);
                newObj.GetComponent<Obstacle>().setterXZ(oldX, oldY, this);
                // Debug.Log(newObj.transform.position);
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
        myPlayerTransform.GetComponent<PlayerParent>()
            .getUltimateAndBombNumber(ref res.ultimateNumber, ref res.bombNumber);
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

    public void updateSolverStruct(int point,int stepNumbers,List<int> checkPoints = null)
    {
        if (point == -1 || stepNumbers < 3)
        {
            Debug.Log("some thing wrong i got -1");
            return;
        }
        float avgPlus = (float)point / GameManager.Instance.getNumberOfRepeat();
        float avgStepPlus = (float)stepNumbers / GameManager.Instance.getNumberOfRepeat();
        if (_testResultSolver.maxOfGoalReach < point)
        {
            _testResultSolver.maxOfGoalReach = point;
        }

        _testResultSolver.avgOfpoints += avgPlus;
        _testResultSolver.avgStepNumber += avgStepPlus;
        if (checkPoints != null)
        {
            if (_testResultSolver.checkPointIsActive == null)
            {
                Debug.Log("yes side");
                _testResultSolver.checkPointIsActive = checkPoints;
            }
            else
            {
                Debug.Log("noSide");
                for (int i = 0; i < _testResultSolver.checkPointIsActive.Count; i++)
                {
                    if (checkPoints[i] == 0 && _testResultSolver.checkPointIsActive[i] ==1)
                    {
                        _testResultSolver.checkPointIsActive[i] = 0;
                    }
                }
            }
        }
    }

    private List<int> clearCheckPointIsActive()
    {
        List<int> cleanCheckpoints = new List<int>();
        for (int i = 0; i < checkpointList.Count; i++)
        {
            cleanCheckpoints.Add(1);
        }

        return cleanCheckpoints;
    }

    
    //based on number of repeat
    public bool canReset()
    {
        if (repeatNumber + 1 >= GameManager.Instance.getNumberOfRepeat() && 
                (GameManager.Instance.getManagerState() == ManagerState.heuristicTraining
                 ||GameManager.Instance.getManagerState() == ManagerState.testFromFile ))
        {
            return false;
        }

        return true;
    }
    

}