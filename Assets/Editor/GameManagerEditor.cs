using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GameManager myScript = (GameManager)target;
        if(GUILayout.Button("remove by id"))
        {
            myScript.removeSavedId(myScript.removeId);
        }
    }
}
