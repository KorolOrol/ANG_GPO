using UnityEngine;
using UnityEngine.UIElements;

namespace MapScripts
{
    [RequireComponent(typeof(UIDocument))]
    public class MapDisplayUI : MonoBehaviour
    {
        [SerializeField] private string mapContainerName = "MapContainer";

        private VisualElement _mapContainer;
        private UIDocument _uiDocument;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            FindMapContainer();
        }

        private void FindMapContainer()
        {
            if (!_uiDocument || _uiDocument.rootVisualElement == null) return;
            _mapContainer = _uiDocument.rootVisualElement.Q<VisualElement>(mapContainerName);
            if (_mapContainer == null)
            {
                Debug.LogError($"�� ������ VisualElement � ������ '{mapContainerName}' � UI Document");
            }
        }

        public void DrawTexture(Texture2D texture)
        {
            if (_mapContainer == null)
            {
                FindMapContainer();
                if (_mapContainer == null) return;
            }

            // ������� Image �� �������� � ��������� � ���������
            _mapContainer.Clear();

            // ������� ���������� ������� ��� ����������� ��������
            var image = new VisualElement
            {
                style =
                {
                    backgroundImage = Background.FromTexture2D(texture),
                    width = texture.width,
                    height = texture.height,
                    alignSelf = Align.Center
                }
            };

            // ��������� ������ ��� ���������
            if (texture.width > _mapContainer.resolvedStyle.width ||
                texture.height > _mapContainer.resolvedStyle.height)
            {
                // ���� �������� ������ ����������, ������������
                float scaleX = _mapContainer.resolvedStyle.width / texture.width;
                float scaleY = _mapContainer.resolvedStyle.height / texture.height;
                float scale = Mathf.Min(scaleX, scaleY);

                image.style.width = texture.width * scale;
                image.style.height = texture.height * scale;
            }

            _mapContainer.Add(image);
        }

        public void ClearMap()
        {
            _mapContainer?.Clear();
        }
    }
}