using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainWindowScript : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset _viewAction;
    [SerializeField] private VisualTreeAsset _elementListItem;
    
    private VisualElement root;
    private TemplateContainer newAction;
    private Button addButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        newAction = _viewAction.CloneTree();
        var actionArea = root.Q<VisualElement>("ActionArea");
        actionArea.Add(newAction);
        addButton = root.Q<Button>("NewButton");
        addButton.clicked += AddElement;
    }
    
    void AddElement()
    {
        Debug.Log("Add element");
        var elementsList = root.Q<ListView>("ElementsList");
        var itemsSource = elementsList.itemsSource as List<string>;
        if (itemsSource == null)
        {
            itemsSource = new List<string>();
            elementsList.itemsSource = itemsSource;
            elementsList.makeItem = () => _elementListItem.CloneTree();
            elementsList.bindItem = (element, i) =>
            {
                // Пример: если в шаблоне есть Label с именем "Label"
                var label = element.Q<Label>("Label");
                if (label != null)
                    label.text = itemsSource[i];
            };
        }
        itemsSource.Add("Новый элемент");
        elementsList.Rebuild();
    }
}
