using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    private Camera cam;
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;
    private float lastYawRotation;

    // ѕорог, при котором мы будем вращать канвас (в градусах)
    public float rotationThreshold = 20f;

    void Start()
    {
        // —охран€ем ссылку на основную камеру
        cam = Camera.main;

        // —охран€ем начальное положение камеры
        if (cam != null)
        {
            lastCameraPosition = cam.transform.position;
            lastCameraRotation = cam.transform.rotation;
            lastYawRotation = GetYawAngle(cam.transform.rotation);
        }
    }

    void LateUpdate()
    {
        // ≈сли камера не найдена, выходим
        if (cam == null)
            return;

        // ѕолучаем текущий угол поворота камеры по оси Y (в градусах)
        float currentYawRotation = GetYawAngle(cam.transform.rotation);

        // ѕровер€ем, если разница в угле поворота больше порогового значени€
        float angleDifference = Mathf.Abs(currentYawRotation - lastYawRotation);

        // ≈сли камера повернулась больше чем на пороговое значение, поворачиваем канвас
        if (angleDifference > rotationThreshold)
        {
            // ќбновл€ем ориентацию канваса относительно камеры
            transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward,
                             cam.transform.rotation * Vector3.up);

            // ќбновл€ем последний угол поворота камеры
            lastYawRotation = currentYawRotation;
        }
    }

    // ‘ункци€ дл€ получени€ угла поворота по оси Y (в градусах)
    private float GetYawAngle(Quaternion rotation)
    {
        // ѕолучаем угол поворота по оси Y
        Vector3 eulerAngles = rotation.eulerAngles;
        return eulerAngles.y;
    }
}
