using UnityEditor;
using UnityEngine;

namespace MapScripts.Editor
{
    [CustomEditor(typeof(MapGenerator))]
    public class MapGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var generator = (MapGenerator)target;

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
}
