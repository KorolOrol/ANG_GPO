using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class HouseGen : MonoBehaviour
{
    public Vector2 position;
    public GameObject objectPrefab;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateHouse();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateHouse()
    {
        Debug.Log($"GenerateHouse()");
        objectPrefab.transform.localScale = new Vector3(100, 100, 100);
        Quaternion rotation = Quaternion.Euler(-90, 90, 0);
        GameObject obj = Instantiate(objectPrefab, new Vector3(0, 0, 0), rotation);

        obj.name = "TestOBJ";
        obj.tag = "GeneratedObject";
    }

}
