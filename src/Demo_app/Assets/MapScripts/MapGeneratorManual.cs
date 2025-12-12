using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MapScripts.LocationGenerate;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MapScripts
{
    /// <summary>
    /// Определение локации для ручного размещения на карте
    /// </summary>
    [System.Serializable]
    public class LocationDefinition
    {
        public string locationName;
        public string biome;
        public List<string> connectedLocations;
    }

    /// <summary>
    /// Данные сохранения позиции и связей локации
    /// </summary>
    [System.Serializable]
    public class LocationSaveData
    {
        public string locationName;
        public string biome;
        public int x;
        public int y;
        public List<string> connectedLocations;
    }

    /// <summary>
    /// Структура для сохранения всей карты с параметрами генерации
    /// </summary>
    [System.Serializable]
    public class MapSaveData
    {
        public int mapWidth;
        public int mapHeight;

        public byte[] compressedHeightMap;
        public byte[] compressedBiomeMap;

        public List<LocationSaveData> locations;

        public int seed;
        public float noiseScale;
        public int octaves;
        public float persistance;
        public float lacunarity;
        public Vector2 noiseOffset;
        public int chunkSize;

        public bool drawChunkBorders;
        public bool showChunkBiomes;
        public bool highlightDominantBiome;
    }

    /// <summary>
    /// Генератор карты с ручным размещением локаций и функционалом сохранения
    /// </summary>
    public class MapGeneratorManual : MonoBehaviour
    {
        [Header("Map Settings")]
        [Range(50, 300)] public int mapWidth = 100;
        [Range(50, 300)] public int mapHeight = 100;
        public int chunkSize = 20;

        [Header("Noise & Biome")]
        public float noiseScale = 50f;
        public int octaves = 4;
        [Range(0, 1)] public float persistance = 0.5f;
        [Range(1, 10)] public float lacunarity = 2f;
        public int seed = 42;
        public Vector2 noiseOffset;
        public BiomeGenerator biomeGenerator;

        [Header("Debug Options")]
        public bool drawChunkBorders = true;
        public bool showChunkBiomes = true;
        public bool highlightDominantBiome;

        [Header("Manual Locations")]
        public List<LocationDefinition> locations = new List<LocationDefinition>();
        public GameObject objectPrefab;

        [Header("Save/Load")]
        [Tooltip("Имя файла (без расширения) для сохранения/загрузки")]
        public string saveFileName = "map_saved";

        [Header("UI Display")]
        [Tooltip("Добавьте сюда компонент MapDisplayUI для отображения карты в интерфейсе")]
        public MapDisplayUI uiMapDisplay;

        [Header("Location UI")]
        [Tooltip("Добавьте сюда компонент LocationUIManager для отображения маркеров локаций")]
        public LocationUIManager locationUIManager;

        private float[,] _heightMap;
        private float[,] _biomeNoiseMap;
        private Color[] _colourMap;

        private readonly Dictionary<string, Vector2> _placedLocations = new Dictionary<string, Vector2>();

        /// <summary>
        /// Сжимает карту высот из float в byte для уменьшения размера файла
        /// </summary>
        private static byte[] CompressFloatMap(float[,] map, int width, int height)
        {
            byte[] compressed = new byte[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    compressed[y * width + x] = (byte)Mathf.Clamp(
                        Mathf.RoundToInt(map[x, y] * 255f), 0, 255);
                }
            }
            return compressed;
        }

        public void ClearMap()
        {
            // Удаляем все созданные объекты
            GameObject[] generatedObjects = GameObject.FindGameObjectsWithTag("GeneratedObject");
            foreach (var obj in generatedObjects)
            {
                DestroyImmediate(obj);
            }

            _placedLocations.Clear();
            locations.Clear();

            // Очищаем UI
            if (uiMapDisplay)
            {
                uiMapDisplay.ClearMap();
            }

            if (locationUIManager)
            {
                locationUIManager.ClearMarkers();
            }

            Debug.Log("Карта и все локации очищены");
        }

        /// <summary>
        /// Получает текущую текстуру карты
        /// </summary>
        public Texture2D GetCurrentTexture()
        {
            if (_colourMap == null || _colourMap.Length == 0)
                return null;

            return TextureGenerator.TextureFromColourMap(_colourMap, mapWidth, mapHeight);
        }

        /// <summary>
        /// Восстанавливает карту высот из сжатого byte массива
        /// </summary>
        private static float[,] DecompressByteMap(byte[] compressed, int width, int height)
        {
            float[,] map = new float[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[x, y] = compressed[y * width + x] / 255f;
                }
            }
            return map;
        }

        /// <summary>
        /// Генерирует карту с ручным размещением локаций по биомам
        /// </summary>
        public void GenerateManualMap()
        {
            Debug.Log("=== НАЧАЛО ГЕНЕРАЦИИ КАРТЫ ===");

            // Очистка
            ClearMap();

            _placedLocations.Clear();

            // Генерация карт
            _heightMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, noiseOffset);

            if (!biomeGenerator)
            {
                Debug.LogWarning("BiomeGenerator не назначен, создаем случайный шум для биомов");
                _biomeNoiseMap = new float[mapWidth, mapHeight];
                for (int y = 0; y < mapHeight; y++)
                for (int x = 0; x < mapWidth; x++)
                    _biomeNoiseMap[x, y] = Random.value;
            }
            else
            {
                _biomeNoiseMap = biomeGenerator.GenerateBiomeNoiseMap(mapWidth, mapHeight);
            }

            // Создаем цветовую карту
            _colourMap = new Color[mapWidth * mapHeight];
            for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
                _colourMap[y * mapWidth + x] = GetColorForBiome(GetBiome(_heightMap[x, y], _biomeNoiseMap[x, y]));

            // Границы чанков
            if (drawChunkBorders)
            {
                for (int cx = 0; cx <= mapWidth; cx += chunkSize)
                for (int y = 0; y < mapHeight; y++)
                    if (cx < mapWidth) _colourMap[y * mapWidth + cx] = Color.black;

                for (int cy = 0; cy <= mapHeight; cy += chunkSize)
                for (int x = 0; x < mapWidth; x++)
                    if (cy < mapHeight) _colourMap[cy * mapWidth + x] = Color.black;
            }

            // Рассчитываем биомы чанков
            string[,] chunkBiome = CalculateChunkBiomes();

            // Отображаем биомы чанков (только если нужны 3D объекты)
            if (showChunkBiomes && objectPrefab)
            {
                int chunksX = Mathf.CeilToInt((float)mapWidth / chunkSize);
                int chunksY = Mathf.CeilToInt((float)mapHeight / chunkSize);

                for (int cx = 0; cx < chunksX; cx++)
                {
                    for (int cy = 0; cy < chunksY; cy++)
                    {
                        float centerX = cx * chunkSize + chunkSize / 2f;
                        float centerY = cy * chunkSize + chunkSize / 2f;
                        float worldX = -(centerX - mapWidth / 2f) * 10f;
                        float worldZ = -(centerY - mapHeight / 2f) * 10f;

                        var textObj = new GameObject($"BiomeLabel_{cx}_{cy}");
                        textObj.transform.SetParent(transform);
                        textObj.transform.position = new Vector3(worldX, 10f, worldZ);
                        textObj.transform.rotation = Quaternion.Euler(90, 0, 0);
                        textObj.transform.localScale = Vector3.one * 65f;
                        textObj.tag = "GeneratedObject";

                        var tm = textObj.AddComponent<TextMeshPro>();
                        tm.text = chunkBiome[cx, cy];
                        tm.fontSize = 10f;
                        tm.enableAutoSizing = false;
                        tm.alignment = TextAlignmentOptions.Center;
                        tm.textWrappingMode = TextWrappingModes.NoWrap;
                        tm.overflowMode = TextOverflowModes.Overflow;
                        tm.ForceMeshUpdate();
                    }
                }
            }

            // Подсветка доминирующего биома
            if (highlightDominantBiome)
            {
                int chunksX = Mathf.CeilToInt((float)mapWidth / chunkSize);
                int chunksY = Mathf.CeilToInt((float)mapHeight / chunkSize);

                for (int cx = 0; cx < chunksX; cx++)
                {
                    for (int cy = 0; cy < chunksY; cy++)
                    {
                        string mainB = chunkBiome[cx, cy];
                        for (int dx = 0; dx < chunkSize; dx++)
                        for (int dy = 0; dy < chunkSize; dy++)
                        {
                            int x = cx * chunkSize + dx, y = cy * chunkSize + dy;
                            if (x >= mapWidth || y >= mapHeight) continue;
                            if (GetBiome(_heightMap[x, y], _biomeNoiseMap[x, y]) == mainB)
                                _colourMap[y * mapWidth + x] = Color.red;
                        }
                    }
                }
            }

            // Размещаем существующие локации
            _placedLocations.Clear();
            foreach (var loc in locations)
            {
                PlaceLocation(loc);
            }

            // Рисуем дороги между связанными локациями
            foreach (var loc in locations)
                if (_placedLocations.TryGetValue(loc.locationName, out var s))
                    foreach (string other in loc.connectedLocations)
                        if (_placedLocations.TryGetValue(other, out var e))
                            DrawSimpleRoad(s, e);

            // Отображаем карту
            var mapTexture = TextureGenerator.TextureFromColourMap(_colourMap, mapWidth, mapHeight);

            if (uiMapDisplay)
            {
                uiMapDisplay.DrawTexture(mapTexture);
                Debug.Log("Карта отображена в UI");
            }
            else
            {
                Debug.LogWarning("uiMapDisplay не назначен!");
            }

            Debug.Log("=== ГЕНЕРАЦИЯ КАРТЫ ЗАВЕРШЕНА ===");
        }

        /// <summary>
        /// Размещает локацию на карте
        /// </summary>
        private void PlaceLocation(LocationDefinition loc)
        {
            int chunksX = Mathf.CeilToInt((float)mapWidth / chunkSize);
            int chunksY = Mathf.CeilToInt((float)mapHeight / chunkSize);
            string[,] chunkBiome = CalculateChunkBiomes();

            List<Vector2Int> valid = new List<Vector2Int>();
            for (int cx = 0; cx < chunksX; cx++)
            for (int cy = 0; cy < chunksY; cy++)
                if (chunkBiome[cx, cy] == loc.biome)
                    valid.Add(new Vector2Int(cx, cy));

            if (valid.Count == 0)
            {
                Debug.LogWarning($"Нет чанков с биомом {loc.biome} для локации {loc.locationName}");
                return;
            }

            var ch = valid[Random.Range(0, valid.Count)];
            int px = Mathf.Clamp(ch.x * chunkSize + Random.Range(0, chunkSize), 0, mapWidth - 1);
            int py = Mathf.Clamp(ch.y * chunkSize + Random.Range(0, chunkSize), 0, mapHeight - 1);

            _placedLocations[loc.locationName] = new Vector2(px, py);
            CreateLocationObject(loc.locationName, px, py);
        }

        /// <summary>
        /// Создает объект локации в указанной позиции на карте
        /// </summary>
        private void CreateLocationObject(string locName, int x, int y)
        {
            // Определяем цвет для биома
            string biome = GetLocationBiome(locName);
            var color = GetBiomeColor(biome);

            // СОЗДАЕМ КУБ ПРОГРАММНО
            var cube =
                // Используем существующий префаб
                objectPrefab ? Instantiate(objectPrefab) :
                // Создаем куб программно
                GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = locName;
            cube.tag = "GeneratedObject";
            cube.transform.localScale = new Vector3(50f, 50f, 50f);

            // Рассчитываем позицию в мире
            float worldX = -(x - mapWidth / 2f) * 10f;
            float worldZ = -(y - mapHeight / 2f) * 10f;

            // Настраиваем позицию
            cube.transform.position = new Vector3(worldX, 5f, worldZ);
            cube.transform.SetParent(transform);

            // НАСТРАИВАЕМ МАТЕРИАЛ И ЦВЕТ
            var renderer = cube.GetComponent<Renderer>();
            if (renderer)
            {
                // Создаем новый материал
                var material = new Material(Shader.Find("Standard"))
                {
                    color = color
                };

                // Добавляем свечение для лучшей видимости
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 0.3f);

                renderer.material = material;
            }

            // ДОБАВЛЯЕМ ПОДПИСЬ (TextMeshPro)
            CreateLocationLabel(cube, locName, color);

            // ДОБАВЛЯЕМ КОМПОНЕНТ ДЛЯ УПРАВЛЕНИЯ
            var locationComponent = cube.AddComponent<LocationObject>();
            locationComponent.Initialize(locName, biome, new Vector2(x, y));

            // UI маркер
            if (locationUIManager)
            {
                var markerColor = GetRandomColorForLocation(locName);
                locationUIManager.AddLocationMarker(locName, new Vector2(x, y), markerColor);
            }

            Debug.Log($"Создан куб локации '{locName}' в позиции ({x}, {y}), цвет: {color}");
        }

        /// <summary>
        /// Создает подпись для локации
        /// </summary>
        private static void CreateLocationLabel(GameObject parent, string name, Color color)
        {
            // Создаем объект для текста
            var textObj = new GameObject("Label");
            textObj.transform.SetParent(parent.transform);
            textObj.transform.localPosition = new Vector3(0, 30f, 0); // Над кубом
            textObj.transform.localScale = new Vector3(10f, 10f, 10f);

            // Добавляем TextMeshPro
            var tm = textObj.AddComponent<TextMeshPro>();
            tm.text = name;
            tm.fontSize = 20f;
            tm.alignment = TextAlignmentOptions.Center;
            tm.enableAutoSizing = false;
            tm.color = Color.white;

            // Черная подложка для лучшей читаемости
            var bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bg.name = "LabelBackground";
            bg.transform.SetParent(textObj.transform);
            bg.transform.localPosition = new Vector3(0, 0, -0.1f);
            bg.transform.localRotation = Quaternion.identity;

            // Рассчитываем размер подложки
            tm.ForceMeshUpdate();
            var textSize = tm.GetRenderedValues(false);
            bg.transform.localScale = new Vector3(textSize.x + 2f, textSize.y + 1f, 1f);

            // Черный материал для подложки
            var bgRenderer = bg.GetComponent<Renderer>();
            var bgMat = new Material(Shader.Find("Unlit/Color"))
            {
                color = Color.black
            };
            bgRenderer.material = bgMat;
        }

        /// <summary>
        /// Возвращает цвет для биома (отличается от цвета на карте для контраста)
        /// </summary>
        private static Color GetBiomeColor(string biome)
        {
            return biome switch
            {
                "Water" => new Color(0.2f, 0.2f, 1f),        // Ярко-синий
                "Deep Water" => new Color(0, 0, 0.8f),       // Темно-синий
                "Sand" => new Color(1f, 0.9f, 0.7f),         // Светло-песочный
                "Grassland" => new Color(0.4f, 0.9f, 0.3f),  // Ярко-зеленый
                "Forest" => new Color(0.1f, 0.7f, 0.1f),     // Темно-зеленый
                "Jungle" => new Color(0, 0.6f, 0),           // Очень темный зеленый
                "MountainBase" => new Color(0.7f, 0.7f, 0.7f), // Светло-серый
                "MountainMid" => new Color(0.8f, 0.8f, 0.8f),  // Серый
                "MountainHigh" => new Color(0.9f, 0.9f, 0.9f), // Бело-серый
                "MountainPeak" => Color.white,
                _ => Color.magenta
            };
        }

        /// <summary>
        /// Возвращает биом для локации по имени
        /// </summary>
        private string GetLocationBiome(string locationName)
        {
            var loc = locations.Find(l => l.locationName == locationName);
            return loc?.biome ?? "Grassland";
        }

        /// <summary>
        /// Возвращает случайный цвет для маркера локации
        /// </summary>
        private static Color GetRandomColorForLocation(string locationName)
        {
            int hash = locationName.GetHashCode();
            return new Color(
                (hash & 0xFF) / 255f,
                (hash >> 8 & 0xFF) / 255f,
                (hash >> 16 & 0xFF) / 255f,
                1f
            );
        }

        /// <summary>
        /// Рассчитывает биомы чанков
        /// </summary>
        private string[,] CalculateChunkBiomes()
        {
            int chunksX = Mathf.CeilToInt((float)mapWidth / chunkSize);
            int chunksY = Mathf.CeilToInt((float)mapHeight / chunkSize);
            string[,] chunkBiome = new string[chunksX, chunksY];

            for (int cx = 0; cx < chunksX; cx++)
            {
                for (int cy = 0; cy < chunksY; cy++)
                {
                    var count = new Dictionary<string, int>();
                    for (int dx = 0; dx < chunkSize; dx++)
                    for (int dy = 0; dy < chunkSize; dy++)
                    {
                        int x = cx * chunkSize + dx, y = cy * chunkSize + dy;
                        if (x >= mapWidth || y >= mapHeight) continue;
                        string b = GetBiome(_heightMap[x, y], _biomeNoiseMap[x, y]);
                        if (b is "Water" or "Deep Water") continue;
                        count.TryAdd(b, 0);
                        count[b]++;
                    }

                    string mainBiome = null;
                    int max = -1;
                    foreach (KeyValuePair<string, int> kv in count.Where(kv => kv.Value > max))
                    {
                        max = kv.Value; mainBiome = kv.Key;
                    }
                    mainBiome ??= "Grassland";
                    chunkBiome[cx, cy] = mainBiome;
                }
            }

            return chunkBiome;
        }

        /// <summary>
        /// Рисует дорогу между двумя точками
        /// </summary>
        private void DrawSimpleRoad(Vector2 a, Vector2 b)
        {
            List<Vector2> open = new List<Vector2> { a };
            Dictionary<Vector2, Vector2> came = new Dictionary<Vector2, Vector2>();
            Dictionary<Vector2, float> g = new Dictionary<Vector2, float> { [a] = 0f };
            Dictionary<Vector2, float> f = new Dictionary<Vector2, float> { [a] = Vector2.Distance(a, b) };

            while (open.Count > 0)
            {
                var cur = open[0];
                foreach (var n in open.Where(n => f[n] < f[cur])) cur = n;
                if (cur == b) break;
                open.Remove(cur);

                foreach (var nb in new[] { Vector2.left, Vector2.right, Vector2.up, Vector2.down })
                {
                    var w = cur + nb;
                    int xi = Mathf.RoundToInt(w.x), yi = Mathf.RoundToInt(w.y);
                    if (xi < 0 || xi >= mapWidth || yi < 0 || yi >= mapHeight) continue;

                    float c = _heightMap[xi, yi] < 0.41f || _heightMap[xi, yi] > 0.79f ? float.MaxValue :
                        _heightMap[xi, yi] > 0.6f ? 2f : 1f;

                    if (Mathf.Approximately(c, float.MaxValue)) continue;
                    float ng = g[cur] + Vector2.Distance(cur, w) * c;

                    if (g.ContainsKey(w) && !(ng < g[w])) continue;
                    came[w] = cur;
                    g[w] = ng;
                    f[w] = ng + Vector2.Distance(w, b);
                    if (!open.Contains(w)) open.Add(w);
                }
            }

            // Сохраняем путь
            List<Vector2> roadPath = new List<Vector2>();
            if (roadPath == null) throw new ArgumentNullException(nameof(roadPath));
            var p = b;
            while (came.ContainsKey(p))
            {
                int xi = Mathf.RoundToInt(p.x), yi = Mathf.RoundToInt(p.y);
                _colourMap[yi * mapWidth + xi] = Color.red;
                roadPath.Add(p);
                p = came[p];
            }
            roadPath.Add(a);

            // UI маркер дороги
            if (locationUIManager)
            {
                locationUIManager.AddRoadMarker(a, b, Color.red);
            }

            // Перерисовываем карту
            var mapTexture = TextureGenerator.TextureFromColourMap(_colourMap, mapWidth, mapHeight);

            if (uiMapDisplay)
            {
                uiMapDisplay.DrawTexture(mapTexture);
            }
        }

        /// <summary>
        /// Добавляет локацию вручную
        /// </summary>
        /// <summary>
        /// Добавляет локацию вручную
        /// </summary>
        public void AddLocationManually(string locationName, string biome, bool autoPlace = true)
        {
            Debug.Log("=== MapGeneratorManual.AddLocationManually() ===");
            Debug.Log($"Параметры: name='{locationName}', biome='{biome}', autoPlace={autoPlace}");

            // Проверяем, нет ли уже такой локации
            bool locationExists = locations.Any(l => l.locationName == locationName);
            Debug.Log($"Локация с именем '{locationName}' уже существует? {locationExists}");

            if (locationExists)
            {
                Debug.LogError($"Локация с именем '{locationName}' уже существует");
                return;
            }

            var newLocation = new LocationDefinition
            {
                locationName = locationName,
                biome = biome,
                connectedLocations = new List<string>()
            };

            locations.Add(newLocation);
            Debug.Log($"Локация добавлена в список. Всего локаций: {locations.Count}");

            if (autoPlace && _heightMap != null && _biomeNoiseMap != null)
            {
                Debug.Log("Авторазмещение локации...");
                PlaceLocation(newLocation);

                // Обновляем текстуру карты
                var mapTexture = TextureGenerator.TextureFromColourMap(_colourMap, mapWidth, mapHeight);
                if (uiMapDisplay != null)
                {
                    uiMapDisplay.DrawTexture(mapTexture);
                    Debug.Log("Карта перерисована");
                }
                else
                {
                    Debug.LogWarning("uiMapDisplay не назначен, карта не перерисована");
                }
            }
            else
            {
                Debug.LogWarning("Авторазмещение отключено или карты не сгенерированы. " +
                    $"heightMap={_heightMap != null}, biomeNoiseMap={_biomeNoiseMap != null}");
            }

            Debug.Log($"=== Локация '{locationName}' добавлена (биом: {biome}) ===");
        }

        /// <summary>
        /// Подключает две локации дорогой
        /// </summary>
        public void ConnectLocations(string location1, string location2)
        {
            if (!_placedLocations.ContainsKey(location1) || !_placedLocations.ContainsKey(location2))
            {
                Debug.LogError("Одна из локаций не размещена на карте");
                return;
            }

            var loc1 = locations.Find(l => l.locationName == location1);
            var loc2 = locations.Find(l => l.locationName == location2);

            if (loc1 == null || loc2 == null) return;
            // Добавляем связь
            if (!loc1.connectedLocations.Contains(location2))
                loc1.connectedLocations.Add(location2);

            if (!loc2.connectedLocations.Contains(location1))
                loc2.connectedLocations.Add(location1);

            // Рисуем дорогу
            DrawSimpleRoad(_placedLocations[location1], _placedLocations[location2]);

            Debug.Log($"Локации '{location1}' и '{location2}' подключены");
        }

        /// <summary>
        /// Удаляет локацию
        /// </summary>
        public void RemoveLocation(string locationName)
        {
            var location = locations.Find(l => l.locationName == locationName);
            if (location == null) return;
            // Удаляем связи с этой локацией из других локаций
            foreach (var loc in locations)
            {
                loc.connectedLocations.Remove(locationName);
            }

            // Удаляем саму локацию
            locations.Remove(location);
            _placedLocations.Remove(locationName);

            // Удаляем 3D объект
            var obj = GameObject.Find(locationName);
            if (obj != null) Destroy(obj);

            // Удаляем UI маркер
            if (locationUIManager != null)
            {
                locationUIManager.RemoveLocationMarker(locationName);
            }

            Debug.Log($"Локация '{locationName}' удалена");
        }

        /// <summary>
        /// Определяет тип биома
        /// </summary>
        private static string GetBiome(float height, float noise)
        {
            return height switch
            {
                <= 0.4f => noise < 0.5f ? "Water" : "Deep Water",
                <= 0.44f => "Sand",
                <= 0.6f => noise < 0.5f ? "Grassland" : "Forest",
                <= 0.69f => noise < 0.5f ? "Forest" : "Jungle",
                <= 0.8f => "MountainBase",
                <= 0.85f => "MountainMid",
                <= 0.9f => "MountainHigh",
                _ => "MountainPeak"
            };
        }

        /// <summary>
        /// Возвращает цвет для биома
        /// </summary>
        private static Color GetColorForBiome(string biome)
        {
            return biome switch
            {
                "Water" => new Color(0.1f, 0.1f, 0.8f),
                "Deep Water" => new Color(0, 0, 0.5f),
                "Sand" => new Color(0.93f, 0.79f, 0.69f),
                "Grassland" => new Color(0.5f, 0.8f, 0.2f),
                "Forest" => new Color(0.1f, 0.5f, 0.1f),
                "Jungle" => new Color(0, 0.4f, 0),
                "MountainBase" => Color.gray,
                "MountainMid" => new Color(0.6f, 0.6f, 0.6f),
                "MountainHigh" => new Color(0.8f, 0.8f, 0.8f),
                "MountainPeak" => Color.white,
                _ => Color.magenta
            };
        }

        /// <summary>
        /// Сохраняет карту
        /// </summary>
        [ContextMenu("Save Map (PNG + JSON)")]
        public void SaveMapContext()
        {
            SaveMap(saveFileName);
        }

        public void SaveMap(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = "map_saved";
            string dir = Path.Combine(Application.dataPath, "SavedMaps");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            if (_colourMap == null || _colourMap.Length != mapWidth * mapHeight)
            {
                Debug.Log("Colour map отсутствует — регенерируем для сохранения.");
                if (_heightMap == null || _biomeNoiseMap == null)
                {
                    Debug.LogError("Нечего сохранять.");
                    return;
                }
                _colourMap = new Color[mapWidth * mapHeight];
                for (int y = 0; y < mapHeight; y++)
                for (int x = 0; x < mapWidth; x++)
                    _colourMap[y * mapWidth + x] = GetColorForBiome(GetBiome(_heightMap[x, y], _biomeNoiseMap[x, y]));
            }

            var tex = TextureGenerator.TextureFromColourMap(_colourMap, mapWidth, mapHeight);
            byte[] png = tex.EncodeToPNG();
            string pngPath = Path.Combine(dir, fileName + ".png");
            File.WriteAllBytes(pngPath, png);

            var data = new MapSaveData
            {
                mapWidth = mapWidth,
                mapHeight = mapHeight,
                compressedHeightMap = CompressFloatMap(_heightMap, mapWidth, mapHeight),
                compressedBiomeMap = CompressFloatMap(_biomeNoiseMap, mapWidth, mapHeight),
                locations = new List<LocationSaveData>(),
                seed = seed,
                noiseScale = noiseScale,
                octaves = octaves,
                persistance = persistance,
                lacunarity = lacunarity,
                noiseOffset = noiseOffset,
                chunkSize = chunkSize,
                drawChunkBorders = drawChunkBorders,
                showChunkBiomes = showChunkBiomes,
                highlightDominantBiome = highlightDominantBiome
            };

            foreach (var locDef in locations)
            {
                var lsd = new LocationSaveData
                {
                    locationName = locDef.locationName,
                    biome = locDef.biome,
                    connectedLocations = new List<string>(locDef.connectedLocations)
                };

                if (_placedLocations.TryGetValue(locDef.locationName, out var pos))
                {
                    lsd.x = Mathf.RoundToInt(pos.x);
                    lsd.y = Mathf.RoundToInt(pos.y);
                }
                else
                {
                    lsd.x = -1; lsd.y = -1;
                }

                data.locations.Add(lsd);
            }

            string json = JsonUtility.ToJson(data, false);
            string jsonPath = Path.Combine(dir, fileName + ".json");
            File.WriteAllText(jsonPath, json);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif

            Debug.Log($"Карта сохранена в {dir} как {fileName}.png и {fileName}.json");
        }

        /// <summary>
        /// Загружает карту
        /// </summary>
        [ContextMenu("Load Map (from SavedMaps)")]
        public void LoadMapContext()
        {
            LoadMap(saveFileName);
        }

        public void LoadMap(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = "map_saved";
            string dir = Path.Combine(Application.dataPath, "SavedMaps");
            string jsonPath = Path.Combine(dir, fileName + ".json");

            if (!File.Exists(jsonPath))
            {
                Debug.LogError($"Файл {jsonPath} не найден");
                return;
            }

            string json = File.ReadAllText(jsonPath);
            var data = JsonUtility.FromJson<MapSaveData>(json);
            if (data == null)
            {
                Debug.LogError("Не удалось распарсить JSON");
                return;
            }

            ClearMap();

            mapWidth = data.mapWidth;
            mapHeight = data.mapHeight;
            seed = data.seed;
            noiseScale = data.noiseScale;
            octaves = data.octaves;
            persistance = data.persistance;
            lacunarity = data.lacunarity;
            noiseOffset = data.noiseOffset;
            chunkSize = data.chunkSize;
            drawChunkBorders = data.drawChunkBorders;
            showChunkBiomes = data.showChunkBiomes;
            highlightDominantBiome = data.highlightDominantBiome;

            if (data.compressedHeightMap != null && data.compressedHeightMap.Length == mapWidth * mapHeight)
            {
                _heightMap = DecompressByteMap(data.compressedHeightMap, mapWidth, mapHeight);
            }
            else
            {
                Debug.LogWarning("Сжатые данные heightMap отсутствуют");
                _heightMap = new float[mapWidth, mapHeight];
            }

            if (data.compressedBiomeMap != null && data.compressedBiomeMap.Length == mapWidth * mapHeight)
            {
                _biomeNoiseMap = DecompressByteMap(data.compressedBiomeMap, mapWidth, mapHeight);
            }
            else
            {
                Debug.LogWarning("Сжатые данные biomeMap отсутствуют");
                _biomeNoiseMap = new float[mapWidth, mapHeight];
            }

            // Восстанавливаем локации
            if (data.locations != null)
            {
                locations.Clear();
                foreach (var ls in data.locations)
                {
                    var loc = new LocationDefinition
                    {
                        locationName = ls.locationName,
                        biome = ls.biome,
                        connectedLocations = new List<string>(ls.connectedLocations ?? new List<string>())
                    };
                    locations.Add(loc);

                    if (ls.x < 0 || ls.y < 0) continue;
                    _placedLocations[ls.locationName] = new Vector2(ls.x, ls.y);
                    CreateLocationObject(ls.locationName, ls.x, ls.y);
                }
            }

            // Восстанавливаем связи (дороги)
            if (data.locations != null)
            {
                foreach (var ls in data.locations)
                {
                    if (!_placedLocations.TryGetValue(ls.locationName, out var s)) continue;
                    if (ls.connectedLocations == null) continue;

                    foreach (string targetName in ls.connectedLocations)
                    {
                        if (_placedLocations.TryGetValue(targetName, out var e))
                        {
                            DrawSimpleRoad(s, e);
                        }
                    }
                }
            }

            // Отображаем карту
            _colourMap = new Color[mapWidth * mapHeight];
            for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)
                _colourMap[y * mapWidth + x] = GetColorForBiome(GetBiome(_heightMap[x, y], _biomeNoiseMap[x, y]));

            var mapTexture = TextureGenerator.TextureFromColourMap(_colourMap, mapWidth, mapHeight);

            if (uiMapDisplay)
            {
                uiMapDisplay.DrawTexture(mapTexture);
            }

            Debug.Log($"Карта загружена из {jsonPath}");
        }
    }
}