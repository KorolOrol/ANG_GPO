using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ModularBuildingGenerator : MonoBehaviour
{

    const float WALL_LENGTH = 2f;    // Длина стены (ось Z)
    const float WALL_HEIGHT = 2f;    // Высота стены (ось Y)
    const float WALL_THICKNESS = 0.5f; // Ширина стены (ось X)

    [Header("Prefabs")]
    public GameObject roofPrefab;

    public GameObject wallPrefab;
    public GameObject windowPrefab;
    public GameObject doorPrefab;

    public GameObject floorPrefab;

    [Header("Generation Settings")]

    public Vector2 minMaxSize = new Vector2(2, 11);

    [Range(1, 5)] public int minFloors = 1;
    [Range(1, 5)] public int maxFloors = 2;

    [Range(1, 11)] public int TestX = 2;
    [Range(1, 11)] public int TestZ = 2;

    private Vector3 buildingSize;
    private int floorsCount;

    private float doorChance = 0.2f;
    private float wallHeight = 2f;
    private float windowChance = 0.3f;

    private Transform currentBuilding;

    private GameObject northWallGroup;
    private GameObject southWallGroup;
    private GameObject westWallGroup;
    private GameObject eastWallGroup;

    void CreateRoof()
    {
        GameObject roofGroup = new GameObject("Roof");
        roofGroup.transform.SetParent(currentBuilding);

        Vector3 roofPosition = new Vector3(
            0.5f,
            floorsCount * wallHeight + 0.2f,
            0
        );

        GameObject roof = Instantiate(
            roofPrefab,
            roofPosition,
            Quaternion.identity,
            roofGroup.transform
        );

        roof.transform.localScale = new Vector3(
            buildingSize.x + 0.5f,
            1,
            buildingSize.z
        );
    }

    void CreateFoundation()
    {
        GameObject foundationGroup = new GameObject("Foundation");
        foundationGroup.transform.SetParent(currentBuilding);

        Vector3 foundationPosition = new Vector3(
            0.5f,
            0,
            0
        );

        GameObject foundation = Instantiate(
            floorPrefab,
            foundationPosition,
            Quaternion.identity,
            foundationGroup.transform
        );

        foundation.transform.localScale = new Vector3(
            buildingSize.x + 0.5f,
            0.4f,
            buildingSize.z
        );
    }

    public void GenerateBuilding()
    {
        if (currentBuilding != null)
        {
            DestroyImmediate(currentBuilding.gameObject);
        }

        // Создаем контейнер для здания
        CreateGroups();

        // Генерируем параметры
        floorsCount = Random.Range(minFloors, maxFloors + 1);
        buildingSize = new Vector3(
            (int)(Random.Range(minMaxSize.x, minMaxSize.y)),
            0,
            (int)(Random.Range(minMaxSize.x, minMaxSize.y))
        );

        CreateFoundation();
        CreateFloors();
        CreateRoof();
    }

    public void TestGenerateBuilding()
    {
        if (currentBuilding != null)
        {
            DestroyImmediate(currentBuilding.gameObject);
        }

        // Создаем контейнер для здания
        CreateGroups();

        // Генерируем параметры
        floorsCount = 1;
        buildingSize = new Vector3(
            (float)TestX,
            0,
            (float)TestZ
        );

        CreateFoundation();
        CreateFloors();
        CreateRoof();
    }

    void CreateGroups()
    {
        currentBuilding = new GameObject("Generated Building").transform;

        northWallGroup = new GameObject("North Wall");
        southWallGroup = new GameObject("South Wall");
        westWallGroup = new GameObject("West Wall");
        eastWallGroup = new GameObject("East Wall");

        northWallGroup.transform.SetParent(currentBuilding);
        southWallGroup.transform.SetParent(currentBuilding);
        westWallGroup.transform.SetParent(currentBuilding);
        eastWallGroup.transform.SetParent(currentBuilding);
    }

    void CreateFloor(int floorNumber)
    {
        float yPos = floorNumber * wallHeight + 0.2f;

        if (floorNumber != 0)
        {
            GameObject floorGroup = new GameObject($"Floor_{floorNumber}");
            floorGroup.transform.SetParent(currentBuilding);

            Vector3 floorPosition = new Vector3(
                0,
                floorNumber * wallHeight,
                -0.5f
            );

            GameObject floor = Instantiate(
                floorPrefab,
                floorPosition,
                Quaternion.identity,
                floorGroup.transform
            );

            floor.transform.localScale = new Vector3(
                buildingSize.x,
                0.4f,
                buildingSize.z - 0.5f
            );
        }

        // Генерация стен по периметру
        CreateWallRing(new Vector3(0, yPos, 0), buildingSize, floorNumber);
    }

    void CreateFloors()
    {
        for (int floor = 0; floor < floorsCount; floor++)
        {
            CreateFloor(floor);
        }
    }

    void CreateWallRing(Vector3 position, Vector3 size, int floorNumber)
    {
        // Рассчитываем позиции для стен
        float X = -buildingSize.x * 2;
        float Z = -buildingSize.z * 2;

        Debug.Log($"X: {buildingSize.x} | Y: {buildingSize.y} | Z: {buildingSize.z}");

        // Создаем стены для каждой стороны
        CreateWallSegment(new Vector3(X, position.y, Z), 180, size.x, floorNumber, northWallGroup.transform);   // Северная
        CreateWallSegment(new Vector3(X + 2, position.y, 0), 0, size.x, floorNumber, southWallGroup.transform);   // Южная
        CreateWallSegment(new Vector3(0, position.y, Z + 2), -90, size.z, floorNumber, westWallGroup.transform);  // Западная
        CreateWallSegment(new Vector3(X, position.y, Z), 90, size.z, floorNumber, eastWallGroup.transform);   // Восточная
    }

    void CreateWallSegment(Vector3 position, float rotation, float length, int floorNumber, Transform group)
    {
        int wallsCount = Mathf.CeilToInt(length);
        bool doorSpawned = false;

        if (Mathf.Abs(rotation) == 90)
        {
            for (int i = 0; i < wallsCount; i++)
            {
                GameObject prefabOfWallDoorWindow;
                float rand = Random.value;

                if (floorNumber == 0 && !doorSpawned && rand < doorChance)
                {
                    prefabOfWallDoorWindow = doorPrefab;
                    doorSpawned = true;
                }
                else if (rand < windowChance)
                {
                    prefabOfWallDoorWindow = windowPrefab;
                }
                else // Выбор между стеной с окном и без
                {
                    prefabOfWallDoorWindow = wallPrefab;
                }

                Vector3 wallPosition = position + new Vector3(0, 0, (i * 2));

                Debug.Log($"{group.name} | CNT: {wallsCount} | I: {i}\n WallPos: {wallPosition} | Pos: {position}");

                GameObject wall = Instantiate(
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

                if (floorNumber == 0 && !doorSpawned && rand < doorChance)
                {
                    prefabOfWallDoorWindow = doorPrefab;
                    doorSpawned = true;
                }
                else if (rand < windowChance)
                {
                    prefabOfWallDoorWindow = windowPrefab;
                }
                else // Выбор между стеной с окном и без
                {
                    prefabOfWallDoorWindow = wallPrefab;
                }

                Vector3 wallPosition = position + new Vector3((i * 2), 0, 0);

                Debug.Log($"{group.name} | CNT: {wallsCount} | I: {i}\n WallPos: {wallPosition} | Pos: {position}");

                GameObject wall = Instantiate(
                    prefabOfWallDoorWindow,
                    wallPosition,
                    Quaternion.Euler(0, rotation, 0),
                    group
                );
            }
        }
    }

    public void DestroyBuilding()
    {
        if (currentBuilding != null)
        {
            DestroyImmediate(currentBuilding.gameObject);
        }
    }
}