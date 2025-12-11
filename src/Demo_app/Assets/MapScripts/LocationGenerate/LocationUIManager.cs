using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LocationUIManager : MonoBehaviour
{
    [SerializeField] private MapGeneratorManual mapGenerator;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string mapContainerName = "MapContainer";

    private VisualElement mapContainer;
    private Dictionary<string, VisualElement> locationMarkers = new Dictionary<string, VisualElement>();
    private List<VisualElement> roadMarkers = new List<VisualElement>();

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null) return;
        }

        if (uiDocument.rootVisualElement != null)
        {
            mapContainer = uiDocument.rootVisualElement.Q<VisualElement>(mapContainerName);
        }
    }

    public void AddLocationMarker(string locationName, Vector2 position, Color color)
    {
        if (mapContainer == null) Initialize();
        if (mapContainer == null || mapGenerator == null) return;

        var marker = new VisualElement();
        marker.name = $"Marker_{locationName}";
        marker.style.width = 16;
        marker.style.height = 16;
        marker.style.backgroundColor = new StyleColor(color);
        marker.style.position = Position.Absolute;

        float xPercent = (position.x / mapGenerator.mapWidth) * 100f;
        float yPercent = (position.y / mapGenerator.mapHeight) * 100f;

        marker.style.left = xPercent;
        marker.style.top = yPercent;

        var label = new Label(locationName);
        label.style.color = Color.white;
        label.style.fontSize = 11;
        label.style.position = Position.Absolute;
        label.style.top = -20;
        label.style.left = -20;

        marker.Add(label);
        mapContainer.Add(marker);

        locationMarkers[locationName] = marker;
    }

    public void AddRoadMarker(Vector2 start, Vector2 end, Color color)
    {
        if (mapContainer == null) Initialize();
        if (mapContainer == null || mapGenerator == null) return;

        float startXPercent = (start.x / mapGenerator.mapWidth) * 100f;
        float startYPercent = (start.y / mapGenerator.mapHeight) * 100f;
        float endXPercent = (end.x / mapGenerator.mapWidth) * 100f;
        float endYPercent = (end.y / mapGenerator.mapHeight) * 100f;

        var line = new VisualElement();
        line.name = $"Road_{Time.frameCount}";

        // Простая линия вместо повернутой
        line.style.position = Position.Absolute;
        line.style.left = startXPercent;
        line.style.top = startYPercent;
        line.style.width = Mathf.Abs(endXPercent - startXPercent);
        line.style.height = 3;
        line.style.backgroundColor = new StyleColor(color);

        mapContainer.Add(line);
        roadMarkers.Add(line);
    }

    public void ClearMarkers()
    {
        if (mapContainer == null) return;

        foreach (var marker in locationMarkers.Values)
            mapContainer.Remove(marker);

        foreach (var road in roadMarkers)
            mapContainer.Remove(road);

        locationMarkers.Clear();
        roadMarkers.Clear();
    }

    public void RemoveLocationMarker(string locationName)
    {
        if (mapContainer == null) return;

        if (locationMarkers.TryGetValue(locationName, out var marker))
        {
            mapContainer.Remove(marker);
            locationMarkers.Remove(locationName);
        }
    }
}