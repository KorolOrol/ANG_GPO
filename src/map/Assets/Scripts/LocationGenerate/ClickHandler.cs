using UnityEngine;
using static UnityEditor.FilePathAttribute;

public class LocationClickHandler : MonoBehaviour
{
    public MapManager mapManager;
    public Location location;

    private void OnMouseDown()
    {
        mapManager.OpenLocation(location);
    }
}