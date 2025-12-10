using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Компонент для создания приближенного вида карты с генерацией городов и зданий
/// </summary>
[ExecuteAlways]
public class MapZoom : MonoBehaviour
{
    [Header("Основной Plane")]
    public Renderer sourceRenderer;
    public int mainMapWidth = 300;
    public int mainMapHeight = 300;

    [Header("Настройки приближения")]
    public Vector2Int coordinate;
    public int zoomSize = 150;
    public int zoomScale = 16;
    [Tooltip("Сколько маленьких пикселей на 1 исходный")]
    public int subdivisions = 2;

    [Header("Разброс цвета")]
    public float maxVariation = 0.05f;

    [Header("Параметры города")]
    public int cityRadius = 6;
    public float cityNoiseScale = 0.35f;
    public float cityNoiseIntensity = 0.35f;
    public float maxBuildHeight = 0.7f;
    public Color cityFloorColor = new Color(0.6f, 0.5f, 0.4f);
    public Color cityWallColor = new Color(0.3f, 0.3f, 0.3f);
    public Color cityRoadColor = Color.grey;

    [Header("Параметры зданий")]
    public GameObject buildingPrefab;
    public float buildingChance = 0.4f;
    public int minBuildingHeight = 1;
    public int maxBuildingHeight = 4;
    public float buildingScale = 0.75f;

    [Header("Размеры зданий")]
    public Vector2 buildingWidthRange = new Vector2(1f, 3f);
    public Vector2 buildingDepthRange = new Vector2(1f, 3f);
    public float buildingMinSize = 1f;
    public float buildingMaxSize = 4f;
    public bool allowRectangularBuildings = true;

    [Header("Метод генерации зданий")]
    public BuildingGenerationMethod buildingGenerationMethod = BuildingGenerationMethod.SimpleCubes;

    [Header("Маркеры")]
    public GameObject locationPrefab;

    [Header("Визуализация")]
    public Renderer targetRenderer;
    public bool updateZoom = false;

    private Texture2D mainMapTexture;
    private Texture2D zoomedTexture;

    /// <summary>
    /// Обновляет приближенный вид карты при включенном флаге обновления
    /// </summary>
    private void Update()
    {
        if (updateZoom)
        {
            updateZoom = false;
            GenerateZoomedMap();
        }
    }

    /// <summary>
    /// Проверяет валидность позиции для размещения здания
    /// </summary>
    /// <param name="x">X координата позиции</param>
    /// <param name="y">Y координата позиции</param>
    /// <param name="baseSize">Размер базовой сетки</param>
    /// <param name="inside">Массив флагов внутренней области города</param>
    /// <param name="roads">Массив флагов дорог</param>
    /// <param name="walls">Массив флагов стен</param>
    /// <param name="occupied">Массив занятых позиций</param>
    /// <returns>True если позиция пригодна для строительства</returns>
    private bool IsPositionValid(int x, int y, int baseSize, bool[,] inside, bool[,] roads, bool[,] walls, bool[,] occupied)
    {
        if (x < 0 || y < 0 || x >= baseSize || y >= baseSize)
            return false;

        return inside[x, y] && !roads[x, y] && !walls[x, y] && !occupied[x, y];
    }

