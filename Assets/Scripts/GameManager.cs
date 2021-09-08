using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class GameManager : Singleton<GameManager>
{
    // public bool learningMode = true;
    [SerializeField] private GameObject mapObj;
    [SerializeField] private List<Map> _maps;
    [SerializeField] private ManagerState _managerState;
    [SerializeField] private string saveFileName;
    [SerializeField] private string saveResultFileName;
    [SerializeField] private GameObject playerGameObject;
    public ListOfMapsStruct ListOfMapsStruct = new ListOfMapsStruct();
    [SerializeField] private bool randomPosReset = true;
    [SerializeField] private bool randomPosTarget = true;
    [SerializeField] private int numberOfrepeat = 10;
    private static int maxId = 1;
    [SerializeField] private float mapDistance = 50;
    
    public int removeId;

    public int getNumberOfRepeat()
    {
        return numberOfrepeat;
    }

    public ManagerState getManagerState()
    {
        return _managerState;
    }
    
    public GameObject getPlayerGameObject()
    {
        return playerGameObject;
    }
    public bool isRandomStart()
    {
        return randomPosReset;
    }

    public bool isRandomTarget()
    {
        return randomPosTarget;
    }
    
    void Awake()
    {
        try
        {
            // load
            ListOfMapsStruct = loadMapsStructs();
            // Debug.Log("here : " + ListOfMapsStruct._structsMap.Count );
        }
        catch (Exception e)
        {
            Debug.Log("load directory not found");
        }
        
        
        //find maxid that has been saved 
        for (int i = 0; i < ListOfMapsStruct._structsMap.Count; i++)
        {
            MapDataStruct mapDataStruct = ListOfMapsStruct._structsMap[i];
            if (mapDataStruct.id >= maxId)
            {
                maxId = mapDataStruct.id + 1;
            }
        }
        
        // this line clear defualt array list with null values when we use save file we must clean the list
        _maps = new List<Map>();
        
        if (_managerState == ManagerState.trainFromFile)
        {
            for (int i = 0; i < ListOfMapsStruct._structsMap.Count; i++)
            {
                int z = i % 8;
                MapDataStruct mapDataStruct = ListOfMapsStruct._structsMap[i];
                GameObject newObj = Instantiate(mapObj, new Vector3(i / 8 * mapDistance, 0, z * mapDistance), quaternion.identity);
                Map newMap = newObj.GetComponent<Map>();
                _maps.Add(newMap);
                newMap.setupMapByStruct(mapDataStruct);
            }    
        }
        else if(_managerState == ManagerState.customMap)
        {
            GameObject[] secenMaps = GameObject.FindGameObjectsWithTag("map");
            foreach (GameObject tempMap in secenMaps)
            {
                _maps.Add(tempMap.GetComponent<Map>());
                Debug.Log("i find the map");
            }
        }else if (_managerState == ManagerState.testFromFile)
        {
            //test manager Setup
            //manually u must setup map and agent for this
            if (ListOfMapsStruct._structsMap.Count !=0)
            {
                //make first map
                makeMapByStruct(ListOfMapsStruct._structsMap[0],Vector3.zero);
            }
            
        }else if (_managerState == ManagerState.heuristicTraining)
        {
            //heuristic training part
            if (ListOfMapsStruct._structsMap.Count !=0)
            {
                //make first map
                makeMapByStruct(ListOfMapsStruct._structsMap[0],Vector3.zero);
            }
        }
        
    }

    public void removeSavedId(int id)
    {
        for (int i = 0; i < ListOfMapsStruct._structsMap.Count; i++)
        {
            if (ListOfMapsStruct._structsMap[i].id == id)
            {
                ListOfMapsStruct._structsMap.RemoveAt(i);
                break;
            }
        }
        
        string savedString = ListOfMapsStruct.ToJson();
        Debug.Log(savedString);
        FileManager.WriteToFile("saveFiles//"+saveFileName, savedString);
        
    }

    //for heuristic levels
    public void loadNextMap(GameObject gameObject , int id)
    {
        //destroy current level and load next level
        Destroy(gameObject);
        int result = 0;
        bool isFind = false;
        //if i is last item there is no more item for checking
        for (int i = 0; i < ListOfMapsStruct._structsMap.Count - 1; i++)
        {
            if (id == ListOfMapsStruct._structsMap[i].id)
            {
                result = i + 1;
                isFind = true;
                break;

            }
        }

        if (isFind)
        {
            MapDataStruct mapDataStruct = ListOfMapsStruct._structsMap[result];
            makeMapByStruct(mapDataStruct, Vector3.zero);
        }
    }

    public MapDataStruct getNextMapStruct(int id)
    {
        int result = 0;
        bool isFind = false;
        //if i is last item there is no more item for checking
        for (int i = 0; i < ListOfMapsStruct._structsMap.Count - 1; i++)
        {
            if (id == ListOfMapsStruct._structsMap[i].id)
            {
                result = i + 1;
                isFind = true;
                break;

            }
        }
        if (isFind)
        {
            MapDataStruct mapDataStruct = ListOfMapsStruct._structsMap[result];
            return mapDataStruct;
        }

        return default(MapDataStruct);
    }

    private void makeMapByStruct(MapDataStruct mapDataStruct , Vector3 pos)
    {
        GameObject newObj = Instantiate(mapObj, pos, quaternion.identity);
        Map newMap = newObj.GetComponent<Map>();
        _maps.Add(newMap);
        newMap.setupMapByStruct(mapDataStruct);
    }
    
    public void saveMap(int id)
    {
        //first remove last data in saved system
        for (int i = 0; i < ListOfMapsStruct._structsMap.Count; i++)
        {
            if (ListOfMapsStruct._structsMap[i].id == id)
            {
                ListOfMapsStruct._structsMap.RemoveAt(i);
                break;
            }
        }
        // extract data from map
        for (int i = 0; i < _maps.Count; i++)
        {
            if (_maps[i].id == id)
            {
                MapDataStruct mapDataStruct = _maps[i].GetMapDataStruct();
                ListOfMapsStruct._structsMap.Add(mapDataStruct);
                // Debug.Log("find the id ");
            }
            
        }
        
        //save entire accepted maps in the directory
        string savedString = ListOfMapsStruct.ToJson();
        Debug.Log(savedString);
        FileManager.WriteToFile("saveFiles//"+saveFileName, savedString);
    }

    private ListOfMapsStruct loadMapsStructs()
    {
        string res;
        ListOfMapsStruct loadedStructs = new ListOfMapsStruct();
        FileManager.LoadFromFile("saveFiles//"+saveFileName, out res);
        Debug.Log(res);
        loadedStructs.LoadFromJson(res);
        // Debug.Log(loadedStructs._structsMap[0].blocksStates.Count);
        return loadedStructs;
    }

    public static void checkMaxId(int id)
    {
        if (GameManager.maxId <= id)
        {
            GameManager.maxId = id + 1;
        }
    }

    public static int getId()
    {
        int res = GameManager.maxId;
        GameManager.maxId++;
        return res;
    }
    
    public void writetoFileSolverStruct(TestResultSolver testResultSolver)
    {
        string added = "";
        added += testResultSolver.id;
        added += ",";
        added += testResultSolver.numberOfrepeat;
        added += ",";
        added += testResultSolver.stepNumber;
        added += ",";
        added += testResultSolver.avgOfpoints;
        added += ",";
        added += testResultSolver.maxOfGoalReach;
        added += "\n";
        
        
        string current;
        FileManager.LoadFromFile("saveFiles//"+saveResultFileName, out current);

        FileManager.WriteToFile("saveFiles//"+saveResultFileName, current + added);

    }
}

//this class just made for serializing with library 
[System.Serializable]
public class ListOfMapsStruct
{
    public List<MapDataStruct> _structsMap = new List<MapDataStruct>();
    
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void LoadFromJson(string a_Json)
    {
        JsonUtility.FromJsonOverwrite(a_Json, this);
    }
}

[System.Serializable]
public struct MapDataStruct
{
    public int id;
    public int XSize;
    public int ZSize;
    public int bombNumber;
    public int ultimateNumber;
    public float blockSize;
    public List<int> blocksStates;
    public List<int> playerPos;
}

public enum ManagerState
{
    testFromFile,
    trainFromFile,
    customMap,
    heuristicTraining
}

public struct TestResultSolver
{
    public int id;
    public int numberOfrepeat;
    public float stepNumber;
    public float avgOfpoints;
    public int maxOfGoalReach; // maximum number of that available
}