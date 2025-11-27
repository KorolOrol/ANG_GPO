using System;
using System.Collections.Generic;
using System.Linq;
using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Скрипт для управления отображением и редактированием элементов сюжета.
/// </summary>
public class ViewActionController : IAction
{
    /// <summary>
    /// Список всех элементов для отображения и редактирования
    /// </summary>
    private Plot _plot;
    
    /// <summary>
    /// Текущий выбранный элемент для редактирования
    /// </summary>
    private Element _currentElement;
    
    /// <summary>
    /// Список элементов для редактирования
    /// </summary>
    private ListView _elementsListView;

    /// <summary>
    /// Выпадающий список для фильтрации по типу элемента
    /// </summary>
    private DropdownField _filterDropdown;
    
    /// <summary>
    /// Текстовое поле для фильтрации списка элементов
    /// </summary>
    private TextField _searchTextField;
    
    /// <summary>
    /// Кнопки добавления новых персонажей
    /// </summary>
    private Button _addCharacterButton;
    
    /// <summary>
    /// Кнопки добавления новых предметов
    /// </summary>
    private Button _addItemButton;
    
    /// <summary>
    /// Кнопки добавления новых локаций
    /// </summary>
    private Button _addLocationButton;
    
    /// <summary>
    /// Кнопки добавления новых событий
    /// </summary>
    private Button _addEventButton;
    
    /// <summary>
    /// Кнопка удаления выбранного элемента
    /// </summary>
    private Button _deleteElementButton;
    
    /// <summary>
    /// Панель редактирования выбранного элемента
    /// </summary>
    private VisualElement _editSelectedElement;
    
    /// <summary>
    /// Контроллер панели редактирования выбранного элемента
    /// </summary>
    private EditSelectedElementController _editSelectedElementController;
    
    /// <summary>
    /// Флаг инициализации визуальных элементов
    /// </summary>
    private bool _isVisualElementsInitiated;
    
    /// <summary>
    /// Привязка элементов к списку в UI
    /// </summary>
    /// <param name="root">Корневой элемент UI</param>
    /// <param name="plot">Сюжет с элементами</param>
    public void Initiate(VisualElement root, Plot plot)
    {
        InitiateVisualElements(root);
        _plot = plot;
        _editSelectedElementController = new EditSelectedElementController(_editSelectedElement, _plot);
        _editSelectedElementController.BeforeLoadSelectedElement += ShowEditSelectedElement;
        _editSelectedElementController.AfterUpdateSelectedElement += () => _elementsListView.RefreshItems();
        
        _elementsListView.itemsSource = _plot.Elements;
    }
    
    /// <summary>
    /// Инициализация визуальных элементов UI
    /// </summary>
    /// <param name="root">Корневой элемент UI</param>
    private void InitiateVisualElements(VisualElement root)
    {
        if (_isVisualElementsInitiated) return;
        _isVisualElementsInitiated = true;
        _elementsListView = root.Q<ListView>("ElementsListView");
        _filterDropdown = root.Q<DropdownField>("FilterDropdown");
        _searchTextField = root.Q<TextField>("SearchTextField");
        _addCharacterButton = root.Q<Button>("AddCharacterButton");
        _addItemButton = root.Q<Button>("AddItemButton");
        _addLocationButton = root.Q<Button>("AddLocationButton");
        _addEventButton = root.Q<Button>("AddEventButton");
        _deleteElementButton = root.Q<Button>("DeleteElementButton");
        _editSelectedElement = root.Q<VisualElement>("EditSelectedElement");
        
        _elementsListView.makeItem = MakeElementListItem;
        _elementsListView.bindItem = BindElementListItem;
        _elementsListView.onAdd = ElementsListViewOnAdd;
        _elementsListView.onRemove = ElementsListViewOnRemove;
        _elementsListView.selectedIndicesChanged += ElementsListViewSelectedIndicesChanged;
        
        _filterDropdown.RegisterValueChangedCallback(_ => ApplyFilters());
        _searchTextField.RegisterValueChangedCallback(_ => ApplyFilters());

        _addCharacterButton.clicked += AddElementListItem(ElemType.Character);
        _addItemButton.clicked += AddElementListItem(ElemType.Item);
        _addLocationButton.clicked += AddElementListItem(ElemType.Location);
        _addEventButton.clicked += AddElementListItem(ElemType.Event);
        _deleteElementButton.clicked += () => ElementsListViewOnRemove(_elementsListView);
    }
    
    /// <summary>
    /// Получение действия для обновления списка элементов
    /// </summary>
    /// <returns>Действие обновления списка элементов</returns>
    public Action GetUpdateAction()
    {
        return ApplyFilters;
    }

    /// <summary>
    /// Поиск индекса элемента в текущем источнике элементов списка
    /// </summary>
    /// <param name="element">Искомый элемент</param>
    /// <returns>Индекс элемента или -1, если не найден</returns>
    private int FindIndexInItems(object element)
    {
        if (_elementsListView.itemsSource is not List<IElement> items) return -1;
        for (int i = 0; i < items.Count; i++)
        {
            var it = items[i];
            if (it == element || it.Equals(element)) return i;
        }
        return -1;
    }
    