    /// <summary>
    /// Генерирует приближенный вид карты с городами, зданиями и дорогами
    /// </summary>
    void GenerateZoomedMap()
    {
        ClearAllBuildings();

        if (sourceRenderer == null || targetRenderer == null) return;
        if (!(sourceRenderer.sharedMaterial.mainTexture is Texture2D tex2D)) return;
        mainMapTexture = tex2D;

        var manual = FindFirstObjectByType<MapGeneratorManual>();

        int baseSize = zoomSize * 2 + 1;
        int subSize = baseSize * subdivisions;
        Color[] subColors = new Color[subSize * subSize];

        for (int by = 0; by < baseSize; by++)
            for (int bx = 0; bx < baseSize; bx++)
            {
                int mapX = coordinate.x - zoomSize + bx;
                int mapY = coordinate.y - zoomSize + by;
                Color c = Color.clear;
                if (mapX >= 0 && mapX < mainMapWidth && mapY >= 0 && mapY < mainMapHeight)
                {
                    c = mainMapTexture.GetPixel(mapX, mapY);
                    if (c == Color.black) c = new Color(0.1f, 0.1f, 0.1f);
                    else c = EnhanceByRelief(c, c.grayscale);
                }
                for (int sy = 0; sy < subdivisions; sy++)
                    for (int sx = 0; sx < subdivisions; sx++)
                    {
                        int x = bx * subdivisions + sx;
                        int y = by * subdivisions + sy;
                        float lg = c.grayscale * 0.9f + 0.05f;
                        subColors[y * subSize + x] = EnhanceByRelief(c, lg);
                    }
            }

        var pivots = new List<(string name, Vector2Int mapPos, Vector2Int bufPos)>();
        int half = zoomSize;

        var allObjects = GameObject.FindGameObjectsWithTag("GeneratedObject");
        Debug.Log($"Всего объектов с тегом GeneratedObject: {allObjects.Length}");

        foreach (var obj in allObjects)
        {
            if (!obj.name.Contains("City") && !obj.name.Contains("Town") && !obj.name.Contains("Village") &&
                !obj.name.Contains("Settlement") && !obj.name.Contains("Capital"))
            {
                continue;
            }

            if (obj.name.Contains("BiomeLabel") || obj.name.Contains("Building") || obj.name.Contains("Marker"))
            {
                continue;
            }

            Vector3 p = obj.transform.position;
            float mx = mainMapWidth * .5f - p.x / 10f;
            float my = mainMapHeight * .5f - p.z / 10f;
            int ix = Mathf.RoundToInt(mx), iy = Mathf.RoundToInt(my);

            bool isVisible = (Mathf.Abs(ix - coordinate.x) <= half && Mathf.Abs(iy - coordinate.y) <= half);

            Debug.Log($"Город: {obj.name}, Карта: ({ix}, {iy}), Видим: {isVisible}");

            if (isVisible)
            {
                int bx = ix - (coordinate.x - zoomSize);
                int by = iy - (coordinate.y - zoomSize);

                if (bx >= 0 && bx < (zoomSize * 2 + 1) && by >= 0 && by < (zoomSize * 2 + 1))
                {
                    pivots.Add((obj.name, new Vector2Int(ix, iy), new Vector2Int(bx, by)));
                    Debug.Log($"  ДОБАВЛЕН в pivots: ({bx}, {by})");
                }
                else
                {
                    Debug.Log($"  ПРОПУЩЕН из-за некорректных координат: ({bx}, {by})");
                }
            }
        }

        Debug.Log($"Найдено городов для генерации зданий: {pivots.Count}");

        Transform buildingsContainer = new GameObject("BuildingsContainer").transform;
        buildingsContainer.parent = targetRenderer.transform;

        foreach (var (name, mapPos, bufPos) in pivots)
        {
            bool[,] inside = new bool[baseSize, baseSize];
            bool[,] wall = new bool[baseSize, baseSize];
            bool[,] roads = new bool[baseSize, baseSize];
            bool[,] buildingOccupied = new bool[baseSize, baseSize];

            for (int by = 0; by < baseSize; by++)
                for (int bx = 0; bx < baseSize; bx++)
                {
                    float dx = bx - bufPos.x, dy = by - bufPos.y;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float nx = (bx + mapPos.x) * cityNoiseScale;
                    float ny = (by + mapPos.y) * cityNoiseScale;
                    float noise = Mathf.PerlinNoise(nx, ny) * 2f - 1f;
                    float dynR = cityRadius * (1f + noise * cityNoiseIntensity);
                    int mx = coordinate.x - zoomSize + bx;
                    int my = coordinate.y - zoomSize + by;
                    float h = 0f;
                    if (mx >= 0 && mx < mainMapWidth && my >= 0 && my < mainMapHeight)
                        h = mainMapTexture.GetPixel(mx, my).grayscale;
                    bool canBuild = h <= maxBuildHeight && h > 0.05f;
                    if (canBuild && d <= dynR)
                        inside[bx, by] = true;
                }

            for (int by = 0; by < baseSize; by++)
                for (int bx = 0; bx < baseSize; bx++)
                {
                    if (inside[bx, by]) continue;
                    for (int oy = -1; oy <= 1; oy++)
                        for (int ox = -1; ox <= 1; ox++)
                        {
                            int nx = bx + ox, ny = by + oy;
                            if (nx >= 0 && nx < baseSize && ny >= 0 && ny < baseSize && inside[nx, ny])
                                wall[bx, by] = true;
                        }
                }

            for (int roadCount = 0; roadCount < 3; roadCount++)
            {
                float angle = Random.value * Mathf.PI * 2f;
                int len = cityRadius;
                for (int t = 0; t < len; t++)
                {
                    int rx = bufPos.x + Mathf.RoundToInt(Mathf.Cos(angle) * t);
                    int ry = bufPos.y + Mathf.RoundToInt(Mathf.Sin(angle) * t);
                    if (rx >= 0 && rx < baseSize && ry >= 0 && ry < baseSize && inside[rx, ry])
                        roads[rx, ry] = true;
                }
            }

            for (int crossCount = 0; crossCount < 2; crossCount++)
            {
                int crossX = bufPos.x + Random.Range(-cityRadius / 2, cityRadius / 2);
                int crossY = bufPos.y + Random.Range(-cityRadius / 2, cityRadius / 2);
                for (int x = Mathf.Max(0, crossX - cityRadius / 2); x < Mathf.Min(baseSize, crossX + cityRadius / 2); x++)
                {
                    if (x >= 0 && x < baseSize && crossY >= 0 && crossY < baseSize && inside[x, crossY])
                        roads[x, crossY] = true;
                }
                for (int y = Mathf.Max(0, crossY - cityRadius / 2); y < Mathf.Min(baseSize, crossY + cityRadius / 2); y++)
                {
                    if (crossX >= 0 && crossX < baseSize && y >= 0 && y < baseSize && inside[crossX, y])
                        roads[crossX, y] = true;
                }
            }

            if (buildingPrefab != null)
            {
                List<Vector2Int> potentialPositions = new List<Vector2Int>();
                for (int by = 0; by < baseSize; by++)
                    for (int bx = 0; bx < baseSize; bx++)
                    {
                        if (IsPositionValid(bx, by, baseSize, inside, roads, wall, buildingOccupied))
                        {
                            potentialPositions.Add(new Vector2Int(bx, by));
                        }
                    }

                potentialPositions = potentialPositions.OrderBy(x => UnityEngine.Random.value).ToList();

                foreach (var pos in potentialPositions)
                {
                    int bx = pos.x;
                    int by = pos.y;

                    if (Random.value < buildingChance)
                    {
                        switch (buildingGenerationMethod)
                        {
                            case BuildingGenerationMethod.AdvancedWithSizes:
                                SpawnBuildingWithSize(bx, by, baseSize, buildingsContainer, inside, roads, buildingOccupied);
                                break;

                            case BuildingGenerationMethod.SimpleCubes:
                                SpawnBuilding(bx, by, baseSize, buildingsContainer);
                                break;
                        }
                    }
                }
            }

            for (int by = 0; by < baseSize; by++)
                for (int bx = 0; bx < baseSize; bx++)
                {
                    bool drawWall = wall[bx, by];
                    bool drawInside = inside[bx, by] && !roads[bx, by];
                    bool drawRoad = roads[bx, by];
                    if (!drawWall && !drawInside && !drawRoad) continue;
                    Color baseC = drawRoad ? cityRoadColor :
                                 drawInside ? cityFloorColor : cityWallColor;
                    for (int sy = 0; sy < subdivisions; sy++)
                        for (int sx = 0; sx < subdivisions; sx++)
                        {
                            int x = bx * subdivisions + sx;
                            int y = by * subdivisions + sy;
                            float lg = baseC.grayscale * 0.9f + 0.05f;
                            subColors[y * subSize + x] = EnhanceByRelief(baseC, lg);
                        }
                }
        }

        if (zoomedTexture == null ||
            zoomedTexture.width != subSize ||
            zoomedTexture.height != subSize)
        {
            zoomedTexture = new Texture2D(subSize, subSize)
            { filterMode = FilterMode.Point };
        }
        zoomedTexture.SetPixels(subColors);
        zoomedTexture.Apply();

        var finalTex = ScaleTexture(zoomedTexture, subSize * zoomScale, subSize * zoomScale);

        finalTex.filterMode = FilterMode.Point;
        targetRenderer.sharedMaterial.mainTexture = finalTex;

        SpawnMarkers(baseSize);
    }

