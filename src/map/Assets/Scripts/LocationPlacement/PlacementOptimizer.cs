using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlacementOptimizer
{
    // locations Ч список LocationData
    // biomeMap Ч массив тех же размеров, но со строковыми биомами
    public static List<LocationData> Optimize(
        List<LocationData> locations,
        string[,] biomeMap,
        int chunkSize,
        int iterations = 5000)
    {
        int w = biomeMap.GetLength(0), h = biomeMap.GetLength(1);
        var cm = new ChunkManager(w, h, chunkSize);
        var chunksByBiome = cm.GetChunksByBiome(biomeMap);
        // 0) «аполн€ем DesiredRoads из инспекторского списка Roads
        foreach (var loc in locations)
        {
            loc.DesiredRoads.Clear();
            foreach (var rc in loc.Roads)
            {
                if (rc.target != null && rc.distance > 0f)
                    loc.DesiredRoads[rc.target] = rc.distance;
            }
        }

        // 1. —писок кандидатов
        foreach (var loc in locations)
            if (chunksByBiome.ContainsKey(loc.Biome))
                loc.CandidateChunks = new List<ChunkManager.Chunk>(chunksByBiome[loc.Biome]);

        var rnd = new System.Random();
        // 2. —лучайна€ инициализаци€
        foreach (var loc in locations)
        {
            var c = loc.CandidateChunks;
            loc.AssignedChunk = c[rnd.Next(c.Count)];
        }

        float bestE = ComputeTotalEnergy(locations);
        // 3. Hill-climbing / annealing
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

    private static float ComputeLocalEnergy(LocationData loc, List<LocationData> all)
    {
        float E = 0;
        foreach (var kv in loc.DesiredRoads)
        {
            var other = kv.Key; float target = kv.Value;
            float d = Vector2.Distance(loc.AssignedChunk.center, other.AssignedChunk.center);
            E += Mathf.Abs(d - target);
        }
        // учтЄм пары, где loc Ч в значени€х DesiredRoads
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
