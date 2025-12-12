using UnityEngine;

namespace MapScripts.LocationGenerate
{
    public class LocationObject : MonoBehaviour
    {
        public string locationName;
        public string Biome { get; set; }
        public Vector2 MapPosition { get; set; }

        public Material OriginalMaterial { get; private set; }
        private Color _originalColor;

        /// <summary>
        /// �������������� ������ �������
        /// </summary>
        public void Initialize(string locName, string biomeType, Vector2 position)
        {
            locationName = locName;
            Biome = biomeType;
            MapPosition = position;

            // ��������� ������������ �������� ��� ��������
            var renderer = GetComponent<Renderer>();
            if (renderer)
            {
                OriginalMaterial = renderer.material;
                _originalColor = renderer.material.color;
            }

            Debug.Log($"������ ������� '{locName}' ���������������");
        }

        /// <summary>
        /// ������������ �������
        /// </summary>
        public void SetHighlighted(bool highlighted)
        {
            var renderer = GetComponent<Renderer>();
            if (renderer == null) return;
            renderer.material.color = highlighted ? Color.yellow : _originalColor;
        }

        /// <summary>
        /// ��������� (��� ���������)
        /// </summary>
        public void Pulse()
        {
            // ����� �������� �������� ���������
            Debug.Log($"������� '{locationName}' ��������");
        }

        /// <summary>
        /// ���������� ��� �����
        /// </summary>
        private void OnMouseDown()
        {
            Debug.Log($"���� �� �������: {locationName}");
            SetHighlighted(true);

            // ����� �������� ����� ����������� �����
            // FindObjectOfType<CameraController>().ZoomToLocation(this);
        }

        private void OnMouseEnter()
        {
            SetHighlighted(true);
        }

        private void OnMouseExit()
        {
            SetHighlighted(false);
        }
    }
}