    /// <summary>
    /// Создает простое здание кубической формы в указанной позиции
    /// </summary>
    /// <param name="bx">X координата в базовой сетке</param>
    /// <param name="by">Y координата в базовой сетке</param>
    /// <param name="baseSize">Размер базовой сетки</param>
    /// <param name="container">Родительский контейнер для здания</param>
    private void SpawnBuilding(int bx, int by, int baseSize, Transform container)
    {
        if (buildingPrefab == null) return;

        Transform tt = targetRenderer.transform;

        float mapX = coordinate.x - zoomSize + bx + 0.5f;
        float mapY = coordinate.y - zoomSize + by + 0.5f;

        float halfSize = zoomSize;
        float u = 1f - (mapX - (coordinate.x - halfSize)) / baseSize;
        float v = 1f - (mapY - (coordinate.y - halfSize)) / baseSize;

        float wS = 10f * tt.localScale.x;
        Vector3 c = tt.position;
        Vector3 r = tt.right;
        Vector3 fwd = tt.forward;
        Vector3 wp = c + r * ((u - 0.5f) * wS) + fwd * ((v - 0.5f) * wS);

        GameObject building = Instantiate(buildingPrefab, wp + Vector3.up * 0.01f, Quaternion.identity, container);
        building.name = "CityBuilding";

        float cellWorldSize = 10f * tt.localScale.x / baseSize;
        float height = Random.Range(minBuildingHeight, maxBuildingHeight) * 10;
        building.transform.localScale = new Vector3(
            buildingScale * cellWorldSize,
            height,
            buildingScale * cellWorldSize
        );
    }

