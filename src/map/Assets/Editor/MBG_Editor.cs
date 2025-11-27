using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(ModularBuildingGenerator))]
public class MBG_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ModularBuildingGenerator MBG_Gen = (ModularBuildingGenerator)target;

        GUILayout.Space(20);
        if (GUILayout.Button("Generate Building", GUILayout.Height(30)))
        {
            MBG_Gen.GenerateBuilding();
        }

        if (GUILayout.Button("TestGenerate Building", GUILayout.Height(30)))
        {
            MBG_Gen.TestGenerateBuilding();
        }

        if (GUILayout.Button("Clear"))
        {
            MBG_Gen.DestroyBuilding();
        }
    }
}
