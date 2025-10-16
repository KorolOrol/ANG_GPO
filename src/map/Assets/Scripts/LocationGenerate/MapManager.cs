using UnityEngine;
using static UnityEditor.FilePathAttribute;

public class MapManager : MonoBehaviour
{
    public GameObject worldMap;              // объект с основной картой
    public GameObject locationMap;           // объект с картой локации
    public LocationMapGenerator locationGenerator;

    private Location currentLocation;

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

    public void ReturnToWorld()
    {
        locationMap.SetActive(false);
        worldMap.SetActive(true);
    }
}
    