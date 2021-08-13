using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class GameManager : Singleton<GameManager>
{
    [SerializeField] private GameObject mapObj;
    [SerializeField] private List<Map> _maps;
    [SerializeField] private bool loadFromSavedData;
    public ListOfMapsStruct ListOfMapsStruct = new ListOfMapsStruct();
    private static int maxId = 1;
    [SerializeField] private float mapDistance = 50;
    public int removeId;
    void Awake()
    {
        // load
        ListOfMapsStruct = loadMapsStructs();
        Debug.Log("here : " + ListOfMapsStruct._structsMap.Count );
        
        //find maxid that has been saved 
        for (int i = 0; i < ListOfMapsStruct._structsMap.Count; i++)
        {
            MapDataStruct mapDataStruct = ListOfMapsStruct._structsMap[i];
            if (mapDataStruct.id >= maxId)
            {
                maxId = mapDataStruct.id + 1;
            }
        }
        
        if (loadFromSavedData)
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
        FileManager.WriteToFile("mapSetting", savedString);
        
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
        FileManager.WriteToFile("mapSetting", savedString);
    }

    private ListOfMapsStruct loadMapsStructs()
    {
        string res;
        ListOfMapsStruct loadedStructs = new ListOfMapsStruct();
        FileManager.LoadFromFile("mapSetting", out res);
        Debug.Log(res);
        loadedStructs.LoadFromJson(res);
        Debug.Log(loadedStructs._structsMap[0].blocksStates.Count);
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
    public float blockSize;
    public List<int> blocksStates;
    public List<int> playerPos;
}