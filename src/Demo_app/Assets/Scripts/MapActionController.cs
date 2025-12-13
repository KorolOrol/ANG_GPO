using System;
using System.Collections.Generic;
using System.Globalization;
using BaseClasses.Model;
using MapScripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

/// <summary>
/// Скрипт для управления настройками карты и генерации карты.
/// </summary>
public class MapActionController : IActionController
{
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
    private TextField _noiseScaleTextField;

    /// <summary>
    /// Текстовое поле для количества октав.
    /// </summary>
    private TextField _octavesTextField;

    /// <summary>
    /// Слайдер для настройки персистентности.
    /// </summary>
    private Slider _persistenceSlider;

    /// <summary>
    /// Слайдер для настройки лакунарности.
    /// </summary>
    private Slider _lacunaritySlider;

    /// <summary>
    /// Переключатель для границ чанков.
    /// </summary>
    private Toggle _drawChunkBordersToggle;

    /// <summary>
    /// Переключатель для границ чанков.
    /// </summary>
    private Toggle _showChunkBiomesToggle;

    /// <summary>
    /// Текстовое поле для смещения шума по X.
    /// </summary>
    private TextField _noiseOffsetXTextField;

    /// <summary>
    /// Текстовое поле для смещения шума по Y.
    /// </summary>
    private TextField _noiseOffsetYTextField;

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

    // НОВЫЕ ПОЛЯ ДЛЯ УПРАВЛЕНИЯ ЛОКАЦИЯМИ
    private TextField _locationNameField;
    private DropdownField _biomeDropdown;
    private Button _addLocationButton;
    private Button _connectLocationsButton;
    private Button _removeLocationButton;

    private Image _map3DView;
    private RenderTexture _mapRenderTexture;
    private Camera _mapCamera;

    /// <summary>
    /// Доступные биомы
    /// </summary>
    private readonly List<string> _availableBiomes = new List<string>
    {
        "Grassland", "Forest", "Jungle", "MountainBase",
        "MountainMid", "MountainHigh", "MountainPeak", "Sand"
    };

    /// <summary>
    /// Флаг инициализации визуальных элементов.
    /// </summary>
    private bool _isVisualElementsInitiated;

    private void Setup3DMapView()
    {
        // Находим камеру для карты
        _mapCamera = GameObject.Find("MapCamera")?.GetComponent<Camera>();
        if (_mapCamera == null)
        {
            Debug.LogError("MapCamera не найдена! Создайте камеру с именем 'MapCamera'");
            return;
        }

        // Получаем Render Texture с камеры
        _mapRenderTexture = _mapCamera.targetTexture;
        if (_mapRenderTexture == null)
        {
            Debug.LogError("MapCamera не имеет назначенной Render Texture");
            return;
        }

        // Устанавливаем текстуру в UI элемент
        _map3DView.image = _mapRenderTexture;

        // При генерации карты обновляем позицию камеры
        _generateMapButton.clicked += () => {
            UpdateCameraPosition();
        };
    }

    private void UpdateCameraPosition()
    {
        if (_mapGenerator == null || _mapCamera == null) return;

        // Рассчитываем позицию камеры в зависимости от размера карты
        float mapSize = Mathf.Max(_mapGenerator.mapWidth, _mapGenerator.mapHeight);
        float cameraHeight = mapSize * 5f; // Высота камеры зависит от размера карты

        _mapCamera.transform.position = new Vector3(0, cameraHeight, 0);
        _mapCamera.orthographicSize = mapSize * 5f; // Масштабируем обзор
    }

    /// <summary>
    /// Инициализация контроллера карты.
    /// </summary>
    public void Initiate(VisualElement root, Plot plot)
    {
        _mapGenerator = Object.FindFirstObjectByType<MapGeneratorManual>();

        InitiateVisualElements(root);
    }

