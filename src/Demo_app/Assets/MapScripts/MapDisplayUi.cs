using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MapDisplayUI : MonoBehaviour
{
    [SerializeField] private string mapContainerName = "MapContainer";

    private VisualElement mapContainer;
    private UIDocument uiDocument;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        FindMapContainer();
    }

    private void FindMapContainer()
    {
        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            mapContainer = uiDocument.rootVisualElement.Q<VisualElement>(mapContainerName);
            if (mapContainer == null)
            {
                Debug.LogError($"Не найден VisualElement с именем '{mapContainerName}' в UI Document");
            }
        }
    }

    public void DrawTexture(Texture2D texture)
    {
        if (mapContainer == null)
        {
            FindMapContainer();
            if (mapContainer == null) return;
        }

        // Создаем Image из текстуры и добавляем в контейнер
        mapContainer.Clear();

        // Создаем визуальный элемент для отображения текстуры
        var image = new VisualElement();
        image.style.backgroundImage = Background.FromTexture2D(texture);
        image.style.width = texture.width;
        image.style.height = texture.height;
        image.style.alignSelf = Align.Center;

        // Подгоняем размер под контейнер
        if (texture.width > mapContainer.resolvedStyle.width ||
            texture.height > mapContainer.resolvedStyle.height)
        {
            // Если текстура больше контейнера, масштабируем
            float scaleX = mapContainer.resolvedStyle.width / texture.width;
            float scaleY = mapContainer.resolvedStyle.height / texture.height;
            float scale = Mathf.Min(scaleX, scaleY);

            image.style.width = texture.width * scale;
            image.style.height = texture.height * scale;
        }

        mapContainer.Add(image);
    }

    public void ClearMap()
    {
        if (mapContainer != null)
        {
            mapContainer.Clear();
        }
    }
}