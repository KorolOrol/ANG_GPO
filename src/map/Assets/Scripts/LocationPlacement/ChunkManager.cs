using System.Collections.Generic;
using UnityEngine;

public class ChunkManager
{
    public class Chunk
    {
        public Vector2Int start, end;      // диапазон пикселей на карте
        public Vector2 center;         // центр чанка (в координатах массива)
        public string biome;          // доминирующий биом
    }

    private int mapW, mapH, chunkSize;
    private List<Chunk> allChunks = new List<Chunk>();

    public ChunkManager(int mapWidth, int mapHeight, int chunkSize)
    {
        this.mapW = mapWidth;
        this.mapH = mapHeight;
        this.chunkSize = chunkSize;
        GenerateChunks();
    }

    private void GenerateChunks()
    {
        int cols = Mathf.CeilToInt((float)mapW / chunkSize);
        int rows = Mathf.CeilToInt((float)mapH / chunkSize);

        for (int j = 0; j < rows; j++)
            for (int i = 0; i < cols; i++)
            {
                var c = new Chunk();
                c.start = new Vector2Int(i * chunkSize, j * chunkSize);
                c.end = new Vector2Int(
                    Mathf.Min((i + 1) * chunkSize, mapW) - 1,
                    Mathf.Min((j + 1) * chunkSize, mapH) - 1);
                c.center = new Vector2(
                    (c.start.x + c.end.x) * 0.5f,
                    (c.start.y + c.end.y) * 0.5f);
                allChunks.Add(c);
            }
    }

    // Строит словарь: biomestring → список чанков этого биома
    public Dictionary<string, List<Chunk>> GetChunksByBiome(string[,] biomeMap)
    {
        var dict = new Dictionary<string, List<Chunk>>();
        foreach (var c in allChunks)
        {
            // подсчитать, какой биом встречается в чанкe чаще всего
            var count = new Dictionary<string, int>();
            for (int y = c.start.y; y <= c.end.y; y++)
                for (int x = c.start.x; x <= c.end.x; x++)
                {
                    var b = biomeMap[x, y];
                    if (!count.ContainsKey(b)) count[b] = 0;
                    count[b]++;
                }

            string dom = null; int best = 0;
            foreach (var kv in count)
                if (kv.Value > best) { best = kv.Value; dom = kv.Key; }

            c.biome = dom;
            if (!dict.ContainsKey(dom)) dict[dom] = new List<Chunk>();
            dict[dom].Add(c);
        }
        return dict;
    }
}
