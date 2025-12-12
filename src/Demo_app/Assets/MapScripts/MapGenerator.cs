using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MapScripts
{
    public class MapGenerator : MonoBehaviour
    {
        public enum DrawMode { NoiseMap, ColourMap }
        public DrawMode drawMode;

        [Range(50, 300)]
        public int mapWidth;
        [Range(50, 300)]
        public int mapHeight;
        public float noiseScale;
        public int octaves;
        [Range(0, 1)]
        public float persistance;
        [Range(1, 10)]
        public float lacunarity;
        public bool autoUpdate;
        public TerrainType[] regions;
        public int seed;
        public Vector2 offset;

        [Range(0, 1)]
        public float objectHeightThresholdMin;
        [Range(0, 1)]
        public float objectHeightThresholdMax;
        [Range(0, 1)]
        public float objectSpawnChance;
        public float minDistanceBetweenObjects = 5f;

        /// <summary>
        /// Если включено - смещение для биомного шума будет случайным при каждом запуске
        /// </summary>
        public bool randomizeBiomeNoise = true;

        private readonly List<Vector2> _objectPositions = new List<Vector2>();
        private float[,] _noiseMap;
        public GameObject objectPrefab;

        private Color[] _colourMap;
        private const float MaxRoadDistance = 50f;

        /// <summary>
        /// Генерирует карту с объектами, дорогами и биомами
        /// </summary>
        public void GenerateMap()
        {
            DeletePreviousObjects();

            _noiseMap = Noise.GenerateNoiseMap(mapWidth,
                mapHeight,
                seed,
                noiseScale,
                octaves,
                persistance,
                lacunarity,
                offset);
            Debug.Log("NoiseMap generated. Size: " +
                $"{_noiseMap.GetLength(0)}x{_noiseMap.GetLength(1)}");

            var biomeGen = FindFirstObjectByType<BiomeGenerator>();
            if (!biomeGen)
            {
                Debug.LogError("BiomeGenerator не найден в сцене!");
                return;
            }
            if (randomizeBiomeNoise)
            {
                biomeGen.offset = new Vector2(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
            }
            float[,] biomeNoiseMap = biomeGen.GenerateBiomeNoiseMap(mapWidth, mapHeight);

            _colourMap = new Color[mapHeight * mapWidth];
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float currentHeight = _noiseMap[x, y];
                    float currentBiomeNoise = biomeNoiseMap[x, y];
                    string biome = GetBiome(currentHeight, currentBiomeNoise);

                    if (currentHeight >= objectHeightThresholdMin && currentHeight <= objectHeightThresholdMax &&
                        Random.value < objectSpawnChance)
                    {
                        var newObjectPosition = new Vector2(x, y);

                        if (IsFarEnoughFromOtherObjects(newObjectPosition))
                        {
                            CreateObject(newObjectPosition, currentHeight, biome);
                            _colourMap[y * mapWidth + x] = Color.black;
                            _objectPositions.Add(newObjectPosition);
                        }
                        else
                        {
                            _colourMap[y * mapWidth + x] = GetColorForBiome(biome);
                        }
                    }
                    else
                    {
                        _colourMap[y * mapWidth + x] = GetColorForBiome(biome);
                    }
                }
            }
            Debug.Log($"First color in ColourMap: {_colourMap[0]}");
            var display = FindFirstObjectByType<MapDisplay>();
            switch (drawMode)
            {
                case DrawMode.NoiseMap:
                    Debug.Log("Drawing NoiseMap...");
                    display.DrawTexture(TextureGenerator.TextureFromHeightMap(_noiseMap));
                    break;
                case DrawMode.ColourMap:
                    Debug.Log("Drawing ColourMap...");
                    display.DrawTexture(TextureGenerator.TextureFromColourMap(_colourMap, mapWidth, mapHeight));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            DrawRoadBetweenClosestObjects();

            _objectPositions.Clear();
        }

        /// <summary>
        /// Определяет тип биома на основе высоты и дополнительного шума
        /// </summary>
        /// <param name="height">Высота точки на карте</param>
        /// <param name="biomeNoise">Дополнительный шум для вариативности биомов</param>
        /// <returns>Название биома для указанной точки</returns>
        private static string GetBiome(float height, float biomeNoise)
        {
            return height switch
            {
                <= 0.4f => biomeNoise < 0.5f ? "Water" : "Deep Water",
                <= 0.44f => "Sand",
                <= 0.6f => biomeNoise < 0.5f ? "Grassland" : "Forest",
                <= 0.69f => biomeNoise < 0.5f ? "Forest" : "Jungle",
                <= 0.8f => "MountainBase",
                <= 0.85f => "MountainMid",
                <= 0.9f => "MountainHigh",
                _ => "MountainPeak"
            };
        }

        /// <summary>
        /// Возвращает цвет для отображения указанного биома на карте
        /// </summary>
        /// <param name="biome">Название биома</param>
        /// <returns>Цвет для визуализации биома</returns>
        private static Color GetColorForBiome(string biome)
        {
            return biome switch
            {
                "Water" => new Color(0.1f, 0.1f, 0.8f),
                "Deep Water" => new Color(0.0f, 0.0f, 0.5f),
                "Sand" => new Color(0.93f, 0.79f, 0.69f),
                "Grassland" => new Color(0.5f, 0.8f, 0.2f),
                "Forest" => new Color(0.1f, 0.5f, 0.1f),
                "Jungle" => new Color(0.0f, 0.4f, 0.0f),
                "MountainBase" => Color.gray,
                "MountainMid" => new Color(0.6f, 0.6f, 0.6f),
                "MountainHigh" => new Color(0.8f, 0.8f, 0.8f),
                "MountainPeak" => Color.white,
                _ => Color.magenta
            };
        }

        /// <summary>
        /// Рисует дороги между ближайшими объектами на карте
        /// </summary>
        private void DrawRoadBetweenClosestObjects()
        {
            if (_objectPositions.Count < 2) return;

            HashSet<(Vector2, Vector2)> connectedPairs = new HashSet<(Vector2, Vector2)>();

            for (int i = 0; i < _objectPositions.Count; i++)
            {
                var closestObject = Vector2.zero;
                float closestDistance = float.MaxValue;

                for (int j = 0; j < _objectPositions.Count; j++)
                {
                    if (i == j) continue;
                    float distance = Vector2.Distance(_objectPositions[i], _objectPositions[j]);

                    if (!(distance <= MaxRoadDistance) || !(distance < closestDistance)) continue;
                    closestDistance = distance;
                    closestObject = _objectPositions[j];
                }

                if (!(closestDistance <= MaxRoadDistance) || closestObject == Vector2.zero) continue;
                var pair = (_objectPositions[i], closestObject);
                var reversePair = (closestObject, _objectPositions[i]);

                if (connectedPairs.Contains(pair) || connectedPairs.Contains(reversePair)) continue;
                DrawRoad(_objectPositions[i], closestObject);
                connectedPairs.Add(pair);
            }
        }

        /// <summary>
        /// Рисует дорогу между двумя точками на карте
        /// </summary>
        /// <param name="start">Начальная точка дороги</param>
        /// <param name="end">Конечная точка дороги</param>
        private void DrawRoad(Vector2 start, Vector2 end)
        {
            List<Vector2> path = FindPath(start, end);

            foreach (var point in path)
            {
                int x = Mathf.RoundToInt(point.x);
                int y = Mathf.RoundToInt(point.y);

                if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
                {
                    _colourMap[y * mapWidth + x] = Color.red;
                }
            }

            MapDisplay display = FindFirstObjectByType<MapDisplay>();
            display.DrawTexture(TextureGenerator.TextureFromColourMap(_colourMap, mapWidth, mapHeight));
        }

        /// <summary>
        /// Находит путь между двумя точками с использованием алгоритма A*
        /// </summary>
        /// <param name="start">Начальная точка пути</param>
        /// <param name="end">Конечная точка пути</param>
        /// <returns>Список точек, составляющих найденный путь</returns>
        private List<Vector2> FindPath(Vector2 start, Vector2 end)
        {
            List<Vector2> openSet = new List<Vector2> { start };
            Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>();

            Dictionary<Vector2, float> gScore = new Dictionary<Vector2, float>
            {
                [start] = 0
            };

            Dictionary<Vector2, float> fScore = new Dictionary<Vector2, float>
            {
                [start] = Vector2.Distance(start, end)
            };

            while (openSet.Count > 0)
            {
                var current = openSet[0];
                foreach (var node in openSet.Where(node =>
                    fScore.ContainsKey(node) && fScore[node] < fScore[current]))
                {
                    current = node;
                }

                if (current == end)
                {
                    List<Vector2> path = new List<Vector2>();
                    while (cameFrom.ContainsKey(current))
                    {
                        path.Add(current);
                        current = cameFrom[current];
                    }
                    path.Reverse();
                    return path;
                }

                openSet.Remove(current);

                foreach (var neighbor in GetNeighbors(current))
                {
                    float terrainCost = GetTerrainCost(neighbor);
                    if (Mathf.Approximately(terrainCost, float.MaxValue)) continue;

                    float tentativeGScore = gScore[current] + Vector2.Distance(current, neighbor) * terrainCost;

                    if (gScore.ContainsKey(neighbor) && !(tentativeGScore < gScore[neighbor])) continue;
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Vector2.Distance(neighbor, end);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }

            return new List<Vector2>();
        }

        /// <summary>
        /// Возвращает список соседних клеток для указанной позиции
        /// </summary>
        /// <param name="cell">Позиция для поиска соседей</param>
        /// <returns>Список доступных соседних позиций</returns>
        private List<Vector2> GetNeighbors(Vector2 cell)
        {

            int x = Mathf.RoundToInt(cell.x);
            int y = Mathf.RoundToInt(cell.y);

            Vector2[] directions = {
                new Vector2(-1, 0),
                new Vector2(1, 0),
                new Vector2(0, -1),
                new Vector2(0, 1)
            };

            return (directions.Select(dir => new { dir, neighborX = x + (int)dir.x })
                .Select(@t => new { @t, neighborY = y + (int)@t.dir.y })
                .Where(@t =>
                    @t.@t.neighborX >= 0 && @t.@t.neighborX < mapWidth && @t.neighborY >= 0 &&
                    @t.neighborY < mapHeight)
                .Select(@t => new { @t, height = _noiseMap[@t.@t.neighborX, @t.neighborY] })
                .Where(@t => !(@t.height < 0.41f) && !(@t.height > 0.79f))
                .Select(@t => new Vector2(@t.@t.@t.neighborX, @t.@t.neighborY))).ToList();
        }

        /// <summary>
        /// Вычисляет стоимость перемещения по указанной позиции на основе рельефа
        /// </summary>
        /// <param name="position">Позиция для оценки стоимости</param>
        /// <returns>Коэффициент стоимости перемещения</returns>
        private float GetTerrainCost(Vector2 position)
        {
            float height = _noiseMap[Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y)];

            return height switch
            {
                < 0.41f or > 0.79f => float.MaxValue,
                > 0.6f => 2f,
                _ => 1f
            };
        }

        /// <summary>
        /// Создает объект на карте в указанной позиции с учетом биома
        /// </summary>
        /// <param name="position">Позиция для создания объекта</param>
        /// <param name="height">Высота точки размещения</param>
        /// <param name="biome">Тип биома в точке размещения</param>
        private void CreateObject(Vector2 position, float height, string biome)
        {
            string objectName;

            if (biome is "MountainBase" or "MountainMid" or "MountainHigh" or "MountainPeak")
            {
                objectName = Random.value < 0.5f ? "Cave" : "Interest Place";
            }
            else
            {
                switch (biome)
                {
                    case "Water":
                    case "Deep Water":
                        return;
                    case "Sand":
                        objectName = "Port";
                        break;
                    case "Grassland":
                        objectName = "Plains City";
                        break;
                    case "Forest":
                    case "Jungle":
                        objectName = "Forest City";
                        break;
                    default:
                        return;
                }
            }

            float xOffset = mapWidth / 2f;
            float yOffset = mapHeight / 2f;
            const float scale = 10f;
            const float heightOffset = 5f;

            var rotation = Quaternion.Euler(-90, 90, 0);
            var obj = Instantiate(objectPrefab,
                new Vector3(
                    (-(position.x - xOffset) * scale - 5),
                    heightOffset,
                    (-(position.y - yOffset) * scale - 5)
                ),
                rotation);

            obj.name = objectName;
            obj.tag = "GeneratedObject";

            var boxCollider = obj.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;

            CreateNameLabel(obj, objectName);
        }

        /// <summary>
        /// Удаляет все ранее сгенерированные объекты сцены
        /// </summary>
        private static void DeletePreviousObjects()
        {
            GameObject[] previousObjects = GameObject.FindGameObjectsWithTag("GeneratedObject");

            Debug.Log($"Deleting {previousObjects.Length} objects");

            foreach (var obj in previousObjects)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(obj);
                }
                else
                {
                    Destroy(obj);
                }
            }
        }

        /// <summary>
        /// Создает текстовую метку с именем для игрового объекта
        /// </summary>
        /// <param name="parentObject">Родительский объект для метки</param>
        /// <param name="objectName">Текст для отображения в метке</param>
        private static void CreateNameLabel(GameObject parentObject, string objectName)
        {
            var textObject = new GameObject("NameLabel");
            textObject.transform.SetParent(parentObject.transform);

            var textMesh = textObject.AddComponent<TextMeshPro>();
            textMesh.text = objectName;
            textMesh.fontSize = 500;
            textMesh.color = Color.black;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.horizontalMapping = TextureMappingOptions.Line;
            textMesh.verticalMapping = TextureMappingOptions.MatchAspect;

            textMesh.fontStyle = FontStyles.Bold;

            textObject.transform.localPosition = new Vector3(-2f, 0, 2f);
            textObject.transform.localRotation = Quaternion.Euler(180, 0, 90);

            var rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(500, 20);
        }

        /// <summary>
        /// Проверяет корректность параметров в редакторе Unity
        /// </summary>
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("OnValidate is being called during play mode. Skipping execution.");
                return;
            }

            mapWidth = Mathf.Max(1, mapWidth);
            mapHeight = Mathf.Max(1, mapHeight);
            lacunarity = Mathf.Max(1, lacunarity);
            octaves = Mathf.Max(1, octaves);
        }

        /// <summary>
        /// Проверяет достаточность расстояния от новой позиции до существующих объектов
        /// </summary>
        /// <param name="newPosition">Новая позиция для проверки</param>
        /// <returns>True если позиция достаточно удалена от других объектов</returns>
        private bool IsFarEnoughFromOtherObjects(Vector2 newPosition)
        {
            return _objectPositions.All(pos => !(Vector2.Distance(pos,
                newPosition) < minDistanceBetweenObjects));
        }

        /// <summary>
        /// Очищает карту, удаляя все сгенерированные объекты
        /// </summary>
        public void ClearMap()
        {
            foreach (var obj in GameObject.FindGameObjectsWithTag("GeneratedObject"))
            {
                DestroyImmediate(obj);
            }
        }
    }

    /// <summary>
    /// Описание типа террейна для генерации карты
    /// </summary>
    [Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color colour;
    }
}