    /// <summary>
    /// Обработчик изменения выбранных индексов в списке элементов
    /// </summary>
    /// <param name="indices">Выбранные индексы</param>
    private void ElementsListViewSelectedIndicesChanged(IEnumerable<int> indices)
    {
        int index = -1;
        foreach (int i in indices)
        {
            index = i;
            break;
        }
        if (index < 0 || _elementsListView.itemsSource is not List<IElement> items || index >= items.Count)
        {
            _currentElement = null;
            return;
        }
        var elem = items[index];
        if (elem == null)
        {
            _currentElement = null;
            return;
        }
        Debug.Log($"{elem.Name}, {elem.Type}");
        _currentElement = (Element)elem;
        _editSelectedElementController.SelectedElement = _currentElement;
    }

    /// <summary>
    /// Обработчик добавления нового элемента заданного типа в список элементов
    /// </summary>
    /// <param name="elemType">Тип нового элемента</param>
    /// <returns>Действие добавления элемента</returns>
    private Action AddElementListItem(ElemType elemType)
    {
        return () =>
        {
            var newElement = FullElementConstructor.CreateFullElement(elemType, "New " + elemType);
            _plot.Add(newElement);
            ApplyFilters();
            
            int idx = FindIndexInItems(newElement);
            if (idx >= 0)
            {
                _elementsListView.ScrollToItem(idx);
            }
        };
    }
    /// <summary>
    /// Применение фильтров к списку элементов
    /// </summary>
    private void ApplyFilters()
    {
        List<IElement> items = _plot.Elements;
        
        string filterType = _filterDropdown.value;
        if (filterType != "All elements")
        {
            if (Enum.TryParse(filterType, out ElemType elemType))
            {
                items = items.Where(e => e.Type == elemType).ToList();
            }
        }
        
        string searchText = _searchTextField.value;
        if (!string.IsNullOrEmpty(searchText))
        {
            searchText = searchText.Trim();
            items = items.Where(e => e.Name
                .Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        
        _elementsListView.itemsSource = items;
        _elementsListView.RefreshItems();
    }


    /// <summary>
    /// Создание элемента списка для отображения в ListView
    /// </summary>
    private VisualElement MakeElementListItem()
    {
        var elementAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/ElementListItem.uxml");
        var element = elementAsset.CloneTree();
        return element;
    }

    /// <summary>
    /// Привязка данных элемента к элементу списка в UI
    /// </summary>
    private void BindElementListItem(VisualElement e, int i)
    {
        var labelUI = e.Q<Label>("ElementName");
        var iconUI = e.Q<VisualElement>("ElementIcon");
        var icon = ScriptableObject.CreateInstance<VectorImage>();
        IElement element = null;
        var items = _elementsListView.itemsSource;
        if (items != null && i >= 0 && i < items.Count)
        {
            element = items[i] as IElement;
        }

        if (element == null) return;
        var vectorImage = element.Type switch
        {
            ElemType.Character =>
                AssetDatabase.LoadAssetAtPath<VectorImage>("Assets/Icons/ElemTypeCharacterIcon.svg"),
            ElemType.Item => AssetDatabase.LoadAssetAtPath<VectorImage>("Assets/Icons/ElemTypeItemIcon.svg"),
            ElemType.Location => AssetDatabase.LoadAssetAtPath<VectorImage>("Assets/Icons/ElemTypeLocationIcon.svg"),
            ElemType.Event => AssetDatabase.LoadAssetAtPath<VectorImage>("Assets/Icons/ElemTypeEventIcon.svg"),
            _ => icon
        };
        labelUI.text = element.Name;
        iconUI.style.backgroundImage = new StyleBackground(vectorImage);
    }

    /// <summary>
    /// Обработчик добавления нового элемента в список элементов
    /// </summary>
    private void ElementsListViewOnAdd(BaseListView listView)
    {
        var newElement = FullElementConstructor.CreateFullElement(ElemType.Character, "New Character");
        _plot.Add(newElement);
        ApplyFilters();
        
        int index = FindIndexInItems(newElement);
        if (index >= 0)
        {
            listView.ScrollToItem(index);
        }
    }

    /// <summary>
    /// Обработчик удаления выбранного элемента из списка элементов
    /// </summary>
    private void ElementsListViewOnRemove(BaseListView listView)
    {
        int index = listView.selectedIndex;
        if (index == -1) index = listView.itemsSource.Count - 1;
        var items = listView.itemsSource;
        if (items == null || index >= items.Count) return;
        if (items[index] is not IElement element) return;
        
        _plot.Remove(element);
        
        ApplyFilters();
        
        var newItems = listView.itemsSource;
        if (newItems is not { Count: > 0 }) return;
        int newIndex = Math.Min(newItems.Count - 1, index);
        listView.ScrollToItem(newIndex);
        ElementsListViewSelectedIndicesChanged(new[] { newIndex });
    }
    
    /// <summary>
    /// Показать или скрыть панель редактирования выбранного элемента
    /// </summary>
    private void ShowEditSelectedElement()
    {
        _editSelectedElement.style.display = _currentElement == null ? DisplayStyle.None : DisplayStyle.Flex;
    }
}
