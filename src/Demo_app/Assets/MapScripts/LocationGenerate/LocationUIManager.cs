using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MapScripts.LocationGenerate
{
    public class LocationUIManager : MonoBehaviour
    {
        [SerializeField] private MapGeneratorManual mapGenerator;
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private string mapContainerName = "MapContainer";

        private VisualElement _mapContainer;
        private readonly Dictionary<string, VisualElement> _locationMarkers = new Dictionary<string, VisualElement>();
        private readonly List<VisualElement> _roadMarkers = new List<VisualElement>();

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (!uiDocument)
            {
                uiDocument = GetComponent<UIDocument>();
                if (uiDocument == null) return;
            }

            if (uiDocument.rootVisualElement != null)
            {
                _mapContainer = uiDocument.rootVisualElement.Q<VisualElement>(mapContainerName);
            }
        }

        public void AddLocationMarker(string locationName, Vector2 position, Color color)
        {
            if (_mapContainer == null) Initialize();
            if (_mapContainer == null || !mapGenerator) return;

            var marker = new VisualElement
            {
                name = $"Marker_{locationName}",
                style =
                {
                    width = 16,
                    height = 16,
                    backgroundColor = new StyleColor(color),
                    position = Position.Absolute
                }
            };

            float xPercent = position.x / mapGenerator.mapWidth * 100f;
            float yPercent = position.y / mapGenerator.mapHeight * 100f;

            marker.style.left = xPercent;
            marker.style.top = yPercent;

            var label = new Label(locationName)
            {
                style =
                {
                    color = Color.white,
                    fontSize = 11,
                    position = Position.Absolute,
                    top = -20,
                    left = -20
                }
            };

            marker.Add(label);
            _mapContainer.Add(marker);

            _locationMarkers[locationName] = marker;
        }

        public void AddRoadMarker(Vector2 start, Vector2 end, Color color)
        {
            if (_mapContainer == null) Initialize();
            if (_mapContainer == null || !mapGenerator) return;

            float startXPercent = start.x / mapGenerator.mapWidth * 100f;
            float startYPercent = start.y / mapGenerator.mapHeight * 100f;
            float endXPercent = end.x / mapGenerator.mapWidth * 100f;
            float endYPercent = end.y / mapGenerator.mapHeight * 100f;

            var line = new VisualElement
            {
                name = $"Road_{Time.frameCount}",
                style =
                {
                    // ������� ����� ������ ����������
                    position = Position.Absolute,
                    left = startXPercent,
                    top = startYPercent,
                    width = Mathf.Abs(endXPercent - startXPercent),
                    height = 3,
                    backgroundColor = new StyleColor(color)
                }
            };

            _mapContainer.Add(line);
            _roadMarkers.Add(line);
        }

        public void ClearMarkers()
        {
            if (_mapContainer == null) return;

            foreach (var marker in _locationMarkers.Values)
                _mapContainer.Remove(marker);

            foreach (var road in _roadMarkers)
                _mapContainer.Remove(road);

            _locationMarkers.Clear();
            _roadMarkers.Clear();
        }

        public void RemoveLocationMarker(string locationName)
        {
            if (_mapContainer == null) return;

            if (!_locationMarkers.TryGetValue(locationName, out var marker)) return;
            _mapContainer.Remove(marker);
            _locationMarkers.Remove(locationName);
        }
    }
}