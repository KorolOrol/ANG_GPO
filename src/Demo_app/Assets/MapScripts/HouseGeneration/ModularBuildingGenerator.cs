using UnityEngine;
using UnityEngine.Serialization;

namespace MapScripts.HouseGeneration
{
    public class ModularBuildingGenerator : MonoBehaviour
    {

        const float WALL_LENGTH = 2f;    // Длина стены (ось Z)
        const float WALL_HEIGHT = 2f;    // Высота стены (ось Y)
        const float WALL_THICKNESS = 0.5f; // Ширина стены (ось X)

        [Header("Prefabs")] // Префабы
        public GameObject roofPrefab;
        public GameObject wallPrefab;
        public GameObject windowPrefab;
        public GameObject doorPrefab;
        public GameObject floorPrefab;

        [Header("Generation Settings")] // Настройки генерации

        public Vector2 minMaxSize = new Vector2(2, 11);

        [Range(1, 5)] public int minFloors = 1;
        [Range(1, 5)] public int maxFloors = 2;

        [FormerlySerializedAs("TestX")] [Range(1, 11)] public int testX = 2;
        [FormerlySerializedAs("TestZ")] [Range(1, 11)] public int testZ = 2;

        private Vector3 _buildingSize;
        private int _floorsCount;

        private const float DoorChance = 0.2f;
        private const float WallHeight = 2f;
        private const float WindowChance = 0.3f;

        private Transform _currentBuilding;

        private GameObject _northWallGroup;
        private GameObject _southWallGroup;
        private GameObject _westWallGroup;
        private GameObject _eastWallGroup;

        /// <summary>
        /// Создание крыши
        /// </summary>
        private void CreateRoof()
        {
            var roofGroup = new GameObject("Roof");
            roofGroup.transform.SetParent(_currentBuilding);

            var roofPosition = new Vector3(
                0.5f,
                _floorsCount * WallHeight + 0.2f,
                0
            );

            var roof = Instantiate(
                roofPrefab,
                roofPosition,
                Quaternion.identity,
                roofGroup.transform
            );

            roof.transform.localScale = new Vector3(
                _buildingSize.x + 0.5f,
                1,
                _buildingSize.z
            );
        }

        /// <summary>
        /// Создание фундамента
        /// </summary>
        private void CreateFoundation()
        {
            var foundationGroup = new GameObject("Foundation");
            foundationGroup.transform.SetParent(_currentBuilding);

            var foundationPosition = new Vector3(
                0.5f,
                0,
                0
            );

            var foundation = Instantiate(
                floorPrefab,
                foundationPosition,
                Quaternion.identity,
                foundationGroup.transform
            );

            foundation.transform.localScale = new Vector3(
                _buildingSize.x + 0.5f,
                0.4f,
                _buildingSize.z
            );
        }

        /// <summary>
        /// Создание здания
        /// </summary>
        public void GenerateBuilding()
        {
            if (_currentBuilding != null)
            {
                DestroyImmediate(_currentBuilding.gameObject);
            }

            CreateGroups();

            _floorsCount = Random.Range(minFloors, maxFloors + 1);
            _buildingSize = new Vector3(
                (int)(Random.Range(minMaxSize.x, minMaxSize.y)),
                0,
                (int)(Random.Range(minMaxSize.x, minMaxSize.y))
            );

            CreateFoundation();
            CreateFloors();
            CreateRoof();
        }

        /// <summary>
        /// Создание тестового здания (с заданием его параметров)
        /// </summary>
        public void TestGenerateBuilding()
        {
            if (_currentBuilding != null)
            {
                DestroyImmediate(_currentBuilding.gameObject);
            }

            CreateGroups();

            _floorsCount = 1;
            _buildingSize = new Vector3(
                testX,
                0,
                testZ
            );

            CreateFoundation();
            CreateFloors();
            CreateRoof();
        }

        /// <summary>
        /// Создаём группы для удобства
        /// </summary>
        private void CreateGroups()
        {
            _currentBuilding = new GameObject("Generated Building").transform;

            _northWallGroup = new GameObject("North Wall");
            _southWallGroup = new GameObject("South Wall");
            _westWallGroup = new GameObject("West Wall");
            _eastWallGroup = new GameObject("East Wall");

            _northWallGroup.transform.SetParent(_currentBuilding);
            _southWallGroup.transform.SetParent(_currentBuilding);
            _westWallGroup.transform.SetParent(_currentBuilding);
            _eastWallGroup.transform.SetParent(_currentBuilding);
        }

