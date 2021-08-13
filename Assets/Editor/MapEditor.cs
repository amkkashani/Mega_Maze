using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;

[CustomEditor(typeof(Map))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Map myScript = (Map)target;
        if(GUILayout.Button("save this map"))
        {
            myScript.saveMap();
        }
    }
    
}
