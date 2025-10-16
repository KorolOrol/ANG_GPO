using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGeneratorManual))]
public class MapGeneratorManualEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapGeneratorManual generator = (MapGeneratorManual)target;

        GUILayout.Space(10);
        GUILayout.Label("=== ”правление картой ===", EditorStyles.boldLabel);

        if (GUILayout.Button("—генерировать карту вручную"))
        {
            generator.GenerateManualMap();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("—охранить карту"))
        {
            generator.SaveMapContext();
        }

        if (GUILayout.Button("Zагрузить карту"))
        {
            generator.LoadMapContext();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "‘айлы сохран€ютс€ в Assets/SavedMaps как .png и .json.\n" +
            "»м€ файла задаЄтс€ в поле 'saveFileName'.", MessageType.Info);
    }
}
