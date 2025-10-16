using UnityEngine;

public class PartsSerialization : MonoBehaviour
{
    public GameObject doorPrefab;
    public Part door = new Part();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        door.objectPrefab = doorPrefab;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
