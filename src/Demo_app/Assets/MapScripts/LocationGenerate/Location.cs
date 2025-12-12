using UnityEngine;

namespace MapScripts.LocationGenerate
{
    [System.Serializable]
    public class Location
    {
        public string name;
        public Vector2Int PositionOnMap { get; }
        public LocationType Type { get; }
        public bool isGenerated;

        public Location(Vector2Int positionOnMap, LocationType type)
        {
            PositionOnMap = positionOnMap;
            Type = type;
        }
    }

    public enum LocationType
    {
        Village,
        City,
        Dungeon
    }
}