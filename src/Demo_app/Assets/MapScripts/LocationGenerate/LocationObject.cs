using UnityEngine;
using TMPro;

public class LocationObject : MonoBehaviour
{
    public string locationName;
    public string biome;
    public Vector2 mapPosition;

    private Material originalMaterial;
    private Color originalColor;

    /// <summary>
    /// Инициализирует объект локации
    /// </summary>
    public void Initialize(string name, string biomeType, Vector2 position)
    {
        locationName = name;
        biome = biomeType;
        mapPosition = position;

        // Сохраняем оригинальный материал для анимаций
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            originalMaterial = renderer.material;
            originalColor = renderer.material.color;
        }

        Debug.Log($"Объект локации '{name}' инициализирован");
    }

    /// <summary>
    /// Подсвечивает локацию
    /// </summary>
    public void SetHighlighted(bool highlighted)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            if (highlighted)
            {
                renderer.material.color = Color.yellow;
            }
            else
            {
                renderer.material.color = originalColor;
            }
        }
    }

    /// <summary>
    /// Пульсация (для выделения)
    /// </summary>
    public void Pulse()
    {
        // Можно добавить анимацию пульсации
        Debug.Log($"Локация '{locationName}' выделена");
    }

    /// <summary>
    /// Вызывается при клике
    /// </summary>
    private void OnMouseDown()
    {
        Debug.Log($"Клик по локации: {locationName}");
        SetHighlighted(true);

        // Можно добавить вызов приближения здесь
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