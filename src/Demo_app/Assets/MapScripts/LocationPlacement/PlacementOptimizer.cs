using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapScripts.LocationPlacement
{
    /// <summary>
    /// Оптимизатор размещения локаций на карте мира
    /// </summary>
    public static class PlacementOptimizer
    {
        /// <summary>
        /// Оптимизирует размещение локаций на карте с учетом биомов и дорожных соединений
        /// </summary>
        /// <param name="locations">Список локаций для размещения</param>
        /// <param name="biomeMap">Карта биомов игрового мира</param>
        /// <param name="chunkSize">Размер чанка для разбиения карты</param>
        /// <param name="iterations">Количество итераций оптимизации</param>
        /// <returns>Список локаций с назначенными позициями на карте</returns>
        public static List<LocationData> Optimize(
            List<LocationData> locations,
            string[,] biomeMap,
            int chunkSize,
            int iterations = 5000)
        {
            int w = biomeMap.GetLength(0), h = biomeMap.GetLength(1);
            var cm = new ChunkManager(w, h, chunkSize);
            Dictionary<string, List<ChunkManager.Chunk>> chunksByBiome = cm.GetChunksByBiome(biomeMap);

            foreach (var loc in locations)
            {
                loc.DesiredRoads.Clear();
                foreach (var rc in loc.Roads.Where(rc => rc.target != null && rc.distance > 0f))
                {
                    loc.DesiredRoads[rc.target] = rc.distance;
                }
            }

            foreach (var loc in locations.Where(loc => chunksByBiome.ContainsKey(loc.Biome)))
                loc.CandidateChunks = new List<ChunkManager.Chunk>(chunksByBiome[loc.Biome]);

            var rnd = new System.Random();

            foreach (var loc in locations)
            {
                List<ChunkManager.Chunk> c = loc.CandidateChunks;
                loc.AssignedChunk = c[rnd.Next(c.Count)];
            }

            float bestE = ComputeTotalEnergy(locations);

            for (int it = 0; it < iterations; it++)
            {
                int i = rnd.Next(locations.Count);
                var loc = locations[i];
                var old = loc.AssignedChunk;
                var cand = loc.CandidateChunks[rnd.Next(loc.CandidateChunks.Count)];
                loc.AssignedChunk = cand;

                float newE = ComputeLocalEnergy(loc, locations);
                if (newE < bestE) bestE = newE;
                else loc.AssignedChunk = old;
            }

            return locations;
        }

        /// <summary>
        /// Вычисляет общую энергию системы (суммарное отклонение длин дорог от желаемых)
        /// </summary>
        /// <param name="locs">Список всех размещаемых локаций</param>
        /// <returns>Общее значение энергии системы</returns>
        private static float ComputeTotalEnergy(List<LocationData> locs)
        {
            return (locs.SelectMany(a => a.DesiredRoads, (a, kv) => new { a, kv })
                .Select(@t => new { @t, b = @t.kv.Key })
                .Select(@t => new { @t, target = @t.@t.kv.Value })
                .Select(@t => new
                {
                    @t, d = Vector2.Distance(@t.@t.@t.a.AssignedChunk.Center, @t.@t.b.AssignedChunk.Center)
                })
                .Select(@t => Mathf.Abs(@t.d - @t.@t.target))).Sum();
        }

        /// <summary>
        /// Вычисляет локальную энергию для конкретной локации
        /// </summary>
        /// <param name="loc">Локация для расчета энергии</param>
        /// <param name="all">Все локации системы</param>
        /// <returns>Значение локальной энергии для указанной локации</returns>
        private static float ComputeLocalEnergy(LocationData loc, List<LocationData> all)
        {
            return loc.DesiredRoads.Select(kv => new { kv, other = kv.Key })
                .Select(@t => new { @t, target = @t.kv.Value })
                .Select(@t => new
                {
                    @t, d = Vector2.Distance(loc.AssignedChunk.Center, @t.@t.other.AssignedChunk.Center)
                })
                .Select(t => Mathf.Abs(t.d - t.t.target)).Sum() + (from other in all
                where other.DesiredRoads.ContainsKey(loc)
                let targ = other.DesiredRoads[loc]
                let d = Vector2.Distance(loc.AssignedChunk.Center, other.AssignedChunk.Center)
                select Mathf.Abs(d - targ)).Sum();
        }
    }
}