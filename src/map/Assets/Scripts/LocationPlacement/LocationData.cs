using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Данные локации для генерации карты мира
/// </summary>
[Serializable]
public class LocationData
{
    public string Id;            // "forest1"
    public string Biome;         // "Forest"

    [Header("Roads from this location")]
    public List<RoadConnection> Roads = new List<RoadConnection>();

    [NonSerialized] public Dictionary<LocationData, float> DesiredRoads = new();
    [NonSerialized] public List<ChunkManager.Chunk> CandidateChunks = new();
    [NonSerialized] public ChunkManager.Chunk AssignedChunk;
}

/// <summary>
/// Описание дорожного соединения между двумя локациями
/// </summary>
[Serializable]
public class RoadConnection
{
    [Tooltip("Target location – выберите из списка ниже")]
    public LocationData target;

    [Tooltip("Желаемая длина дороги в единицах карты")]
    public float distance;
}
