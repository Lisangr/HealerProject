using UnityEngine;
using UnityEngine.EventSystems;

public class CameraZoom : MonoBehaviour
{
    public float zoomSpeed = 10f;
    public float minFOV = 15f;
    public float maxFOV = 90f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("CameraZoom script requires a Camera component on the same GameObject.");
        }
    }

    private void LateUpdate()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            //  урсор над UI Ц можно не обрабатывать игровой ввод
            return;
        }

        MapForComputers();       
    }

    private void MapForComputers()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            ZoomCamera(scrollInput);
        }
    }

    private void ZoomCamera(float increment)
    {
        if (mainCamera == null) return;

        float currentFOV = mainCamera.fieldOfView;
        float newFOV = currentFOV - increment * zoomSpeed;
        mainCamera.fieldOfView = Mathf.Clamp(newFOV, minFOV, maxFOV);
    }   
}