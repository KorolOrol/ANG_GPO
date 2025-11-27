using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameObject worldMap;
    public GameObject locationMap;
    public LocationMapGenerator locationGenerator;

    private Location currentLocation;

    /// <summary>
    /// Открывает указанную локацию
    /// </summary>
    /// <param name="location"></param>
    public void OpenLocation(Location location)
    {
        currentLocation = location;
        worldMap.SetActive(false);
        locationMap.SetActive(true);

        if (!location.isGenerated)
        {
            locationGenerator.GenerateLocationMap(location);
            location.isGenerated = true;
        }
        else
        {
            locationGenerator.LoadLocationMap(location);
        }
    }

    /// <summary>
    /// Возвращает на карту мира
    /// </summary>
    public void ReturnToWorld()
    {
        locationMap.SetActive(false);
        worldMap.SetActive(true);
    }
}
    