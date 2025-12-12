using UnityEditor;
using UnityEngine;

namespace MapScripts.Editor
{
    [CustomEditor(typeof(MapGeneratorManual))]
    public class MapGeneratorManualEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MapGeneratorManual generator = (MapGeneratorManual)target;

            GUILayout.Space(10);
            GUILayout.Label("=== Управление картой ===", EditorStyles.boldLabel);

            if (GUILayout.Button("Сгенерировать карту вручную"))
            {
                generator.GenerateManualMap();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Сохранить карту"))
            {
                generator.SaveMapContext();
            }

            if (GUILayout.Button("Загрузить карту"))
            {
                generator.LoadMapContext();
            }

            GUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Файлы сохраняются в Assets/SavedMaps как .png и .json.\n" +
                "Имя файла задаётся в поле 'saveFileName'.", MessageType.Info);
        }
    }
}