    /// <summary>
    /// Создает здание с учетом доступного пространства и случайных размеров
    /// </summary>
    /// <param name="startX">Начальная X координата здания</param>
    /// <param name="startY">Начальная Y координата здания</param>
    /// <param name="baseSize">Размер базовой сетки</param>
    /// <param name="container">Родительский контейнер для здания</param>
    /// <param name="inside">Массив флагов внутренней области города</param>
    /// <param name="roads">Массив флагов дорог</param>
    /// <param name="occupied">Массив занятых позиций</param>
    private void SpawnBuildingWithSize(int startX, int startY, int baseSize, Transform container,
                                      bool[,] inside, bool[,] roads, bool[,] occupied)
    {
        if (buildingPrefab == null) return;

        Transform tt = targetRenderer.transform;

        int maxWidth = 1;
        int maxDepth = 1;

        for (int w = 1; w <= buildingMaxSize; w++)
        {
            int x = startX + w - 1;
            if (x >= baseSize || !inside[x, startY] || roads[x, startY] || occupied[x, startY])
                break;
            maxWidth = w;
        }

        for (int d = 1; d <= buildingMaxSize; d++)
        {
            int y = startY + d - 1;
            if (y >= baseSize || !inside[startX, y] || roads[startX, y] || occupied[startX, y])
                break;
            maxDepth = d;
        }

        if (maxWidth < buildingMinSize || maxDepth < buildingMinSize)
            return;

        int width = Mathf.RoundToInt(Random.Range(buildingMinSize, Mathf.Min(maxWidth, buildingMaxSize)));
        int depth = Mathf.RoundToInt(Random.Range(buildingMinSize, Mathf.Min(maxDepth, buildingMaxSize)));

        if (!allowRectangularBuildings)
        {
            int minSize = Mathf.Min(width, depth);
            width = minSize;
            depth = minSize;
        }

        bool canPlace = true;
        for (int y = 0; y < depth; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int checkX = startX + x;
                int checkY = startY + y;

                if (checkX >= baseSize || checkY >= baseSize ||
                    !inside[checkX, checkY] || roads[checkX, checkY] || occupied[checkX, checkY])
                {
                    canPlace = false;
                    break;
                }
            }
            if (!canPlace) break;
        }

        if (!canPlace) return;

        for (int y = 0; y < depth; y++)
        {
            for (int x = 0; x < width; x++)
            {
                occupied[startX + x, startY + y] = true;
            }
        }

        float centerX = startX + width * 0.5f;
        float centerY = startY + depth * 0.5f;

        float mapX = coordinate.x - zoomSize + centerX;
        float mapY = coordinate.y - zoomSize + centerY;

        float halfSize = zoomSize;
        float u = 1f - (mapX - (coordinate.x - halfSize)) / baseSize;
        float v = 1f - (mapY - (coordinate.y - halfSize)) / baseSize;

        float wS = 10f * tt.localScale.x;
        Vector3 c = tt.position;
        Vector3 r = tt.right;
        Vector3 fwd = tt.forward;
        Vector3 wp = c + r * ((u - 0.5f) * wS) + fwd * ((v - 0.5f) * wS);

        GameObject building = Instantiate(buildingPrefab, wp + Vector3.up * 0.01f, Quaternion.identity, container);
        building.name = $"CityBuilding_{startX}_{startY}_{width}x{depth}";

        float cellWorldSize = 10f * tt.localScale.x / baseSize;
        float height = Random.Range(minBuildingHeight, maxBuildingHeight) * 10;

