using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class MapZoom : MonoBehaviour
{
    [Header("Основной Plane")]
    public Renderer sourceRenderer;
    public int mainMapWidth = 256;
    public int mainMapHeight = 256;

    [Header("Настройки приближения")]
    public Vector2Int coordinate;
    public int zoomSize = 32;
    public int zoomScale = 4;
    [Tooltip("Сколько маленьких пикселей на 1 исходный")]
    public int subdivisions = 2;

    [Header("Префабы построек")]
    public GameObject buildingPrefab;
    public GameObject mainBuildingPrefab;

    [Header("Второй уровень приближения")]
    public bool useSecondZoom = false;
    public Vector2Int secondZoomOffset;
    public int secondZoomSize = 16;
    public int secondZoomScale = 2;

    [Header("Разброс цвета")]
    public float maxVariation = 0.1f;

    [Header("Параметры города")]
    public int cityRadius = 5;
    public float cityNoiseScale = 0.2f;
    public float cityNoiseIntensity = 0.5f;
    public float maxBuildHeight = 0.85f;
    public Color cityFloorColor = new Color(0.6f, 0.5f, 0.4f);
    public Color cityWallColor = new Color(0.3f, 0.3f, 0.3f);
    public Color cityRoadColor = Color.grey;

    [Header("Маркеры")]
    public GameObject locationPrefab;

    [Header("Визуализация")]
    public Renderer targetRenderer;
    public bool updateZoom = false;

    private Texture2D mainMapTexture;
    private Texture2D zoomedTexture;

    private void Update()
    {
        if (updateZoom)
        {
            updateZoom = false;
            /*            UpdateCoordinateToNearestObject();*/
            GenerateZoomedMap();
        }
    }

    private void UpdateCoordinateToNearestObject()
    {
        if (!useSecondZoom) return;

        GameObject[] objs = GameObject.FindGameObjectsWithTag("GeneratedObject");
        if (objs.Length == 0) return;

        Vector2Int currentCoord = coordinate;
        float minDist = float.MaxValue;
        Vector2Int nearestCoord = currentCoord;

        foreach (GameObject obj in objs)
        {
            if (obj.name.Contains("BiomeLabel")) continue;

            Vector3 pos = obj.transform.position;
            float mx = mainMapWidth * .5f - pos.x / 10f;
            float my = mainMapHeight * .5f - pos.z / 10f;
            Vector2Int objCoord = new Vector2Int(Mathf.RoundToInt(mx), Mathf.RoundToInt(my));

            float dist = Vector2Int.Distance(currentCoord, objCoord);
            if (dist < minDist)
            {
                minDist = dist;
                nearestCoord = objCoord;
            }
        }

        coordinate = nearestCoord;
    }
    void GenerateZoomedMap()
    {
        if (sourceRenderer == null || targetRenderer == null) return;
        if (!(sourceRenderer.sharedMaterial.mainTexture is Texture2D tex2D)) return;
        mainMapTexture = tex2D;

        var manual = FindObjectOfType<MapGeneratorManual>();
        var defs = manual != null ? manual.locations : new List<LocationDefinition>();

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
        foreach (var obj in GameObject.FindGameObjectsWithTag("GeneratedObject"))
        {
            if (obj.name.Contains("BiomeLabel")) continue;
            Vector3 p = obj.transform.position;
            float mx = mainMapWidth * .5f - p.x / 10f;
            float my = mainMapHeight * .5f - p.z / 10f;
            int ix = Mathf.RoundToInt(mx), iy = Mathf.RoundToInt(my);
            if (Mathf.Abs(ix - coordinate.x) <= half && Mathf.Abs(iy - coordinate.y) <= half)
            {
                int bx = ix - (coordinate.x - zoomSize);
                int by = iy - (coordinate.y - zoomSize);
                pivots.Add((obj.name, new Vector2Int(ix, iy), new Vector2Int(bx, by)));
            }
        }

        foreach (var (name, mapPos, bufPos) in pivots)
        {
            bool[,] inside = new bool[baseSize, baseSize];
            bool[,] wall = new bool[baseSize, baseSize];
            bool[,] roads = new bool[baseSize, baseSize];

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

            float angle = Random.value * Mathf.PI * 2f;
            int len = cityRadius;
            for (int t = 0; t < len; t++)
            {
                int rx = bufPos.x + Mathf.RoundToInt(Mathf.Cos(angle) * t);
                int ry = bufPos.y + Mathf.RoundToInt(Mathf.Sin(angle) * t);
                if (rx >= 0 && rx < baseSize && ry >= 0 && ry < baseSize && inside[rx, ry])
                    roads[rx, ry] = true;
            }

            var def = defs.Find(l => l.locationName == name);
            if (def != null)
            {
                foreach (var otherName in def.connectedLocations)
                {
                    var other = pivots.Find(p => p.name == otherName);
                    if (other.name != null)
                    {
                        int x0 = bufPos.x, y0 = bufPos.y;
                        int x1 = other.bufPos.x, y1 = other.bufPos.y;
                        int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
                        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
                        int err = dx - dy;
                        while (true)
                        {
                            if (x0 >= 0 && x0 < baseSize && y0 >= 0 && y0 < baseSize) roads[x0, y0] = true;
                            if (x0 == x1 && y0 == y1) break;
                            int e2 = 2 * err;
                            if (e2 > -dy) { err -= dy; x0 += sx; }
                            if (e2 < dx) { err += dx; y0 += sy; }
                        }
                    }
                }
            }

            for (int by = 0; by < baseSize; by++)
                for (int bx = 0; bx < baseSize; bx++)
                {
                    bool drawWall = wall[bx, by];
                    bool drawInside = inside[bx, by];
                    bool drawRoad = roads[bx, by];

                    if (!drawWall && !drawInside && !drawRoad) continue;

                    Color baseC = drawRoad
                        ? cityRoadColor
                        : (drawInside ? cityFloorColor : cityWallColor);

                    for (int sy = 0; sy < subdivisions; sy++)
                        for (int sx = 0; sx < subdivisions; sx++)
                        {
                            int x = bx * subdivisions + sx;
                            int y = by * subdivisions + sy;
                            subColors[y * subSize + x] = EnhanceByRelief(baseC, baseC.grayscale);
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

        Texture2D finalTex;

        if (useSecondZoom)
        {
            int sz = secondZoomSize * 2 + 1;
            // получаем базовую обрезанную текстуру
            Color[] basePixels = new Color[sz * sz];
            int center = subSize / 2;

            for (int y = 0; y < sz; y++)
                for (int x = 0; x < sz; x++)
                {
                    int srcX = center - secondZoomSize + secondZoomOffset.x + x;
                    int srcY = center - secondZoomSize + secondZoomOffset.y + y;
                    if (srcX >= 0 && srcX < subSize && srcY >= 0 && srcY < subSize)
                        basePixels[y * sz + x] = zoomedTexture.GetPixel(srcX, srcY);
                    else
                        basePixels[y * sz + x] = Color.clear;
                }

            // теперь разбиваем каждый пиксель на secondZoomScale? мелких
            int outSize = sz * secondZoomScale;
            Color[] outPixels = new Color[outSize * outSize];
            for (int y = 0; y < sz; y++)
                for (int x = 0; x < sz; x++)
                {
                    Color c = basePixels[y * sz + x];
                    // для каждого субпикселя
                    for (int sy = 0; sy < secondZoomScale; sy++)
                        for (int sx = 0; sx < secondZoomScale; sx++)
                        {
                            int ox = x * secondZoomScale + sx;
                            int oy = y * secondZoomScale + sy;
                            float lg = c.grayscale * 0.9f + 0.05f;
                            outPixels[oy * outSize + ox] = EnhanceByRelief(c, lg);
                        }
                }

            // создаём и заполняем итоговую текстуру
            Texture2D finalCropped = new Texture2D(outSize, outSize) { filterMode = FilterMode.Point };
            finalCropped.SetPixels(outPixels);
            finalCropped.Apply();
            finalTex = finalCropped;
        }

        else
        {
            finalTex = ScaleTexture(zoomedTexture, subSize * zoomScale, subSize * zoomScale);
        }

        finalTex.filterMode = FilterMode.Point;
        targetRenderer.material.mainTexture = finalTex;

        SpawnMarkers(baseSize);
    }

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

    void SpawnMarkers(int baseSize)
    {
        Transform tt = targetRenderer.transform;
        for (int i = tt.childCount - 1; i >= 0; i--)
            DestroyImmediate(tt.GetChild(i).gameObject);
        if (locationPrefab == null) return;

        float wS = 10f * tt.localScale.x;
        int half = zoomSize;
        foreach (var obj in GameObject.FindGameObjectsWithTag("GeneratedObject"))
        {
            if (obj.name.Contains("BiomeLabel")) continue;
            Vector3 p = obj.transform.position;
            float mx = mainMapWidth * .5f - p.x / 10f;
            float my = mainMapHeight * .5f - p.z / 10f;
            Vector2 mf = new Vector2(mx, my);
            Vector2Int mm = new Vector2Int(
                Mathf.RoundToInt(mx),
                Mathf.RoundToInt(my)
            );
            if (Mathf.Abs(mm.x - coordinate.x) > half || Mathf.Abs(mm.y - coordinate.y) > half)
                continue;

            float u, v;

            if (useSecondZoom)
            {
                int subSize = baseSize * subdivisions;
                int sz = secondZoomSize * 2 + 1;

                // Позиция объекта в субпикселях
                float subX = (mf.x - (coordinate.x - zoomSize)) * subdivisions;
                float subY = (mf.y - (coordinate.y - zoomSize)) * subdivisions;

                // Центр второго зума в субпикселях
                float secondCenterX = subSize / 2 + secondZoomOffset.x;
                float secondCenterY = subSize / 2 + secondZoomOffset.y;

                float secondHalfSize = sz / 2f;

                u = 1f - (subX - (secondCenterX - secondHalfSize)) / (sz - 1f);
                v = 1f - (subY - (secondCenterY - secondHalfSize)) / (sz - 1f);
            }
            else
            {
                u = 1f - (mf.x - (coordinate.x - half)) / (baseSize - 1f);
                v = 1f - (mf.y - (coordinate.y - half)) / (baseSize - 1f);
            }


            Vector3 c = tt.position;
            Vector3 r = tt.right;
            Vector3 fwd = tt.forward;
            Vector3 wp = c + r * ((u - 0.5f) * wS) + fwd * ((v - 0.5f) * wS);

            var m = Instantiate(locationPrefab, wp + tt.up * 0.5f, obj.transform.rotation, tt);
            m.name = obj.name;
            float sf = 1f / (subdivisions * zoomScale);
            if (useSecondZoom)
                sf *= (float)(zoomSize * 2 + 1) / (secondZoomSize * 2 + 1);

            m.transform.localScale = Vector3.one * sf;
        }
    }

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