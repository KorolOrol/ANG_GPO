using UnityEngine;

public class Part : MonoBehaviour 
{
    public GameObject objectPrefab;
    public Vector3 position = new Vector3(0, 0, 0);

    public Part(GameObject obj, Vector3 pos)
    {
        objectPrefab = obj;
        position = pos;
    }

    public Part(GameObject obj)
    {
        objectPrefab = obj;
    }

    public Part()
    {
        objectPrefab = null;
    }
}
