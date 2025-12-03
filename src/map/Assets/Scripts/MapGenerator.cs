using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColourMap };
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

    private List<Vector2> objectPositions = new List<Vector2>();
    private float[,] noiseMap;
    public GameObject objectPrefab;

    private Color[] colourMap;
    private const float maxRoadDistance = 50f;

    /// <summary>
    /// Генерирует карту с объектами, дорогами и биомами
    /// </summary>
    public void GenerateMap()
    {
        DeletePreviousObjects();

        noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
        Debug.Log($"NoiseMap generated. Size: {noiseMap.GetLength(0)}x{noiseMap.GetLength(1)}");

        BiomeGenerator biomeGen = FindObjectOfType<BiomeGenerator>();
        if (biomeGen == null)
        {
            Debug.LogError("BiomeGenerator не найден в сцене!");
            return;
        }
        if (randomizeBiomeNoise)
        {
            biomeGen.offset = new Vector2(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        }
        float[,] biomeNoiseMap = biomeGen.GenerateBiomeNoiseMap(mapWidth, mapHeight);

        colourMap = new Color[mapHeight * mapWidth];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                float currentBiomeNoise = biomeNoiseMap[x, y];
                string biome = GetBiome(currentHeight, currentBiomeNoise);

                if (currentHeight >= objectHeightThresholdMin && currentHeight <= objectHeightThresholdMax && Random.value < objectSpawnChance)
                {
                    Vector2 newObjectPosition = new Vector2(x, y);

                    if (IsFarEnoughFromOtherObjects(newObjectPosition))
                    {
                        CreateObject(newObjectPosition, currentHeight, biome);
                        colourMap[y * mapWidth + x] = Color.black;
                        objectPositions.Add(newObjectPosition);
                    }
                    else
                    {
                        colourMap[y * mapWidth + x] = GetColorForBiome(biome);
                    }
                }
                else
                {
                    colourMap[y * mapWidth + x] = GetColorForBiome(biome);
                }
            }
        }
        Debug.Log($"First color in ColourMap: {colourMap[0]}");
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            Debug.Log("Drawing NoiseMap...");
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            Debug.Log("Drawing ColourMap...");
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        }

        DrawRoadBetweenClosestObjects();

        objectPositions.Clear();
    }

    /// <summary>
    /// Определяет тип биома на основе высоты и дополнительного шума
    /// </summary>
    /// <param name="height">Высота точки на карте</param>
    /// <param name="biomeNoise">Дополнительный шум для вариативности биомов</param>
    /// <returns>Название биома для указанной точки</returns>
    private string GetBiome(float height, float biomeNoise)
    {
        if (height <= 0.4f)
        {
            return (biomeNoise < 0.5f) ? "Water" : "Deep Water";
        }
        else if (height <= 0.44f)
        {
            return "Sand";
        }
        else if (height <= 0.6f)
        {
            return (biomeNoise < 0.5f) ? "Grassland" : "Forest";
        }
        else if (height <= 0.69f)
        {
            return (biomeNoise < 0.5f) ? "Forest" : "Jungle";
        }
        else if (height <= 0.8f)
        {
            return "MountainBase";
        }
        else if (height <= 0.85f)
        {
            return "MountainMid";
        }
        else if (height <= 0.9f)
        {
            return "MountainHigh";
        }
        else
        {
            return "MountainPeak";
        }
    }

    /// <summary>
    /// Возвращает цвет для отображения указанного биома на карте
    /// </summary>
    /// <param name="biome">Название биома</param>
    /// <returns>Цвет для визуализации биома</returns>
    private Color GetColorForBiome(string biome)
    {
        switch (biome)
        {
            case "Water": return new Color(0.1f, 0.1f, 0.8f);
            case "Deep Water": return new Color(0.0f, 0.0f, 0.5f);
            case "Sand": return new Color(0.93f, 0.79f, 0.69f);
            case "Grassland": return new Color(0.5f, 0.8f, 0.2f);
            case "Forest": return new Color(0.1f, 0.5f, 0.1f);
            case "Jungle": return new Color(0.0f, 0.4f, 0.0f);
            case "MountainBase": return Color.gray;
            case "MountainMid": return new Color(0.6f, 0.6f, 0.6f);
            case "MountainHigh": return new Color(0.8f, 0.8f, 0.8f);
            case "MountainPeak": return Color.white;
            default: return Color.magenta;
        }
    }

    /// <summary>
    /// Рисует дороги между ближайшими объектами на карте
    /// </summary>
    private void DrawRoadBetweenClosestObjects()
    {
        if (objectPositions.Count < 2) return;

        HashSet<(Vector2, Vector2)> connectedPairs = new HashSet<(Vector2, Vector2)>();

        for (int i = 0; i < objectPositions.Count; i++)
        {
            Vector2 closestObject = Vector2.zero;
            float closestDistance = float.MaxValue;

            for (int j = 0; j < objectPositions.Count; j++)
            {
                if (i != j)
                {
                    float distance = Vector2.Distance(objectPositions[i], objectPositions[j]);

                    if (distance <= maxRoadDistance && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestObject = objectPositions[j];
                    }
                }
            }

            if (closestDistance <= maxRoadDistance && closestObject != Vector2.zero)
            {
                var pair = (objectPositions[i], closestObject);
                var reversePair = (closestObject, objectPositions[i]);

                if (!connectedPairs.Contains(pair) && !connectedPairs.Contains(reversePair))
                {
                    DrawRoad(objectPositions[i], closestObject);
                    connectedPairs.Add(pair);
                }
            }
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

        foreach (Vector2 point in path)
        {
            int x = Mathf.RoundToInt(point.x);
            int y = Mathf.RoundToInt(point.y);

            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
            {
                colourMap[y * mapWidth + x] = Color.red;
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
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

        Dictionary<Vector2, float> gScore = new Dictionary<Vector2, float>();
        gScore[start] = 0;

        Dictionary<Vector2, float> fScore = new Dictionary<Vector2, float>();
        fScore[start] = Vector2.Distance(start, end);

        while (openSet.Count > 0)
        {
            Vector2 current = openSet[0];
            foreach (Vector2 node in openSet)
            {
                if (fScore.ContainsKey(node) && fScore[node] < fScore[current])
                {
                    current = node;
                }
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

            foreach (Vector2 neighbor in GetNeighbors(current))
            {
                float terrainCost = GetTerrainCost(neighbor);
                if (terrainCost == float.MaxValue) continue;

                float tentativeGScore = gScore[current] + Vector2.Distance(current, neighbor) * terrainCost;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Vector2.Distance(neighbor, end);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
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
        List<Vector2> neighbors = new List<Vector2>();

        int x = Mathf.RoundToInt(cell.x);
        int y = Mathf.RoundToInt(cell.y);

        Vector2[] directions = new Vector2[]
        {
            new Vector2(-1, 0),
            new Vector2(1, 0),
            new Vector2(0, -1),
            new Vector2(0, 1),
        };

        foreach (var dir in directions)
        {
            int neighborX = x + (int)dir.x;
            int neighborY = y + (int)dir.y;

            if (neighborX >= 0 && neighborX < mapWidth && neighborY >= 0 && neighborY < mapHeight)
            {
                float height = noiseMap[neighborX, neighborY];

                if (height < 0.41f || height > 0.79f)
                    continue;

                neighbors.Add(new Vector2(neighborX, neighborY));
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Вычисляет стоимость перемещения по указанной позиции на основе рельефа
    /// </summary>
    /// <param name="position">Позиция для оценки стоимости</param>
    /// <returns>Коэффициент стоимости перемещения</returns>
    private float GetTerrainCost(Vector2 position)
    {
        float height = noiseMap[Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y)];

        if (height < 0.41f || height > 0.79f)
            return float.MaxValue;
        else if (height > 0.6f)
            return 2f;
        else
            return 1f;
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

        if (biome == "MountainBase" || biome == "MountainMid" || biome == "MountainHigh" || biome == "MountainPeak")
        {
            objectName = (Random.value < 0.5f) ? "Cave" : "Interest Place";
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
        float scale = 10f;
        float heightOffset = 5f;

        Quaternion rotation = Quaternion.Euler(-90, 90, 0);
        GameObject obj = Instantiate(objectPrefab,
            new Vector3(
                (-(position.x - xOffset) * scale - 5),
                heightOffset,
                (-(position.y - yOffset) * scale - 5)
            ),
            rotation);

        obj.name = objectName;
        obj.tag = "GeneratedObject";

        BoxCollider collider = obj.AddComponent<BoxCollider>();
        collider.isTrigger = true;

        CreateNameLabel(obj, objectName);
    }

    /// <summary>
    /// Удаляет все ранее сгенерированные объекты сцены
    /// </summary>
    private void DeletePreviousObjects()
    {
        GameObject[] previousObjects = GameObject.FindGameObjectsWithTag("GeneratedObject");

        Debug.Log($"Deleting {previousObjects.Length} objects");

        foreach (GameObject obj in previousObjects)
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
    private void CreateNameLabel(GameObject parentObject, string objectName)
    {
        GameObject textObject = new GameObject("NameLabel");
        textObject.transform.SetParent(parentObject.transform);

        TextMeshPro textMesh = textObject.AddComponent<TextMeshPro>();
        textMesh.text = objectName;
        textMesh.fontSize = 500;
        textMesh.color = Color.black;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.horizontalMapping = TextureMappingOptions.Line;
        textMesh.verticalMapping = TextureMappingOptions.MatchAspect;

        textMesh.fontStyle = FontStyles.Bold;

        textObject.transform.localPosition = new Vector3(-2f, 0, 2f);
        textObject.transform.localRotation = Quaternion.Euler(180, 0, 90);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
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
        foreach (Vector2 pos in objectPositions)
        {
            if (Vector2.Distance(pos, newPosition) < minDistanceBetweenObjects)
            {
                return false;
            }
        }
        return true;
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
[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}