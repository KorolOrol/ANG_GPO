using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LocationData
{
    public string Id;            // например "forest1"
    public string Biome;         // например "Forest"

    [Header("Roads from this location")]
    public List<RoadConnection> Roads = new List<RoadConnection>();

    // Эти поля нужны внутренне, их не показываем в инспекторе
    [NonSerialized] public Dictionary<LocationData, float> DesiredRoads = new();
    [NonSerialized] public List<ChunkManager.Chunk> CandidateChunks = new();
    [NonSerialized] public ChunkManager.Chunk AssignedChunk;
}

[Serializable]
public class RoadConnection
{
    [Tooltip("Target location – выберите из списка выше")]
    public LocationData target;

    [Tooltip("Желаемая длина дороги в единицах карты")]
    public float distance;
}
