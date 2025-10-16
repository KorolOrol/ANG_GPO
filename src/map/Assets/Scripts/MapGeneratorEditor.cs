using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapGenerator generator = (MapGenerator)target;

        if (GUILayout.Button("Generate Map"))
        {
            generator.GenerateMap();
        }

        if (GUILayout.Button("Clear Map"))
        {
            generator.ClearMap();
        }
    }
}
