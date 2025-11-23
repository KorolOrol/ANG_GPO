using System;
using System.Collections.Generic;
using UnityEngine;

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
        var chunksByBiome = cm.GetChunksByBiome(biomeMap);

        foreach (var loc in locations)
        {
            loc.DesiredRoads.Clear();
            foreach (var rc in loc.Roads)
            {
                if (rc.target != null && rc.distance > 0f)
                    loc.DesiredRoads[rc.target] = rc.distance;
            }
        }

        foreach (var loc in locations)
            if (chunksByBiome.ContainsKey(loc.Biome))
                loc.CandidateChunks = new List<ChunkManager.Chunk>(chunksByBiome[loc.Biome]);

        var rnd = new System.Random();

        foreach (var loc in locations)
        {
            var c = loc.CandidateChunks;
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
        float E = 0;
        foreach (var a in locs)
            foreach (var kv in a.DesiredRoads)
            {
                var b = kv.Key; float target = kv.Value;
                float d = Vector2.Distance(a.AssignedChunk.center, b.AssignedChunk.center);
                E += Mathf.Abs(d - target);
            }
        return E;
    }

    /// <summary>
    /// Вычисляет локальную энергию для конкретной локации
    /// </summary>
    /// <param name="loc">Локация для расчета энергии</param>
    /// <param name="all">Все локации системы</param>
    /// <returns>Значение локальной энергии для указанной локации</returns>
    private static float ComputeLocalEnergy(LocationData loc, List<LocationData> all)
    {
        float E = 0;
        foreach (var kv in loc.DesiredRoads)
        {
            var other = kv.Key; float target = kv.Value;
            float d = Vector2.Distance(loc.AssignedChunk.center, other.AssignedChunk.center);
            E += Mathf.Abs(d - target);
        }
        foreach (var other in all)
            if (other.DesiredRoads.ContainsKey(loc))
            {
                float targ = other.DesiredRoads[loc];
                float d = Vector2.Distance(loc.AssignedChunk.center, other.AssignedChunk.center);
                E += Mathf.Abs(d - targ);
            }
        return E;
    }
}