using UnityEngine;

public class CamerasController : MonoBehaviour
{
    private Camera MainCamera;
    public Camera CityCamera;

    public GameObject ZoomPlane;
    private MapZoom MZ;

    private void Start()
    {
        MainCamera = Camera.main;
        MZ = ZoomPlane.GetComponent<MapZoom>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(2))
        {
            ChangeCamera();
            MZ.HandleBackClick();
        }
    }

public void ChangeCamera()
    {
        MainCamera.enabled = !MainCamera.enabled;
        CityCamera.enabled = !CityCamera.enabled;
    }
}
