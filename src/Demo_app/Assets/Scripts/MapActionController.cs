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

    // НОВЫЕ ПОЛЯ ДЛЯ УПРАВЛЕНИЯ ЛОКАЦИЯМИ
    private TextField _locationNameField;
    private DropdownField _biomeDropdown;
    private Button _addLocationButton;
    private Button _connectLocationsButton;
    private Button _removeLocationButton;
    private Toggle _autoPlaceToggle;

    /// <summary>
    /// Доступные биомы
    /// </summary>
    private List<string> _availableBiomes = new List<string>
    {
        "Grassland", "Forest", "Jungle", "MountainBase",
        "MountainMid", "MountainHigh", "MountainPeak", "Sand"
    };

    /// <summary>
    /// Флаг инициализации визуальных элементов.
    /// </summary>
    private bool _isVisualElementsInitiated;

    /// <summary>
    /// Инициализация контроллера карты.
    /// </summary>
    public void Initiate(VisualElement root, Plot plot)
    {
        Debug.Log("=== MapActionController.Initiate() начато ===");
        Debug.Log($"Корневой элемент: {root.name}");

        _plot = plot;
        _mapGenerator = GameObject.FindObjectOfType<MapGeneratorManual>();
        if (_mapGenerator == null)
        {
            Debug.LogError("MapGeneratorManual не найден на сцене.");
        }
        else
        {
            Debug.Log($"MapGeneratorManual найден: {_mapGenerator.gameObject.name}");
        }

        // ВЫВОДИМ ВСЕ ЭЛЕМЕНТЫ ДЛЯ ОТЛАДКИ
        DebugLogAllUIElements(root);

        InitiateVisualElements(root);

        Debug.Log("=== MapActionController.Initiate() завершено ===");
    }

    /// <summary>
    /// Выводит все UI элементы для отладки
    /// </summary>
    private void DebugLogAllUIElements(VisualElement root)
    {
        Debug.Log("=== ВСЕ UI ЭЛЕМЕНТЫ ===");

        // Ищем все кнопки
        var allButtons = root.Query<Button>().ToList();
        Debug.Log($"Найдено кнопок: {allButtons.Count}");
        foreach (var button in allButtons)
        {
            Debug.Log($"  Кнопка: имя='{button.name}', текст='{button.text}'");
        }

        // Ищем все TextField
        var allTextFields = root.Query<TextField>().ToList();
        Debug.Log($"Найдено TextField: {allTextFields.Count}");
        foreach (var field in allTextFields)
        {
            Debug.Log($"  TextField: имя='{field.name}', label='{field.label}', значение='{field.value}'");
        }

        // Ищем все DropdownField
        var allDropdowns = root.Query<DropdownField>().ToList();
        Debug.Log($"Найдено DropdownField: {allDropdowns.Count}");
        foreach (var dropdown in allDropdowns)
        {
            Debug.Log($"  Dropdown: имя='{dropdown.name}', значение='{dropdown.value}'");
        }

        // Ищем все Toggle
        var allToggles = root.Query<Toggle>().ToList();
        Debug.Log($"Найдено Toggle: {allToggles.Count}");
        foreach (var toggle in allToggles)
        {
            Debug.Log($"  Toggle: имя='{toggle.name}', значение={toggle.value}");
        }

        // Ищем все ListView
        var allListViews = root.Query<ListView>().ToList();
        Debug.Log($"Найдено ListView: {allListViews.Count}");
        foreach (var listView in allListViews)
        {
            Debug.Log($"  ListView: имя='{listView.name}'");
        }

        Debug.Log("=== КОНЕЦ СПИСКА ЭЛЕМЕНТОВ ===");
    }

    /// <summary>
    /// Инициализация начальных значений UI из компонента генерации или конфигурации.
    /// </summary>
    private void InitializeUIValues()
    {
        if (_mapGenerator == null) return;

        Debug.Log("Инициализация значений UI из MapGeneratorManual...");

        if (_widthSlider != null)
        {
            _widthSlider.value = _mapGenerator.mapWidth;
            Debug.Log($"WidthSlider установлен: {_widthSlider.value}");
        }
        if (_heightSlider != null)
        {
            _heightSlider.value = _mapGenerator.mapHeight;
            Debug.Log($"HeightSlider установлен: {_heightSlider.value}");
        }
        if (_chunkSizeTextField != null)
        {
            _chunkSizeTextField.value = _mapGenerator.chunkSize.ToString();
            Debug.Log($"ChunkSizeTextField установлен: {_chunkSizeTextField.value}");
        }
        if (_seedTextField != null)
        {
            _seedTextField.value = _mapGenerator.seed.ToString();
            Debug.Log($"SeedTextField установлен: {_seedTextField.value}");
        }
        if (_noizeScaleTextField != null)
        {
            _noizeScaleTextField.value = _mapGenerator.noiseScale.ToString();
            Debug.Log($"NoizeScaleTextField установлен: {_noizeScaleTextField.value}");
        }
        if (_octavesTextField != null)
        {
            _octavesTextField.value = _mapGenerator.octaves.ToString();
            Debug.Log($"OctavesTextField установлен: {_octavesTextField.value}");
        }
        if (_persistanceSlider != null)
        {
            _persistanceSlider.value = _mapGenerator.persistance;
            Debug.Log($"PersistanceSlider установлен: {_persistanceSlider.value}");
        }
        if (_lacunaritySlider != null)
        {
            _lacunaritySlider.value = _mapGenerator.lacunarity;
            Debug.Log($"LacunaritySlider установлен: {_lacunaritySlider.value}");
        }
        if (_noizeOffsetXTextField != null)
        {
            _noizeOffsetXTextField.value = _mapGenerator.noiseOffset.x.ToString();
            Debug.Log($"NoizeOffsetXTextField установлен: {_noizeOffsetXTextField.value}");
        }
        if (_noizeOffsetYTextField != null)
        {
            _noizeOffsetYTextField.value = _mapGenerator.noiseOffset.y.ToString();
            Debug.Log($"NoizeOffsetYTextField установлен: {_noizeOffsetYTextField.value}");
        }
    }

    /// <summary>
    /// Инициализация визуальных элементов UI.
    /// </summary>
    private void InitiateVisualElements(VisualElement root)
    {
        if (_isVisualElementsInitiated) return;
        _isVisualElementsInitiated = true;

        Debug.Log("=== ИНИЦИАЛИЗАЦИЯ ВИЗУАЛЬНЫХ ЭЛЕМЕНТОВ ===");

        // Существующие элементы
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
        _generateMapButton = root.Q<Button>("GenerateMapButton");
        _clearMapButton = root.Q<Button>("ClearMapButton");
        _locationsListView = root.Q<ListView>("LocationsListView");
        _mapViewVisualElement = root.Q<VisualElement>("MapViewVisualElement");

        Debug.Log($"Основные элементы: GenerateMapButton={_generateMapButton != null}, ClearMapButton={_clearMapButton != null}");
        Debug.Log($"ListView найден: {_locationsListView != null}");

        // НОВЫЕ ЭЛЕМЕНТЫ ДЛЯ УПРАВЛЕНИЯ ЛОКАЦИЯМИ
        // Пробуем разные варианты имен
        _locationNameField = root.Q<TextField>("LocationNameField");
        if (_locationNameField == null)
        {
            Debug.LogWarning("LocationNameField не найден, пробую другие варианты...");
            _locationNameField = root.Q<TextField>("locationNameField"); // с маленькой буквы
            _locationNameField = root.Q<TextField>("LocationName"); // без Field
            _locationNameField = root.Q<TextField>("location-name"); // через дефис
        }

        _biomeDropdown = root.Q<DropdownField>("BiomeDropdown");
        if (_biomeDropdown == null)
        {
            Debug.LogWarning("BiomeDropdown не найден, пробую другие варианты...");
            _biomeDropdown = root.Q<DropdownField>("biomeDropdown");
            _biomeDropdown = root.Q<DropdownField>("Biome");
        }

        _addLocationButton = root.Q<Button>("AddLocationButton");
        if (_addLocationButton == null)
        {
            Debug.LogWarning("AddLocationButton не найден, пробую другие варианты...");
            _addLocationButton = root.Q<Button>("addLocationButton");
            _addLocationButton = root.Q<Button>("AddLocation");
            _addLocationButton = root.Q<Button>("Add Location"); // с пробелом
            _addLocationButton = root.Q<Button>("Add");
        }

        _connectLocationsButton = root.Q<Button>("ConnectLocationsButton");
        if (_connectLocationsButton == null)
        {
            Debug.LogWarning("ConnectLocationsButton не найден, пробую другие варианты...");
            _connectLocationsButton = root.Q<Button>("connectLocationsButton");
            _connectLocationsButton = root.Q<Button>("ConnectLocations");
            _connectLocationsButton = root.Q<Button>("Connect Locations");
            _connectLocationsButton = root.Q<Button>("Connect");
        }

        _removeLocationButton = root.Q<Button>("RemoveLocationButton");
        if (_removeLocationButton == null)
        {
            Debug.LogWarning("RemoveLocationButton не найден, пробую другие варианты...");
            _removeLocationButton = root.Q<Button>("removeLocationButton");
            _removeLocationButton = root.Q<Button>("RemoveLocation");
            _removeLocationButton = root.Q<Button>("Remove Locations");
            _removeLocationButton = root.Q<Button>("Remove");
        }

        _autoPlaceToggle = root.Q<Toggle>("AutoPlaceToggle");
        if (_autoPlaceToggle == null)
        {
            Debug.LogWarning("AutoPlaceToggle не найден, пробую другие варианты...");
            _autoPlaceToggle = root.Q<Toggle>("autoPlaceToggle");
            _autoPlaceToggle = root.Q<Toggle>("AutoPlace");
        }

        Debug.Log($"Управление локациями: LocationNameField={_locationNameField != null}, AddLocationButton={_addLocationButton != null}");
        Debug.Log($"ConnectLocationsButton={_connectLocationsButton != null}, RemoveLocationButton={_removeLocationButton != null}");

        // Настройка Dropdown для биомов
        if (_biomeDropdown != null)
        {
            _biomeDropdown.choices = _availableBiomes;
            if (_biomeDropdown.choices.Count > 0)
                _biomeDropdown.value = _biomeDropdown.choices[0];
            Debug.Log($"BiomeDropdown настроен: {_biomeDropdown.choices.Count} биомов, значение: {_biomeDropdown.value}");
        }
        else
        {
            Debug.LogError("BiomeDropdown НЕ НАЙДЕН! Создайте DropdownField с именем 'BiomeDropdown' в UI Builder");
        }

        // Настройка ListView локаций
        if (_locationsListView != null)
        {
            _locationsListView.makeItem = MakeLocationListItem;
            _locationsListView.bindItem = BindLocationListItem;

            // Пробуем настроить множественный выбор
            try
            {
                _locationsListView.selectionType = SelectionType.Multiple;
                Debug.Log("ListView настроен на множественный выбор");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Не удалось установить множественный выбор: {e.Message}");
                Debug.Log("Работаем с одиночным выбором");
            }

            Debug.Log("ListView инициализирован");
        }
        else
        {
            Debug.LogError("LocationsListView НЕ НАЙДЕН! Создайте ListView с именем 'LocationsListView' в UI Builder");
        }

        // Регистрация обработчиков событий
        if (_generateMapButton != null)
        {
            _generateMapButton.clicked += OnGenerateMapButtonClicked;
            Debug.Log("Кнопка Generate подписана");
        }
        else
        {
            Debug.LogError("GenerateMapButton НЕ НАЙДЕН!");
        }

        if (_clearMapButton != null)
        {
            _clearMapButton.clicked += OnClearMapButtonClicked;
            Debug.Log("Кнопка Clear подписана");
        }

        if (_addLocationButton != null)
        {
            _addLocationButton.clicked += OnAddLocationButtonClicked;
            Debug.Log("Кнопка Add Location подписана");
        }
        else
        {
            Debug.LogError("AddLocationButton НЕ НАЙДЕН! Проверьте имя кнопки в UI Builder");
        }

        if (_connectLocationsButton != null)
        {
            _connectLocationsButton.clicked += OnConnectLocationsButtonClicked;
            Debug.Log("Кнопка Connect подписана");
        }

        if (_removeLocationButton != null)
        {
            _removeLocationButton.clicked += OnRemoveLocationButtonClicked;
            Debug.Log("Кнопка Remove подписана");
        }

        // Обновляем список локаций при старте
        UpdateLocationsList();

        Debug.Log("=== ИНИЦИАЛИЗАЦИЯ ЗАВЕРШЕНА ===");
    }

    /// <summary>
    /// Обработчик нажатия на кнопку генерации карты.
    /// </summary>
    private void OnGenerateMapButtonClicked()
    {
        Debug.Log("=== НАЖАТА КНОПКА 'Generate Map' ===");
        if (_mapGenerator == null)
        {
            Debug.LogError("MapGeneratorManual не найден.");
            return;
        }

        // Считываем значения из UI
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

        Debug.Log("Параметры установлены, вызываю GenerateManualMap()...");

        // Генерируем карту
        _mapGenerator.GenerateManualMap();

        // Обновляем список локаций
        UpdateLocationsList();

        Debug.Log("=== ГЕНЕРАЦИЯ КАРТЫ ЗАВЕРШЕНА ===");
    }

    /// <summary>
    /// Обработчик нажатия на кнопку очистки карты.
    /// </summary>
    private void OnClearMapButtonClicked()
    {
        Debug.Log("=== НАЖАТА КНОПКА 'Clear Map' ===");
        if (_mapGenerator == null)
        {
            Debug.LogError("MapGeneratorManual не найден.");
            return;
        }

        _mapGenerator.ClearMap();

        if (_locationsListView != null)
        {
            _locationsListView.Clear();
            _locationsListView.RefreshItems();
        }

        Debug.Log("Карта очищена");
    }

    /// <summary>
    /// Обработчик нажатия на кнопку добавления локации.
    /// </summary>
    private void OnAddLocationButtonClicked()
    {
        Debug.Log("=== КНОПКА 'Add Location' НАЖАТА ===");

        if (_mapGenerator == null)
        {
            Debug.LogError("MapGeneratorManual не найден.");
            return;
        }

        // Проверяем, что поля существуют
        if (_locationNameField == null)
        {
            Debug.LogError("Поле для имени локации не найдено!");
            return;
        }

        if (_biomeDropdown == null)
        {
            Debug.LogError("Dropdown для биома не найден!");
            return;
        }

        string locationName = _locationNameField.value?.Trim();
        string biome = _biomeDropdown.value;
        bool autoPlace = _autoPlaceToggle?.value ?? true;

        Debug.Log($"Введенные данные: имя='{locationName}', биом='{biome}', авторазмещение={autoPlace}");

        if (string.IsNullOrEmpty(locationName))
        {
            Debug.LogError("Введите имя локации");
            return;
        }

        if (string.IsNullOrEmpty(biome))
        {
            Debug.LogError("Выберите биом");
            return;
        }

        Debug.Log($"Добавляю локацию: {locationName}...");

        // Добавляем локацию
        _mapGenerator.AddLocationManually(locationName, biome, autoPlace);

        Debug.Log("Локация добавлена в MapGenerator");

        // Обновляем список локаций
        UpdateLocationsList();

        // Очищаем поле ввода
        _locationNameField.value = "";

        Debug.Log("=== ДОБАВЛЕНИЕ ЗАВЕРШЕНО ===");
    }

    /// <summary>
    /// Обработчик нажатия на кнопку подключения локаций.
    /// </summary>
    private void OnConnectLocationsButtonClicked()
    {
        Debug.Log("=== КНОПКА 'Connect Locations' НАЖАТА ===");

        if (_mapGenerator == null)
        {
            Debug.LogError("MapGeneratorManual не найден");
            return;
        }

        if (_locationsListView == null)
        {
            Debug.LogError("ListView не найден");
            return;
        }

        // Проверяем выбранные элементы
        if (_locationsListView.selectedIndex < 0)
        {
            Debug.LogError("Выберите хотя бы одну локацию в списке");
            Debug.Log($"Всего локаций: {_mapGenerator.locations.Count}");
            return;
        }

        // В старой версии UI Toolkit множественный выбор может не работать
        // Делаем простое соединение - соединяем выбранную локацию с последней добавленной
        int selectedIndex = _locationsListView.selectedIndex;
        Debug.Log($"Выбрана локация с индексом: {selectedIndex}");

        if (_mapGenerator.locations.Count < 2)
        {
            Debug.LogError("Для соединения нужно как минимум 2 локации");
            return;
        }

        // Соединяем выбранную локацию с первой локацией в списке (или последней)
        string location1 = _mapGenerator.locations[selectedIndex].locationName;
        string location2;

        if (selectedIndex == 0)
        {
            // Если выбрана первая, соединяем со второй
            location2 = _mapGenerator.locations[1].locationName;
        }
        else
        {
            // Иначе соединяем с первой
            location2 = _mapGenerator.locations[0].locationName;
        }

        Debug.Log($"Соединяю локации: '{location1}' и '{location2}'");
        _mapGenerator.ConnectLocations(location1, location2);

        Debug.Log("=== СОЕДИНЕНИЕ ЗАВЕРШЕНО ===");
    }

    /// <summary>
    /// Обработчик нажатия на кнопку удаления локации.
    /// </summary>
    private void OnRemoveLocationButtonClicked()
    {
        Debug.Log("=== КНОПКА 'Remove Location' НАЖАТА ===");

        if (_mapGenerator == null)
        {
            Debug.LogError("MapGeneratorManual не найден");
            return;
        }

        if (_locationsListView == null || _locationsListView.selectedIndex < 0)
        {
            Debug.LogError("Выберите локацию для удаления в списке");
            return;
        }

        int selectedIndex = _locationsListView.selectedIndex;
        Debug.Log($"Выбран индекс для удаления: {selectedIndex}");

        if (selectedIndex >= 0 && selectedIndex < _mapGenerator.locations.Count)
        {
            var locationName = _mapGenerator.locations[selectedIndex].locationName;
            Debug.Log($"Удаляю локацию: '{locationName}'");

            _mapGenerator.RemoveLocation(locationName);

            // Обновляем список локаций
            UpdateLocationsList();

            Debug.Log($"Локация '{locationName}' удалена");
        }

        Debug.Log("=== УДАЛЕНИЕ ЗАВЕРШЕНО ===");
    }

    /// <summary>
    /// Получение действия для обновления данных.
    /// </summary>
    public Action GetUpdateAction()
    {
        return UpdateLocationsList;
    }

    /// <summary>
    /// Обновление списка локаций.
    /// </summary>
    private void UpdateLocationsList()
    {
        Debug.Log("=== ОБНОВЛЕНИЕ СПИСКА ЛОКАЦИЙ ===");

        if (_mapGenerator == null)
        {
            Debug.LogError("MapGeneratorManual не найден");
            return;
        }

        if (_locationsListView == null)
        {
            Debug.LogError("ListView не найден");
            return;
        }

        Debug.Log($"Количество локаций в MapGeneratorManual: {_mapGenerator.locations.Count}");

        // Устанавливаем источник данных
        _locationsListView.itemsSource = _mapGenerator.locations;

        // Обновляем отображение
        _locationsListView.RefreshItems();

        // Выводим список локаций в консоль
        for (int i = 0; i < _mapGenerator.locations.Count; i++)
        {
            var loc = _mapGenerator.locations[i];
            Debug.Log($"{i}: '{loc.locationName}' ({loc.biome}) - связей: {loc.connectedLocations?.Count ?? 0}");
        }

        Debug.Log("=== ОБНОВЛЕНИЕ ЗАВЕРШЕНО ===");
    }

    /// <summary>
    /// Создание элемента списка для отображения локации.
    /// </summary>
    private VisualElement MakeLocationListItem()
    {
        var elementAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/ElementListItem.uxml");
        if (elementAsset != null)
        {
            return elementAsset.CloneTree();
        }

        // Если файл не найден, создаем простой элемент
        var element = new VisualElement();
        element.style.flexDirection = FlexDirection.Row;
        element.style.alignItems = Align.Center;
        element.style.paddingLeft = 10;
        element.style.paddingRight = 10;
        element.style.paddingTop = 5;
        element.style.paddingBottom = 5;

        var label = new Label();
        label.name = "ElementName";
        label.style.flexGrow = 1;
        label.style.unityTextAlign = UnityEngine.TextAnchor.MiddleLeft;

        var biomeLabel = new Label();
        biomeLabel.name = "BiomeLabel";
        biomeLabel.style.width = 100;
        biomeLabel.style.unityTextAlign = UnityEngine.TextAnchor.MiddleRight;
        biomeLabel.style.color = new Color(0.7f, 0.7f, 0.7f);

        element.Add(label);
        element.Add(biomeLabel);

        return element;
    }

    /// <summary>
    /// Привязка данных локации к элементу списка в UI.
    /// </summary>
    private void BindLocationListItem(VisualElement e, int i)
    {
        var labelUI = e.Q<Label>("ElementName");
        var biomeLabelUI = e.Q<Label>("BiomeLabel");
        var iconUI = e.Q<VisualElement>("ElementIcon");

        if (_mapGenerator == null || i < 0 || i >= _mapGenerator.locations.Count)
        {
            Debug.LogError($"Некорректный индекс: {i}, количество локаций: {_mapGenerator?.locations.Count}");
            return;
        }

        var location = _mapGenerator.locations[i];

        if (labelUI != null)
        {
            labelUI.text = location.locationName;
        }

        if (biomeLabelUI != null)
        {
            biomeLabelUI.text = location.biome;
        }

        // Иконка (если есть)
        if (iconUI != null)
        {
            var vectorImage = AssetDatabase.LoadAssetAtPath<VectorImage>("Assets/Icons/ElemTypeLocationIcon.svg");
            if (vectorImage != null)
            {
                iconUI.style.backgroundImage = new StyleBackground(vectorImage);
            }
        }
    }
}