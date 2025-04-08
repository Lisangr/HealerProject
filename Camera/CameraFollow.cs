using UnityEngine;
using UnityEngine.EventSystems;

public class CameraControllerForMainCamera : MonoBehaviour
{
    [Header("������")]
    public Transform player;      // �����
    public Transform targetPoint; // �����, �� ������� ������� ������ (������ ���� �������� �������� ������)

    [Header("��������� ������")]
    public float distance = 5.0f;   // ���������� �� ������ �� targetPoint
    public float xSpeed = 120.0f;   // �������� �������� �� �����������
    public float ySpeed = 120.0f;   // �������� �������� �� ���������
    public float yMinLimit = -20f;  // ����������� ���� �� ���������
    public float yMaxLimit = 80f;   // ������������ ���� �� ���������
    public float offsetRight = 2.0f; // �������� ������ ������
    public float offsetUp = 1.5f;    // �������� ������ �����

    private float x = 0.0f;
    private float y = 0.0f;

    void Start()
    {
        // ���� player �� ��������, ���� ��� ������������� �� ���� "Player"
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("Player �� ������ � �����! ��������� ��� 'Player'.");
                return;
            }
        }

        // ���� targetPoint �� ��������, ���������� ������� ������
        if (targetPoint == null)
        {
            string[] faceNames = { "Face1", "Face2", "Face3", "Face4", "Face5" };
            foreach (string faceName in faceNames)
            {
                Transform face = player.Find(faceName);
                if (face != null)
                {
                    targetPoint = face;
                    break;
                }
            }
            // ���� �� ���� ������ �� ������, ���������� player ��� targetPoint
            if (targetPoint == null)
            {
                targetPoint = player;
            }
        }

        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // ������ ��� UI � ����� �� ������������ ������� ����
            return;
        }

        if (player == null || targetPoint == null)
            return;

        // ��������� ���� �������� ������ �� ������ ����� ����
        x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
        y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
        y = ClampAngle(y, yMinLimit, yMaxLimit);

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 offset = new Vector3(offsetRight, offsetUp, -distance);
        Vector3 desiredPosition = rotation * offset + targetPoint.position;

        // ��������� ��������� ������ �� ��������
        int layerMask = ~(1 << LayerMask.NameToLayer("Player"));
        RaycastHit hit;
        if (Physics.Linecast(targetPoint.position, desiredPosition, out hit, layerMask))
        {
            desiredPosition = hit.point + hit.normal * 0.3f;
        }

        transform.position = desiredPosition;
        transform.rotation = rotation;
    }

    // ����������� ���� (��� ������������� ��������)
    public static float ClampAngle(float angle, float min, float max)
    {
        while (angle < -360f)
            angle += 360f;
        while (angle > 360f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