        /// <summary>
        /// Создание этажа
        /// </summary>
        /// <param name="floorNumber">Номер этажа</param>
        private void CreateFloor(int floorNumber)
        {
            float yPos = floorNumber * WallHeight + 0.2f;

            if (floorNumber != 0)
            {
                var floorGroup = new GameObject($"Floor_{floorNumber}");
                floorGroup.transform.SetParent(_currentBuilding);

                var floorPosition = new Vector3(
                    0,
                    floorNumber * WallHeight,
                    -0.5f
                );

                var floor = Instantiate(
                    floorPrefab,
                    floorPosition,
                    Quaternion.identity,
                    floorGroup.transform
                );

                floor.transform.localScale = new Vector3(
                    _buildingSize.x,
                    0.4f,
                    _buildingSize.z - 0.5f
                );
            }

            // Генерация стен по периметру
            CreateWallRing(new Vector3(0, yPos, 0), _buildingSize, floorNumber);
        }

        /// <summary>
        /// Создание этажей
        /// </summary>
        private void CreateFloors()
        {
            for (int floor = 0; floor < _floorsCount; floor++)
            {
                CreateFloor(floor);
            }
        }

        /// <summary>
        /// Создание стен по всем сторонам света (кольцо)
        /// </summary>
        /// <param name="position">Позиция здания</param>
        /// <param name="size">Размер здания</param>
        /// <param name="floorNumber">Номер этажа</param>
        private void CreateWallRing(Vector3 position, Vector3 size, int floorNumber)
        {
            // Рассчитываем позиции для стен
            float x = -_buildingSize.x * 2;
            float z = -_buildingSize.z * 2;

            Debug.Log($"X: {_buildingSize.x} | Y: {_buildingSize.y} | Z: {_buildingSize.z}");

            // Создаем стены для каждой стороны
            CreateWallSegment(new Vector3(x, position.y, z), 180, size.x, floorNumber,
                _northWallGroup.transform); // Северная
            CreateWallSegment(new Vector3(x + 2, position.y, 0), 0, size.x, floorNumber,
                _southWallGroup.transform); // Южная
            CreateWallSegment(new Vector3(0, position.y, z + 2), -90, size.z, floorNumber,
                _westWallGroup.transform); // Западная
            CreateWallSegment(new Vector3(x, position.y, z), 90, size.z, floorNumber,
                _eastWallGroup.transform); // Восточная
        }

        /// <summary>
        /// Создание стены одной стороны света
        /// </summary>
        /// <param name="position">Позиция здания</param>
        /// <param name="rotation">Поворот стены (какая сторона света)</param>
        /// <param name="length">Длина стены</param>
        /// <param name="floorNumber">Номер этажа</param>
        /// <param name="group">Группа стены</param>
        private void CreateWallSegment(Vector3 position,
            float rotation,
            float length,
            int floorNumber,
            Transform group)
        {
            int wallsCount = Mathf.CeilToInt(length);
            bool doorSpawned = false;

            if (Mathf.Approximately(Mathf.Abs(rotation), 90))
            {
                for (int i = 0; i < wallsCount; i++)
                {
                    GameObject prefabOfWallDoorWindow;
                    float rand = Random.value;

                    if (floorNumber == 0 && !doorSpawned && rand < DoorChance)
                    {
                        prefabOfWallDoorWindow = doorPrefab;
                        doorSpawned = true;
                    }
                    else if (rand < WindowChance)
                    {
                        prefabOfWallDoorWindow = windowPrefab;
                    }
                    else // Выбор между стеной с окном и без
                    {
                        prefabOfWallDoorWindow = wallPrefab;
                    }

                    var wallPosition = position + new Vector3(0, 0, (i * 2));

                    Debug.Log($"{group.name} | CNT: {wallsCount} | " +
                        $"I: {i}\n WallPos: {wallPosition} | Pos: {position}");

                    Instantiate(
                        prefabOfWallDoorWindow,
                        wallPosition,
                        Quaternion.Euler(0, rotation, 0),
                        group
                    );
                }
            }
            else
            {
                for (int i = 0; i < wallsCount; i++)
                {
                    GameObject prefabOfWallDoorWindow;
                    float rand = Random.value;

                    if (floorNumber == 0 && !doorSpawned && rand < DoorChance)
                    {
                        prefabOfWallDoorWindow = doorPrefab;
                        doorSpawned = true;
                    }
                    else if (rand < WindowChance)
                    {
                        prefabOfWallDoorWindow = windowPrefab;
                    }
                    else // Выбор между стеной с окном и без
                    {
                        prefabOfWallDoorWindow = wallPrefab;
                    }

                    var wallPosition = position + new Vector3((i * 2), 0, 0);

                    Debug.Log($"{group.name} | CNT: {wallsCount} | " +
                        $"I: {i}\n WallPos: {wallPosition} | Pos: {position}");
                    
                    Instantiate(
                        prefabOfWallDoorWindow,
                        wallPosition,
                        Quaternion.Euler(0, rotation, 0),
                        group
                    );
                }
            }
        }

        /// <summary>
        /// Убрать предыдущее сгенерированное здание
        /// </summary>
        public void DestroyBuilding()
        {
            if (_currentBuilding != null)
            {
                DestroyImmediate(_currentBuilding.gameObject);
            }
        }
    }
}