    /// <summary>
    /// Инициализация начальных значений UI из компонента генерации или конфигурации.
    /// </summary>
    private void InitializeUIValues()
    {
        if (_mapGenerator == null) return;

        if (_widthSlider != null)
        {
            _widthSlider.value = _mapGenerator.mapWidth;
        }
        if (_heightSlider != null)
        {
            _heightSlider.value = _mapGenerator.mapHeight;
        }
        if (_chunkSizeTextField != null)
        {
            _chunkSizeTextField.value = _mapGenerator.chunkSize.ToString();
        }
        if (_seedTextField != null)
        {
            _seedTextField.value = _mapGenerator.seed.ToString();
        }
        if (_noiseScaleTextField != null)
        {
            _noiseScaleTextField.value = _mapGenerator.noiseScale.ToString(CultureInfo.InvariantCulture);
        }
        if (_octavesTextField != null)
        {
            _octavesTextField.value = _mapGenerator.octaves.ToString();
        }
        if (_persistenceSlider != null)
        {
            _persistenceSlider.value = _mapGenerator.persistence;
        }
        if (_lacunaritySlider != null)
        {
            _lacunaritySlider.value = _mapGenerator.lacunarity;
        }
        if (_noiseOffsetXTextField != null)
        {
            _noiseOffsetXTextField.value = _mapGenerator.noiseOffset.x.ToString(CultureInfo.InvariantCulture);
        }
        if (_noiseOffsetYTextField != null)
        {
            _noiseOffsetYTextField.value = _mapGenerator.noiseOffset.y.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Инициализация визуальных элементов UI.
    /// </summary>
    private void InitiateVisualElements(VisualElement root)
    {
        if (_isVisualElementsInitiated) return;
        _isVisualElementsInitiated = true;

        _generateMapButton = root.Q<Button>("GenerateMapButton");
        _clearMapButton = root.Q<Button>("ClearMapButton");

        _widthSlider = root.Q<Slider>("WidthSlider");
        _heightSlider = root.Q<Slider>("HeightSlider");
        _chunkSizeTextField = root.Q<TextField>("ChunkSizeTextField");

        _seedTextField = root.Q<TextField>("SeedTextField");
        _noiseScaleTextField = root.Q<TextField>("NoiseScaleTextField");
        _octavesTextField = root.Q<TextField>("OctavesTextField");
        _persistenceSlider = root.Q<Slider>("PersistenceSlider");
        _lacunaritySlider = root.Q<Slider>("LacunaritySlider");

        _drawChunkBordersToggle = root.Q<Toggle>("DrawChunkBordersToggle");
        _showChunkBiomesToggle = root.Q<Toggle>("ShowChunkBiomesToggle");

        _noiseOffsetXTextField = root.Q<TextField>("NoiseOffsetXTextField");
        _noiseOffsetYTextField = root.Q<TextField>("NoiseOffsetYTextField");

        _locationsListView = root.Q<ListView>("LocationsListView");
        root.Q<VisualElement>("MapViewVisualElement");

        // НОВЫЕ ЭЛЕМЕНТЫ ДЛЯ УПРАВЛЕНИЯ ЛОКАЦИЯМИ
        _locationNameField = root.Q<TextField>("LocationNameField");
        _biomeDropdown = root.Q<DropdownField>("BiomeDropdown");
        _addLocationButton = root.Q<Button>("AddMapLocationButton");
        _connectLocationsButton = root.Q<Button>("ConnectLocationsButton");
        _removeLocationButton = root.Q<Button>("RemoveLocationButton");

        _map3DView = root.Q<Image>("Map3DView");

        _biomeDropdown.choices = _availableBiomes;
        if (_biomeDropdown.choices.Count > 0)
            _biomeDropdown.value = _biomeDropdown.choices[0];

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


        _generateMapButton.clicked += OnGenerateMapButtonClicked;
        _clearMapButton.clicked += OnClearMapButtonClicked;
        _addLocationButton.clicked += OnAddLocationButtonClicked;
        _connectLocationsButton.clicked += OnConnectLocationsButtonClicked;
        _removeLocationButton.clicked += OnRemoveLocationButtonClicked;

        UpdateLocationsList();

        Setup3DMapView();
    }

    /// <summary>
    /// Обработчик нажатия на кнопку генерации карты.
    /// </summary>
    private void OnGenerateMapButtonClicked()
    {
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
        if (_noiseScaleTextField != null && float.TryParse(_noiseScaleTextField.value, out float noiseScale))
        {
            _mapGenerator.noiseScale = noiseScale;
        }
        if (_octavesTextField != null && int.TryParse(_octavesTextField.value, out int octaves))
        {
            _mapGenerator.octaves = octaves;
        }

        if (_persistenceSlider != null) _mapGenerator.persistence = _persistenceSlider.value;
        if (_lacunaritySlider != null) _mapGenerator.lacunarity = _lacunaritySlider.value;

        _mapGenerator.drawChunkBorders = _drawChunkBordersToggle.value;
        _mapGenerator.showChunkBiomes = _showChunkBiomesToggle.value;

        if (_noiseOffsetXTextField != null && float.TryParse(_noiseOffsetXTextField.value, out float offsetX))
        {
            float offsetY = _mapGenerator.noiseOffset.y;
            if (_noiseOffsetYTextField != null && float.TryParse(_noiseOffsetYTextField.value, out float parsedY))
            {
                offsetY = parsedY;
            }
            _mapGenerator.noiseOffset = new Vector2(offsetX, offsetY);
        }

        // Генерируем карту
        _mapGenerator.GenerateManualMap();

        // Обновляем список локаций
        UpdateLocationsList();
    }

    /// <summary>
    /// Обработчик нажатия на кнопку очистки карты.
    /// </summary>
    private void OnClearMapButtonClicked()
    {
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

        Debug.Log($"Введенные данные: имя='{locationName}', биом='{biome}'");

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

        // Добавляем локацию
        _mapGenerator.AddLocationManually(locationName, biome);

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

        // Получаем все выбранные индексы
        List<int> selectedIndices = new List<int>();

        // Современный способ для множественного выбора
        if (_locationsListView.selectionType == SelectionType.Multiple)
        {
            try
            {
                foreach (var index in _locationsListView.selectedIndices)
                {
                    selectedIndices.Add(index);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Не удалось получить несколько выбранных индексов: {e.Message}");
                // Падаем обратно на одиночный выбор
                if (_locationsListView.selectedIndex >= 0)
                {
                    selectedIndices.Add(_locationsListView.selectedIndex);
                }
            }
        }
        else
        {
            // Старый способ - только один выбранный индекс
            if (_locationsListView.selectedIndex >= 0)
            {
                selectedIndices.Add(_locationsListView.selectedIndex);
            }
        }

        if (selectedIndices.Count < 2)
        {
            Debug.LogError($"Выберите минимум 2 локации для соединения (выбрано: {selectedIndices.Count})");
            return;
        }

        Debug.Log($"Выбрано локаций для соединения: {selectedIndices.Count}");

        // Собираем имена выбранных локаций
        List<string> selectedLocationNames = new List<string>();
        foreach (int index in selectedIndices)
        {
            if (index >= 0 && index < _mapGenerator.locations.Count)
            {
                selectedLocationNames.Add(_mapGenerator.locations[index].locationName);
            }
        }

        // Проверяем, что все выбранные локации размещены на карте
        bool allPlaced = true;
        foreach (string locName in selectedLocationNames)
        {
            if (!_mapGenerator.IsLocationPlaced(locName))
            {
                Debug.LogError($"Локация '{locName}' не размещена на карте");
                allPlaced = false;
            }
        }

        if (!allPlaced)
        {
            Debug.LogError("Не все выбранные локации размещены на карте");
            return;
        }

        // Вариант 1: Соединяем в цепочку (A-B, B-C, C-D)
        for (int i = 0; i < selectedLocationNames.Count - 1; i++)
        {
            string loc1 = selectedLocationNames[i];
            string loc2 = selectedLocationNames[i + 1];
            _mapGenerator.ConnectLocations(loc1, loc2);
            Debug.Log($"Соединены локации: '{loc1}' и '{loc2}'");
        }

        // Вариант 2: Замыкаем в кольцо (только если больше 2 локаций)
        if (selectedLocationNames.Count > 2)
        {
            string first = selectedLocationNames[0];
            string last = selectedLocationNames[selectedLocationNames.Count - 1];
            _mapGenerator.ConnectLocations(first, last);
            Debug.Log($"Замкнуто кольцо между '{first}' и '{last}'");
        }

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
            string locationName = _mapGenerator.locations[selectedIndex].locationName;
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
    private static VisualElement MakeLocationListItem()
    {
        var handle = Addressables.LoadAssetAsync<VisualTreeAsset>("Assets/UI/ElementListItem.uxml");
        handle.WaitForCompletion();
        var elementAsset = handle.Result;
        if (elementAsset != null)
        {
            return elementAsset.CloneTree();
        }

        // Если файл не найден, создаем простой элемент
        var element = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                alignItems = Align.Center,
                paddingLeft = 10,
                paddingRight = 10,
                paddingTop = 5,
                paddingBottom = 5
            }
        };

        var label = new Label
        {
            name = "ElementName",
            style =
            {
                flexGrow = 1,
                unityTextAlign = TextAnchor.MiddleLeft
            }
        };

        var biomeLabel = new Label
        {
            name = "BiomeLabel",
            style =
            {
                width = 100,
                unityTextAlign = TextAnchor.MiddleRight,
                color = new Color(0.7f, 0.7f, 0.7f)
            }
        };

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
        if (iconUI == null) return;
        var handle = Addressables.LoadAssetAsync<VectorImage>("Assets/Icons/ElemTypeLocationIcon.svg");
        handle.WaitForCompletion();
        var vectorImage = handle.Result;
        if (vectorImage != null)
        {
            iconUI.style.backgroundImage = new StyleBackground(vectorImage);
        }
    }
}