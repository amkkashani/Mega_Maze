using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameManager : Singleton<GameManager>
{
    [SerializeField] private List<Map> _maps;
    public ListOfMapsStruct ListOfMapsStruct = new ListOfMapsStruct();

    void Start()
    {
        // load
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
                Debug.Log("find the id ");
            }
            
        }
        
        //save entire accepted maps in the directory
        Debug.Log(ListOfMapsStruct._structsMap);
        string savedString = JsonUtility.ToJson(ListOfMapsStruct);
        Debug.Log(savedString);
        FileManager.WriteToFile("mapSetting", savedString);
    }
}

//this class just made for serializing with library 
[System.Serializable]
public class ListOfMapsStruct
{
    public List<MapDataStruct> _structsMap = new List<MapDataStruct>();
}

[System.Serializable]
public struct MapDataStruct
{
    public int id;
    public int XSize;
    public int ZSize;
    public float blockSize;
    public List<int> blocksStates;
    public int[] playerPos;
}