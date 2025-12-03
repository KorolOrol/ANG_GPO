using UnityEngine;

[System.Serializable]
public class Location
{
    public string name;
    public Vector2Int positionOnMap;
    public LocationType type;
    public bool isGenerated = false;
}

public enum LocationType
{
    Village,
    City,
    Dungeon
}
