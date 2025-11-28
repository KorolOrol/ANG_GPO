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
/// Скрипт для управления настройками карты и генерации карты.
/// </summary>
public class MapActionController : IActionController
{
    /// <summary>
    /// Ссылка на текущий сюжет.
    /// </summary>
    private Plot _plot;
    
    /// <summary>
    /// Слайдер для настройки ширины карты.
    /// </summary>
    private Slider _widthSlider;
    
    /// <summary>
    /// Слайдер для настройки высоты карты.
    /// </summary>
    private Slider _heightSlider;
    
    /// <summary>
    /// Текстовое поле для размера чанка.
    /// </summary>
    private TextField _chunkSizeTextField;
    
    /// <summary>
    /// Текстовое поле для сида генерации.
    /// </summary>
    private TextField _seedTextField;
    
    /// <summary>
    /// Текстовое поле для масштаба шума.
    /// </summary>
    private TextField _noizeScaleTextField;
    
    /// <summary>
    /// Текстовое поле для количества октав.
    /// </summary>
    private TextField _octavesTextField;
    
    /// <summary>
    /// Слайдер для настройки персистентности.
    /// </summary>
    private Slider _persistanceSlider;
    
    /// <summary>
    /// Слайдер для настройки лакунарности.
    /// </summary>
    private Slider _lacunaritySlider;
    
    /// <summary>
    /// Текстовое поле для смещения шума по X.
    /// </summary>
    private TextField _noizeOffsetXTextField;
    
    /// <summary>
    /// Текстовое поле для смещения шума по Y.
    /// </summary>
    private TextField _noizeOffsetYTextField;
    
    /// <summary>
    /// Список локаций для отображения на карте.
    /// </summary>
    private ListView _locationsListView;
    
    /// <summary>
    /// Элемент визуального интерфейса для отображения карты.
    /// </summary>
    private VisualElement _mapViewVisualElement;
    
    /// <summary>
    /// Флаг инициализации визуальных элементов.
    /// </summary>
    private bool _isVisualElementsInitiated;
    
    /// <summary>
    /// Инициализация контроллера карты.
    /// </summary>
    /// <param name="root">Корневой элемент визуального интерфейса.</param>
    /// <param name="plot">Текущий сюжет.</param>
    public void Initiate(VisualElement root, Plot plot)
    {
        InitiateVisualElements(root);
        _plot = plot;
    }
    
    /// <summary>
    /// Инициализация визуальных элементов UI.
    /// </summary>
    /// <param name="root">Корневой элемент визуального интерфейса.</param>
    private void InitiateVisualElements(VisualElement root)
    {
        if (_isVisualElementsInitiated) return;
        _isVisualElementsInitiated = true;
        
        _widthSlider = root.Q<Slider>("WidthSlider");
        _heightSlider = root.Q<Slider>("HeightSlider");
        _chunkSizeTextField = root.Q<TextField>("ChunkSizeTextField");
        _seedTextField = root.Q<TextField>("SeedTextField");
        _noizeScaleTextField = root.Q<TextField>("NoizeScaleTextField");
        _octavesTextField = root.Q<TextField>("OctavesTextField");
        _persistanceSlider = root.Q<Slider>("PersistanceSlider");
        _lacunaritySlider = root.Q<Slider>("LacunaritySlider");
        _noizeOffsetXTextField = root.Q<TextField>("NoizeOffsetXTextField");
        _noizeOffsetYTextField = root.Q<TextField>("NoizeOffsetYTextField");
        _locationsListView = root.Q<ListView>("LocationsListView");
        _mapViewVisualElement = root.Q<VisualElement>("MapViewVisualElement");
        
        _locationsListView.makeItem = MakeLocationListItem;
        _locationsListView.bindItem = BindLocationListItem;
    }
    
    /// <summary>
    /// Получение действия для обновления данных.
    /// </summary>
    /// <returns>Действие обновления.</returns>
    public Action GetUpdateAction()
    {
        return UpdateLocationsList;
    }
    
    /// <summary>
    /// Обновление списка локаций из сюжета.
    /// </summary>
    private void UpdateLocationsList()
    {
        var locations = _plot.Elements
            .Where(e => e.Type == ElemType.Location)
            .ToList();
        
        _locationsListView.itemsSource = locations;
        _locationsListView.RefreshItems();
    }
    
    /// <summary>
    /// Создание элемента списка для отображения локации.
    /// </summary>
    /// <returns>Визуальный элемент для списка.</returns>
    private VisualElement MakeLocationListItem()
    {
        var elementAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/ElementListItem.uxml");
        var element = elementAsset.CloneTree();
        return element;
    }
    
    /// <summary>
    /// Привязка данных локации к элементу списка в UI.
    /// </summary>
    /// <param name="e">Визуальный элемент.</param>
    /// <param name="i">Индекс элемента в списке.</param>
    private void BindLocationListItem(VisualElement e, int i)
    {
        var labelUI = e.Q<Label>("ElementName");
        var iconUI = e.Q<VisualElement>("ElementIcon");
        
        IElement element = null;
        var items = _locationsListView.itemsSource;
        if (items != null && i >= 0 && i < items.Count)
        {
            element = items[i] as IElement;
        }

        if (element == null) return;
        
        var vectorImage = AssetDatabase.LoadAssetAtPath<VectorImage>("Assets/Icons/ElemTypeLocationIcon.svg");
        labelUI.text = element.Name;
        iconUI.style.backgroundImage = new StyleBackground(vectorImage);
    }
}
