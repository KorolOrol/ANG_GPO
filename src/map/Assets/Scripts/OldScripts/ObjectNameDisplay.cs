using UnityEngine;
using UnityEngine.UI;
using TMPro; // Обязательно для TextMeshPro


// Данный скрипт нужен для появления инфы о локации при клике на неё

public class ObjectNameDisplay : MonoBehaviour
{
    public Canvas worldCanvas;
    public GameObject namePrefab;
    private GameObject currentNameDisplay;

    private void OnMouseDown()
    {
        // Лог для проверки клика
        Debug.Log($"Clicked on {gameObject.name} at position {transform.position}");

        if (currentNameDisplay != null)
        {
            Debug.Log("Name display already exists for this object.");
            return;
        }

        Debug.Log("Instantiating name display...");
        currentNameDisplay = Instantiate(namePrefab, worldCanvas.transform);
        TextMeshProUGUI textComponent = currentNameDisplay.GetComponentInChildren<TextMeshProUGUI>();

        if (textComponent != null)
        {
            textComponent.text = "Город"; // Задаем имя
            Debug.Log("Text component found and name set.");
        }
        else
        {
            Debug.LogWarning("Text component not found in prefab!");
        }

        // Позиционирование текста
        RectTransform rectTransform = currentNameDisplay.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2);
            Debug.Log("Text positioned.");
        }
        else
        {
            Debug.LogWarning("RectTransform not found!");
        }
    }
}