        building.transform.localScale = new Vector3(
            width * cellWorldSize * 0.9f,
            height,
            depth * cellWorldSize * 0.9f
        );
    }

    /// <summary>
    /// Очищает все сгенерированные здания и маркеры на карте
    /// </summary>
    private void ClearAllBuildings()
    {
        GameObject container = GameObject.Find("BuildingsContainer");
        if (container != null)
        {
            DestroyImmediate(container);
        }

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("CityBuilding") ||
                obj.name.StartsWith("ZoomBuilding") ||
                obj.name.StartsWith("MainBuilding") ||
                obj.name.Contains("Marker_City") ||
                obj.name.Contains("Marker_Town"))
            {
                DestroyImmediate(obj);
            }
        }

        container = GameObject.Find("ZoomBuildingsContainer");
        if (container != null) DestroyImmediate(container);
    }

    /// <summary>
    /// Улучшает цвет с учетом рельефа и добавляет случайные вариации
    /// </summary>
    /// <param name="bc">Базовый цвет</param>
    /// <param name="gray">Значение серого для расчета рельефа</param>
    /// <returns>Улучшенный цвет с вариациями</returns>
    Color EnhanceByRelief(Color bc, float gray)
    {
        float bs = (gray - .5f) * 2f;
        float v = Random.Range(-maxVariation, maxVariation);
        if (bc.g > bc.r && bc.g > bc.b) v *= 1.5f;
        float shift = v + bs * maxVariation;
        return new Color(
            Mathf.Clamp01(bc.r + shift),
            Mathf.Clamp01(bc.g + shift),
            Mathf.Clamp01(bc.b + shift),
            1f
        );
    }

    /// <summary>
    /// Создает маркеры локаций на приближенной карте
    /// </summary>
    /// <param name="baseSize">Размер базовой сетки для позиционирования</param>
    void SpawnMarkers(int baseSize)
    {
        Transform tt = targetRenderer.transform;
        for (int i = tt.childCount - 1; i >= 0; i--)
            if (tt.GetChild(i).name != "BuildingsContainer")
                DestroyImmediate(tt.GetChild(i).gameObject);

        if (locationPrefab == null) return;

        float wS = 10f * tt.localScale.x;
        int half = zoomSize;

        foreach (var obj in GameObject.FindGameObjectsWithTag("GeneratedObject"))
        {
            if (!obj.name.Contains("City") && !obj.name.Contains("Town") && !obj.name.Contains("Village") &&
                !obj.name.Contains("Settlement") && !obj.name.Contains("Capital"))
            {
                continue;
            }

            if (obj.name.Contains("BiomeLabel") || obj.name.Contains("Building") || obj.name.Contains("Marker"))
            {
                continue;
            }

            Vector3 p = obj.transform.position;
            float mx = mainMapWidth * .5f - p.x / 10f;
            float my = mainMapHeight * .5f - p.z / 10f;
            Vector2 mf = new Vector2(mx, my);
            Vector2Int mm = new Vector2Int(
                Mathf.RoundToInt(mx),
                Mathf.RoundToInt(my)
            );

            if (Mathf.Abs(mm.x - coordinate.x) > half || Mathf.Abs(mm.y - coordinate.y) > half)
            {
                continue;
            }

            float adjustedX = mf.x + 0.5f;
            float adjustedY = mf.y + 0.5f;

            float u = 1f - (adjustedX - (coordinate.x - half)) / baseSize;
            float v = 1f - (adjustedY - (coordinate.y - half)) / baseSize;

            Vector3 c = tt.position;
            Vector3 r = tt.right;
            Vector3 fwd = tt.forward;
            Vector3 wp = c + r * ((u - 0.5f) * wS) + fwd * ((v - 0.5f) * wS);
            var m = Instantiate(locationPrefab, wp + tt.up * 0.5f, obj.transform.rotation, tt);
            m.name = "Marker_" + obj.name;
            float sf = 1f / (subdivisions * zoomScale);
            m.transform.localScale = Vector3.one * sf;
            Vector3 scale = m.transform.localScale;
            scale.z = 25f;
            m.transform.localScale = scale;
        }
    }

    /// <summary>
    /// Масштабирует текстуру до указанных размеров с использованием точечной фильтрации
    /// </summary>
    /// <param name="src">Исходная текстура для масштабирования</param>
    /// <param name="w">Целевая ширина текстуры</param>
    /// <param name="h">Целевая высота текстуры</param>
    /// <returns>Масштабированная текстура</returns>
    Texture2D ScaleTexture(Texture2D src, int w, int h)
    {
        var dst = new Texture2D(w, h) { filterMode = FilterMode.Point };
        Color[] sp = src.GetPixels();
        Color[] dp = new Color[w * h];
        int sw = src.width, sh = src.height;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                dp[y * w + x] = sp[(y * sh / h) * sw + (x * sw / w)];
        dst.SetPixels(dp);
        dst.Apply();
        return dst;
    }
}

/// <summary>
/// Методы генерации зданий для приближенного вида карты
/// </summary>
public enum BuildingGenerationMethod
{
    SimpleCubes,        // Простой метод с кубиками одинакового размера
    AdvancedWithSizes  // Метод с разными размерами зданий
}