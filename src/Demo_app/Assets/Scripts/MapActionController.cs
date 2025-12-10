using System;
using System.Linq;
using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

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
    /// Ссылка на компонент генерации карты.
    /// </summary>
    private MapGeneratorManual _mapGenerator;

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
    /// Кнопка для запуска генерации карты.
    /// </summary>
    private Button _generateMapButton;

    /// <summary>
    /// Кнопка для очистки карты.
    /// </summary>
    private Button _clearMapButton;

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
        _plot = plot;
        // Предполагаем, что MapGeneratorManual находится на той же сцене
        _mapGenerator = Object.FindFirstObjectByType<MapGeneratorManual>();
        if (_mapGenerator == null)
        {
            Debug.LogError("MapGeneratorManual не найден на сцене. Генерация карты будет недоступна.");
            // Можно отключить UI-элементы, связанные с генерацией
        }
        else
        {
            // Инициализируем UI значениями из генератора по умолчанию
            // (или из сохранения, если оно будет реализовано)
            InitializeUIValues();
        }
        InitiateVisualElements(root);
    }

    /// <summary>
    /// Инициализация начальных значений UI из компонента генерации или конфигурации.
    /// </summary>
    private void InitializeUIValues()
    {
        if (_mapGenerator == null) return;

        // Устанавливаем значения из _mapGenerator в UI-элементы
        // Проверяем, что элементы инициализированы
        if (_widthSlider != null) _widthSlider.value = _mapGenerator.mapWidth;
        if (_heightSlider != null) _heightSlider.value = _mapGenerator.mapHeight;
        if (_chunkSizeTextField != null) _chunkSizeTextField.value = _mapGenerator.chunkSize.ToString();
        if (_seedTextField != null) _seedTextField.value = _mapGenerator.seed.ToString();
        if (_noizeScaleTextField != null) _noizeScaleTextField.value = _mapGenerator.noiseScale.ToString();
        if (_octavesTextField != null) _octavesTextField.value = _mapGenerator.octaves.ToString();
        if (_persistanceSlider != null) _persistanceSlider.value = _mapGenerator.persistance;
        if (_lacunaritySlider != null) _lacunaritySlider.value = _mapGenerator.lacunarity;
        if (_noizeOffsetXTextField != null) _noizeOffsetXTextField.value = _mapGenerator.noiseOffset.x.ToString();
        if (_noizeOffsetYTextField != null) _noizeOffsetYTextField.value = _mapGenerator.noiseOffset.y.ToString();
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
        _generateMapButton = root.Q<Button>("GenerateMapButton"); // Добавлен
        _clearMapButton = root.Q<Button>("ClearMapButton");       // Добавлен
        _locationsListView = root.Q<ListView>("LocationsListView");
        _mapViewVisualElement = root.Q<VisualElement>("MapViewVisualElement");

        _locationsListView.makeItem = MakeLocationListItem;
        _locationsListView.bindItem = BindLocationListItem;

        // Регистрация обработчиков событий
        if (_generateMapButton != null)
        {
            _generateMapButton.clicked += OnGenerateMapButtonClicked;
        }
        if (_clearMapButton != null)
        {
            _clearMapButton.clicked += OnClearMapButtonClicked;
        }
    }

    /// <summary>
    /// Обработчик нажатия на кнопку генерации карты.
    /// </summary>
    private void OnGenerateMapButtonClicked()
    {
        Debug.Log("Нажата кнопка 'Generate Map' в MapActionController.");
        if (_mapGenerator == null)
        {
            Debug.LogError("MapGeneratorManual не найден. Генерация невозможна.");
            return;
        }

        // Считываем значения из UI и устанавливаем их в MapGeneratorManual
        if (_widthSlider != null) _mapGenerator.mapWidth = (int)_widthSlider.value;
        if (_heightSlider != null) _mapGenerator.mapHeight = (int)_heightSlider.value;
        if (_chunkSizeTextField != null && int.TryParse(_chunkSizeTextField.value, out int chunkSize))
        {
            _mapGenerator.chunkSize = chunkSize;
        }
        if (_seedTextField != null && int.TryParse(_seedTextField.value, out int seed))
        {
            _mapGenerator.seed = seed;
        }
        if (_noizeScaleTextField != null && float.TryParse(_noizeScaleTextField.value, out float noiseScale))
        {
            _mapGenerator.noiseScale = noiseScale;
        }
        if (_octavesTextField != null && int.TryParse(_octavesTextField.value, out int octaves))
        {
            _mapGenerator.octaves = octaves;
        }
        if (_persistanceSlider != null) _mapGenerator.persistance = _persistanceSlider.value;
        if (_lacunaritySlider != null) _mapGenerator.lacunarity = _lacunaritySlider.value;
        if (_noizeOffsetXTextField != null && float.TryParse(_noizeOffsetXTextField.value, out float offsetX))
        {
            float offsetY = _mapGenerator.noiseOffset.y;
            if (_noizeOffsetYTextField != null && float.TryParse(_noizeOffsetYTextField.value, out float parsedY))
            {
                offsetY = parsedY;
            }
            _mapGenerator.noiseOffset = new Vector2(offsetX, offsetY);
        }
        // Пока что не обрабатываем ручные локации из Plot, используем _mapGenerator.locations

        Debug.Log("Вызов _mapGenerator.GenerateManualMap() с новыми параметрами.");
        // Вызываем генерацию
        _mapGenerator.GenerateManualMap();

        // Обновляем список локаций после генерации (если они были добавлены в Plot)
        UpdateLocationsList();
    }

    /// <summary>
    /// Обработчик нажатия на кнопку очистки карты.
    /// </summary>
    private void OnClearMapButtonClicked()
    {
        Debug.Log("Нажата кнопка 'Clear Map' в MapActionController.");
        if (_mapGenerator == null)
        {
            Debug.LogError("MapGeneratorManual не найден. Очистка невозможна.");
            return;
        }

        // Очищаем объекты, созданные генератором
        _mapGenerator.ClearMap(); // Предполагаем, что такой метод будет добавлен в MapGeneratorManual

        // Очищаем визуализацию карты (например, устанавливая пустую текстуру)
        // Это зависит от реализации MapDisplay и VisualElement
        // Пока что просто очистим список локаций
        _locationsListView.Clear();
        _locationsListView.RefreshItems();
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
    private static VisualElement MakeLocationListItem()
    {
        AsyncOperationHandle<VisualTreeAsset> handle = 
            Addressables.LoadAssetAsync<VisualTreeAsset>("Assets/UI/ElementListItem.uxml");
        handle.WaitForCompletion();
        var elementAsset = handle.Result;
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

        var handle = 
            Addressables.LoadAssetAsync<VectorImage>("Assets/Icons/ElemTypeLocationIcon.svg");
        handle.WaitForCompletion();
        var vectorImage = handle.Result;
        labelUI.text = element.Name;
        iconUI.style.backgroundImage = new StyleBackground(vectorImage);
    }
}