using UnityEngine;

namespace MapScripts.LocationGenerate
{
    public class MapManager : MonoBehaviour
    {
        public GameObject worldMap;
        public GameObject locationMap;
        public LocationMapGenerator locationGenerator;

        public Location CurrentLocation { get; private set; }

        /// <summary>
        /// Открывает указанную локацию
        /// </summary>
        /// <param name="location"></param>
        public void OpenLocation(Location location)
        {
            CurrentLocation = location;
            worldMap.SetActive(false);
            locationMap.SetActive(true);

            if (!location.isGenerated)
            {
                locationGenerator.GenerateLocationMap(location);
                location.isGenerated = true;
            }
            else
            {
                LocationMapGenerator.LoadLocationMap(location);
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
}
    