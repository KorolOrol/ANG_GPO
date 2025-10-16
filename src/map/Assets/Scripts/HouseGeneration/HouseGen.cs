using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class HouseGen : MonoBehaviour
{
    public Vector2 position;
    public GameObject objectPrefab;

    Quaternion zero_rot = Quaternion.Euler(0, 0, 0);


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateHouse()
    {
        Debug.Log($"GenerateHouse()");

/*      float ran_x = Random.Range(-1000f, 1000f);
        float ran_z = Random.Range(-1000f, 1000f);
        float ran_rot = Random.Range(0f, 360f);*/

        objectPrefab.transform.localScale = new Vector3(10, 10, 10);
        GameObject obj = Instantiate(objectPrefab, new Vector3(0, 0, 0), zero_rot);

        obj.name = "TestOBJ";
        obj.tag = "GeneratedObject";
    }

    public void GeneratePart()
    {
        Debug.Log($"GeneratePart()");

        objectPrefab.transform.localScale = new Vector3(1, 1, 1);
        GameObject obj = Instantiate(objectPrefab, new Vector3(0, 0, 0), zero_rot);

        obj.name = "TestDoor";
        obj.tag = "GeneratedObject";
    }

    public void DeleteHouses()
    {
        GameObject[] previousObjects = GameObject.FindGameObjectsWithTag("GeneratedObject");

        Debug.Log($"Deleting {previousObjects.Length} objects");

        foreach (GameObject obj in previousObjects)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(obj);
            }
            else
            {
                Destroy(obj);
            }
        }
    }
}
