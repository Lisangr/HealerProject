using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    private Camera cam;
    private Vector3 lastCameraPosition;
    private Quaternion lastCameraRotation;
    private float lastYawRotation;

    // �����, ��� ������� �� ����� ������� ������ (� ��������)
    public float rotationThreshold = 20f;

    void Start()
    {
        // ��������� ������ �� �������� ������
        cam = Camera.main;

        // ��������� ��������� ��������� ������
        if (cam != null)
        {
            lastCameraPosition = cam.transform.position;
            lastCameraRotation = cam.transform.rotation;
            lastYawRotation = GetYawAngle(cam.transform.rotation);
        }
    }

    void LateUpdate()
    {
        // ���� ������ �� �������, �������
        if (cam == null)
            return;

        // �������� ������� ���� �������� ������ �� ��� Y (� ��������)
        float currentYawRotation = GetYawAngle(cam.transform.rotation);

        // ���������, ���� ������� � ���� �������� ������ ���������� ��������
        float angleDifference = Mathf.Abs(currentYawRotation - lastYawRotation);

        // ���� ������ ����������� ������ ��� �� ��������� ��������, ������������ ������
        if (angleDifference > rotationThreshold)
        {
            // ��������� ���������� ������� ������������ ������
            transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward,
                             cam.transform.rotation * Vector3.up);

            // ��������� ��������� ���� �������� ������
            lastYawRotation = currentYawRotation;
        }
    }

    // ������� ��� ��������� ���� �������� �� ��� Y (� ��������)
    private float GetYawAngle(Quaternion rotation)
    {
        // �������� ���� �������� �� ��� Y
        Vector3 eulerAngles = rotation.eulerAngles;
        return eulerAngles.y;
    }
}
