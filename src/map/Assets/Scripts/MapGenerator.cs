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

    // Новое свойство: если true – смещение для биомного шума будет случайным, что даёт разные биомы при каждом запуске.
    public bool randomizeBiomeNoise = true;

    private List<Vector2> objectPositions = new List<Vector2>();
    private float[,] noiseMap;
    public GameObject objectPrefab;

    private Color[] colourMap;
    private const float maxRoadDistance = 50f; // Максимальное расстояние для создания дороги

    public void GenerateMap()
    {
        DeletePreviousObjects();

        // Генерация основного шума высот
        noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
        Debug.Log($"NoiseMap generated. Size: {noiseMap.GetLength(0)}x{noiseMap.GetLength(1)}");

        // Генерация дополнительного шума для биомов через компонент BiomeGenerator
        BiomeGenerator biomeGen = FindObjectOfType<BiomeGenerator>();
        if (biomeGen == null)
        {
            Debug.LogError("BiomeGenerator не найден в сцене!");
            return;
        }
        // Если включена случайность, обновляем смещение для биомного шума
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
                // Вычисляем биом с учётом высоты и дополнительного шума
                string biome = GetBiome(currentHeight, currentBiomeNoise);

                if (currentHeight >= objectHeightThresholdMin && currentHeight <= objectHeightThresholdMax && Random.value < objectSpawnChance)
                {
                    Vector2 newObjectPosition = new Vector2(x, y);

                    if (IsFarEnoughFromOtherObjects(newObjectPosition))
                    {
                        // Передаём вычисленный биом в метод создания объекта
                        CreateObject(newObjectPosition, currentHeight, biome);
                        colourMap[y * mapWidth + x] = Color.black;
                        objectPositions.Add(newObjectPosition);

                        /* Debug.Log($"Object created at position: {newObjectPosition}"); */
                    }
                    else
                    {
                        // Вместо старой ApplyRegionColour используем новый метод для цвета по биому
                        colourMap[y * mapWidth + x] = GetColorForBiome(biome);
                    }
                }
                else
                {
                    // Применяем цвет с учётом вычисленного биома
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

    // Новый метод для определения биома по высоте и дополнительному шуму
    private string GetBiome(float height, float biomeNoise)
    {
        if (height <= 0.4f)
        {
            // Для низких высот можно варьировать тип воды или болота
            return (biomeNoise < 0.5f) ? "Water" : "Deep Water";
        }
        else if (height <= 0.44f)
        {
            return "Sand";
        }
        else if (height <= 0.6f)
        {
            // Используем биомный шум для выбора между лугом и лесом
            return (biomeNoise < 0.5f) ? "Grassland" : "Forest";
        }
        else if (height <= 0.69f)
        {
            // Дополнительное разделение для леса или, например, джунглей
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

    // Новый метод для получения цвета для вычисленного биома
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
                    /* Debug.Log($"Drawing road between {objectPositions[i]} and {closestObject}, Distance: {closestDistance}"); */
                    DrawRoad(objectPositions[i], closestObject);
                    connectedPairs.Add(pair);
                }
            }
        }
    }

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

    // Обновленный метод CreateObject – теперь принимает вычисленный биом
    private void CreateObject(Vector2 position, float height, string biome)
    {
        string objectName;

        // Если биом горный, создаём пещеры или интересные места
        if (biome == "MountainBase" || biome == "MountainMid" || biome == "MountainHigh" || biome == "MountainPeak")
        {
            objectName = (Random.value < 0.5f) ? "Cave" : "Interest Place";
        }
        else
        {
            // Для остальных биомов создаём города или точки интереса
            switch (biome)
            {
                case "Water":
                case "Deep Water":
                    return; // Не создаём объекты на воде
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
                    return; // На неизвестных биомах объекты не создаются
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

    private void CreateNameLabel(GameObject parentObject, string objectName)
    {
        GameObject textObject = new GameObject("NameLabel");
        textObject.transform.SetParent(parentObject.transform);

        TextMeshPro textMesh = textObject.AddComponent<TextMeshPro>();
        textMesh.text = objectName; // Устанавливаем имя объекта
        textMesh.fontSize = 500; // Размер текста
        textMesh.color = Color.black;
        textMesh.alignment = TextAlignmentOptions.Center; // Центрируем текст
        textMesh.horizontalMapping = TextureMappingOptions.Line;
        textMesh.verticalMapping = TextureMappingOptions.MatchAspect;

        // Устанавливаем жирный шрифт
        textMesh.fontStyle = FontStyles.Bold;

        // Позиционируем текст над объектом
        textObject.transform.localPosition = new Vector3(-2f, 0, 2f); // Над объектом на высоте 2
        textObject.transform.localRotation = Quaternion.Euler(180, 0, 90); // Поворот текста на 90 градусов по X

        // Устанавливаем размер текстового поля, чтобы оно было достаточно широким
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(500, 20); // Увеличиваем ширину текста
    }

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
    public void ClearMap()
    {
        // Удаляем все объекты с тегом "GeneratedObject"
        foreach (var obj in GameObject.FindGameObjectsWithTag("GeneratedObject"))
        {
            DestroyImmediate(obj);
        }
    }


    // Старая версия GetBiome удалена – используется новая версия с двумя параметрами
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}