using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapScripts.LocationPlacement
{
    public class ChunkManager
    {
        public class Chunk
        {
            public Vector2Int Start, End;
            public Vector2 Center;
            public string Biome;
        }

        private readonly int _mapW;
        private readonly int _mapH;
        private readonly int _chunkSize;
        private readonly List<Chunk> _allChunks = new List<Chunk>();

        /// <summary>
        /// Инициализирует менеджер чанков с заданными размерами карты и чанков
        /// </summary>
        /// <param name="mapWidth">Ширина карты в тайлах</param>
        /// <param name="mapHeight">Высота карты в тайлах</param>
        /// <param name="chunkSize">Размер чанка в тайлах</param>
        public ChunkManager(int mapWidth, int mapHeight, int chunkSize)
        {
            _mapW = mapWidth;
            _mapH = mapHeight;
            _chunkSize = chunkSize;
            GenerateChunks();
        }

        /// <summary>
        /// Разделяет карту на чанки заданного размера
        /// </summary>
        private void GenerateChunks()
        {
            int cols = Mathf.CeilToInt((float)_mapW / _chunkSize);
            int rows = Mathf.CeilToInt((float)_mapH / _chunkSize);

            for (int j = 0; j < rows; j++)
            for (int i = 0; i < cols; i++)
            {
                var c = new Chunk
                {
                    Start = new Vector2Int(i * _chunkSize, j * _chunkSize),
                    End = new Vector2Int(
                        Mathf.Min((i + 1) * _chunkSize, _mapW) - 1,
                        Mathf.Min((j + 1) * _chunkSize, _mapH) - 1)
                };
                c.Center = new Vector2(
                    (c.Start.x + c.End.x) * 0.5f,
                    (c.Start.y + c.End.y) * 0.5f);
                _allChunks.Add(c);
            }
        }

        /// <summary>
        /// Строит словарь: biomestring → список чанков этого биома
        /// </summary>
        /// <param name="biomeMap">Двумерный массив с информацией о биомах для каждого тайла</param>
        /// <returns>Словарь, группирующий чанки по типам биомов</returns>
        public Dictionary<string, List<Chunk>> GetChunksByBiome(string[,] biomeMap)
        {
            Dictionary<string, List<Chunk>> dict = new Dictionary<string, List<Chunk>>();
            foreach (var c in _allChunks)
            {
                // подсчитать, какой биом встречается в чанкe чаще всего
                Dictionary<string, int> count = new Dictionary<string, int>();
                for (int y = c.Start.y; y <= c.End.y; y++)
                for (int x = c.Start.x; x <= c.End.x; x++)
                {
                    string b = biomeMap[x, y];
                    count.TryAdd(b, 0);
                    count[b]++;
                }

                string dom = null; int best = 0;
                foreach (KeyValuePair<string, int> kv in count.Where(kv => kv.Value > best))
                {
                    best = kv.Value; dom = kv.Key;
                }

                c.Biome = dom;
                if (dom != null && !dict.ContainsKey(dom)) dict[dom] = new List<Chunk>();
                if (dom != null) dict[dom].Add(c);
            }
            return dict;
        }
    }
}
