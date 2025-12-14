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
    private Slider _sizeSlider;


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
    private DropdownField _cityModeDropdown;
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

        float mapWidthUnits = _mapGenerator.mapWidth * 10f;
        float mapHeightUnits = _mapGenerator.mapHeight * 10f;

        float aspectRatio = (float)_mapRenderTexture.width / _mapRenderTexture.height;

        // Правильный расчет orthographicSize для прямоугольной карты
        float widthBasedSize = mapWidthUnits / (2f * aspectRatio);
        float heightBasedSize = mapHeightUnits / 2f;
        float targetOrthoSize = Mathf.Max(widthBasedSize, heightBasedSize);

        // Высота камеры с небольшим отступом для лучшего обзора
        float cameraHeight = targetOrthoSize * 1.1f;

        // Позиционирование камеры
        _mapCamera.transform.position = new Vector3(0, cameraHeight, 0);
        _mapCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
        _mapCamera.orthographicSize = targetOrthoSize;

        Debug.Log($"Камера обновлена: позиция=({_mapCamera.transform.position}), " +
                  $"размер={_mapCamera.orthographicSize:F2}, " +
                  $"ширина карты={mapWidthUnits:F1}, высота карты={mapHeightUnits:F1}");
    }

    private void UpdateCameraPositionZoom()
    {
        if (_mapGenerator == null || _mapCamera == null) return;

        // Рассчитываем позицию камеры в зависимости от размера карты
        float mapSize = 100f;
        float cameraHeight = mapSize * 5f; // Высота камеры зависит от размера карты

        // Позиционируем камеру точно над центром карты
        _mapCamera.transform.position = new Vector3(0, cameraHeight, 0);
        _mapCamera.transform.rotation = Quaternion.Euler(90, 0, 0); // Смотрим прямо вниз

        // Рассчитываем orthographicSize для полного охвата карты
        float aspectRatio = (float)_mapRenderTexture.width / _mapRenderTexture.height;
        _mapCamera.orthographicSize = Mathf.Max(
            mapSize * 5f / aspectRatio,
            mapSize * 5f
        );

        Debug.Log($"Камера обновлена: позиция=({_mapCamera.transform.position}), размер={_mapCamera.orthographicSize}, соотношение={aspectRatio:F2}");
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

        if (_sizeSlider != null)
        {
            _sizeSlider.value = _mapGenerator.mapWidth;
        }
        if (_sizeSlider != null)
        {
            _sizeSlider.value = _mapGenerator.mapHeight;
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

        _sizeSlider = root.Q<Slider>("SizeSlider");
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
        _cityModeDropdown = root.Q<DropdownField>("CityModeDropdown");
        _addLocationButton = root.Q<Button>("AddMapLocationButton");
        _connectLocationsButton = root.Q<Button>("ConnectLocationsButton");
        _removeLocationButton = root.Q<Button>("RemoveLocationButton");

        _map3DView = root.Q<Image>("Map3DView");

        _biomeDropdown.choices = _availableBiomes;
        if (_biomeDropdown.choices.Count > 0)
            _biomeDropdown.value = _biomeDropdown.choices[0];

        _cityModeDropdown.choices = new List<string>()
        {
            "Simple Cubes", "Advanced with Sizes"
        };
        if (_cityModeDropdown.choices.Count > 0)
            _cityModeDropdown.value = _cityModeDropdown.choices[0];

        // Настройка ListView локаций
        if (_locationsListView != null)
        {
            _locationsListView.makeItem = MakeLocationListItem;
            _locationsListView.bindItem = BindLocationListItem;

            // Пробуем настроить множественный выбор
            try
            {
                _locationsListView.selectionType = SelectionType.Multiple;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Не удалось установить множественный выбор: {e.Message}");
            }

        }

        SetupMapClickHandler(root);

        _generateMapButton.clicked += OnGenerateMapButtonClicked;
        _clearMapButton.clicked += OnClearMapButtonClicked;
        _addLocationButton.clicked += OnAddLocationButtonClicked;
        _connectLocationsButton.clicked += OnConnectLocationsButtonClicked;
        _removeLocationButton.clicked += OnRemoveLocationButtonClicked;

        UpdateLocationsList();

        Setup3DMapView();
    }

    private void SetupMapClickHandler(VisualElement root)
    {
        // Находим контейнер с картой
        var mapContainer = root.Q<VisualElement>("MapContainer");

        if (mapContainer == null)
        {
            Debug.LogError("Элемент 'MapContainer' не найден в UI");
            return;
        }

        // Отладка размеров контейнера
        mapContainer.RegisterCallback<GeometryChangedEvent>(evt => {
            Debug.Log($"MapContainer размер: {mapContainer.layout}, видимость: {mapContainer.style.display}");
        });

        // Регистрируем обработчик кликов по контейнеру карты
        mapContainer.RegisterCallback<MouseDownEvent>(evt => {
            if (evt.button == 0) // Левая кнопка мыши
            {
                HandleMapClick(evt.localMousePosition, mapContainer);
            }
        });

        // Обработка возврата из зума при нажатии средней кнопки или escape
        mapContainer.RegisterCallback<MouseDownEvent>(evt => {
            if (evt.button == 2) // Средняя кнопка мыши
            {
                HandleReturnFromZoom();
            }
        });

        root.RegisterCallback<KeyDownEvent>(evt => {
            if (evt.keyCode == KeyCode.Escape)
            {
                HandleReturnFromZoom();
            }
        });
    }

    private void HandleMapClick(Vector2 screenPosition, VisualElement container)
    {
        if (_mapGenerator == null || _mapCamera == null ||
            _map3DView == null || _mapRenderTexture == null ||
            _mapRenderTexture.width == 0 || _mapRenderTexture.height == 0)
        {
            Debug.LogWarning("Компоненты карты не инициализированы или имеют нулевые размеры");
            return;
        }

        float containerWidth = _map3DView.layout.width;
        float containerHeight = _map3DView.layout.height;

        if (containerWidth <= 0 || containerHeight <= 0)
        {
            Debug.LogWarning("Контейнер карты имеет недопустимые размеры");
            return;
        }

        Vector2 localPosition = _map3DView.WorldToLocal(container.LocalToWorld(screenPosition));

        // КРИТИЧЕСКОЕ ИСПРАВЛЕНИЕ: ИСПОЛЬЗУЕМ СООТНОШЕНИЕ КАРТЫ, А НЕ RENDER TEXTURE
        float mapAspectRatio = (float)_mapGenerator.mapWidth / _mapGenerator.mapHeight;
        float containerRatio = containerWidth / containerHeight;

        float displayedWidth, displayedHeight;
        float offsetX = 0, offsetY = 0;

        // ПРАВИЛЬНЫЙ РАСЧЕТ ОТСТУПОВ НА ОСНОВЕ СООТНОШЕНИЯ КАРТЫ
        if (mapAspectRatio > containerRatio)
        {
            // Карта шире контейнера - отступы сверху и снизу
            displayedWidth = containerWidth;
            displayedHeight = containerWidth / mapAspectRatio;
            offsetY = (containerHeight - displayedHeight) / 2;
        }
        else
        {
            // Карта уже контейнера - отступы слева и справа
            displayedHeight = containerHeight;
            displayedWidth = containerHeight * mapAspectRatio;
            offsetX = (containerWidth - displayedWidth) / 2;
        }

        // ПРОВЕРКА ПОПАДАНИЯ В ВИДИМУЮ ОБЛАСТЬ
        if (localPosition.x < offsetX || localPosition.x > offsetX + displayedWidth ||
            localPosition.y < offsetY || localPosition.y > offsetY + displayedHeight)
        {
            Debug.Log("Клик вне области отображаемой карты");
            return;
        }

        // Нормализуем координаты ОТНОСИТЕЛЬНО ВИДИМОЙ ОБЛАСТИ
        float normalizedX = (localPosition.x - offsetX) / displayedWidth;
        float normalizedY = (localPosition.y - offsetY) / displayedHeight;

        normalizedX = Mathf.Clamp01(normalizedX);
        normalizedY = Mathf.Clamp01(normalizedY);

        // ПРАВИЛЬНЫЙ РАСЧЕТ КООРДИНАТ КАРТЫ
        int mapX = Mathf.RoundToInt(normalizedX * _mapGenerator.mapWidth);
        int mapY = Mathf.RoundToInt(normalizedY * _mapGenerator.mapHeight); // УБРАНО (1.0f - ...)

        // Проверка границ карты
        if (mapX < 0 || mapX >= _mapGenerator.mapWidth ||
            mapY < 0 || mapY >= _mapGenerator.mapHeight)
        {
            Debug.Log($"Клик вне карты: ({mapX}, {mapY}) не в пределах ({_mapGenerator.mapWidth}, {_mapGenerator.mapHeight})");
            return;
        }

        Debug.Log($"Контейнер: ширина={containerWidth}, высота={containerHeight}\n" +
            $"RenderTexture: ширина={_mapRenderTexture.width}, высота={_mapRenderTexture.height}\n" +
            $"Соотношение: карты={mapAspectRatio:F2}, контейнера={containerRatio:F2}\n" +
            $"Отступы: offsetX={offsetX:F1}, offsetY={offsetY:F1}\n" +
            $"Видимая область: ширина={displayedWidth:F1}, высота={displayedHeight:F1}");

        // КОРРЕКТНАЯ ФОРМУЛА ДЛЯ ПРЯМОУГОЛЬНОЙ КАРТЫ
        float worldX = (mapX - _mapGenerator.mapWidth / 2f) * 10f;
        float worldZ = -(mapY - _mapGenerator.mapHeight / 2f) * 10f; // ВЕРНУЛИ ОТРИЦАНИЕ

        Debug.Log($"Мировые координаты карты: ({worldX:F1}, 0, {worldZ:F1})");

        // Создаем луч из камеры в точку на карте
        Vector3 cameraPosition = _mapCamera.transform.position;
        Vector3 targetPosition = new Vector3(worldX, 0, worldZ);
        Ray ray = new Ray(cameraPosition, (targetPosition - cameraPosition).normalized);

        if (Physics.Raycast(ray, out RaycastHit hit, 10000f))
        {
            Debug.Log($"Попадание в объект: {hit.collider.name}, Позиция: {hit.point}");

            if (hit.collider.CompareTag("GeneratedObject"))
            {
                Debug.Log($"Клик по локации: {hit.collider.name}");
                _mapGenerator.HandleCityClick(hit.point);
                UpdateCameraPositionZoom();
            }
        }
        else
        {
            Debug.Log($"Клик в пустое место. Мировые координаты: ({worldX:F1}, 0, {worldZ:F1})");
        }
    }
    private void HandleReturnFromZoom()
    {
        // Проверяем, есть ли MapZoom в сцене
        var mapZoom = Object.FindFirstObjectByType<MapZoom>();
        if (mapZoom != null && mapZoom.gameObject.activeSelf)
        {
            mapZoom.ReturnToMainMap();
            UpdateCameraPosition();
        }
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

        if (!_mapGenerator.mapDisplay.gameObject.activeInHierarchy)
        {
            _mapGenerator.mapDisplay.gameObject.SetActive(true);
            _mapGenerator.SetChildrenVisibility(true);
            _mapGenerator.mapZoom.ClearAllBuildings();
            _mapGenerator.zoomDisplay.gameObject.SetActive(false);
        }

        // Считываем значения из UI
        if (_sizeSlider != null) _mapGenerator.mapWidth = (int)_sizeSlider.value;
        if (_sizeSlider != null) _mapGenerator.mapHeight = (int)_sizeSlider.value;
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

        if (_cityModeDropdown.value == "Simple Cubes")
        {
            _mapGenerator.mapZoom.buildingGenerationMethod = BuildingGenerationMethod.SimpleCubes;
        }

        if (_cityModeDropdown.value == "Advanced with Sizes")
        {
            _mapGenerator.mapZoom.buildingGenerationMethod = BuildingGenerationMethod.AdvancedWithSizes;
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

        if (!_mapGenerator.mapDisplay.gameObject.activeInHierarchy)
        {
            _mapGenerator.mapDisplay.gameObject.SetActive(true);
            _mapGenerator.SetChildrenVisibility(true);
            _mapGenerator.mapZoom.ClearAllBuildings();
            _mapGenerator.zoomDisplay.gameObject.SetActive(false);
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

        if (!_mapGenerator.mapDisplay.gameObject.activeInHierarchy)
        {
            Debug.LogError("Вы в режиме Zoom! MapDisplay не найден!");
            return;
        }

        string locationName = _locationNameField.value?.Trim();
        string biome = _biomeDropdown.value;

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
    }

    /// <summary>
    /// Обработчик нажатия на кнопку подключения локаций.
    /// </summary>
    private void OnConnectLocationsButtonClicked()
    {
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

        if (!_mapGenerator.mapDisplay.gameObject.activeInHierarchy)
        {
            Debug.LogError("Вы в режиме Zoom! MapDisplay не найден!");
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
    }

    /// <summary>
    /// Обработчик нажатия на кнопку удаления локации.
    /// </summary>
    private void OnRemoveLocationButtonClicked()
    {
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

        if (!_mapGenerator.mapDisplay.gameObject.activeInHierarchy)
        {
            Debug.LogError("Вы в режиме Zoom! MapDisplay не найден!");
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

        // Устанавливаем источник данных
        _locationsListView.itemsSource = _mapGenerator.locations;

        // Обновляем отображение
        _locationsListView.RefreshItems();

        // Выводим список локаций в консоль
        for (int i = 0; i < _mapGenerator.locations.Count; i++)
        {
            var loc = _mapGenerator.locations[i];
        }

        if (_cityModeDropdown.value == "Simple Cubes")
        {
            _mapGenerator.mapZoom.buildingGenerationMethod = BuildingGenerationMethod.SimpleCubes;
        }

        if (_cityModeDropdown.value == "Advanced with Sizes")
        {
            _mapGenerator.mapZoom.buildingGenerationMethod = BuildingGenerationMethod.AdvancedWithSizes;
        }
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