using UnityEngine;
using UnityEngine.EventSystems;

public class CameraControllerForMainCamera : MonoBehaviour
{
    [Header("Ссылки")]
    public Transform player;      // Игрок
    public Transform targetPoint; // Точка, за которой следует камера (должна быть дочерним объектом игрока)

    [Header("Параметры камеры")]
    public float distance = 5.0f;   // Расстояние от камеры до targetPoint
    public float xSpeed = 120.0f;   // Скорость вращения по горизонтали
    public float ySpeed = 120.0f;   // Скорость вращения по вертикали
    public float yMinLimit = -20f;  // Минимальный угол по вертикали
    public float yMaxLimit = 80f;   // Максимальный угол по вертикали
    public float offsetRight = 2.0f; // Смещение камеры вправо
    public float offsetUp = 1.5f;    // Смещение камеры вверх

    private float x = 0.0f;
    private float y = 0.0f;

    void Start()
    {
        // Если player не назначен, ищем его автоматически по тегу "Player"
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("Player не найден в сцене! Проверьте тег 'Player'.");
                return;
            }
        }

        // Если targetPoint не назначен, используем позицию игрока
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
            // Если ни один объект не найден, используем player как targetPoint
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
            // Курсор над UI – можно не обрабатывать игровой ввод
            return;
        }

        if (player == null || targetPoint == null)
            return;

        // Обновляем углы вращения камеры на основе ввода мыши
        x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
        y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
        y = ClampAngle(y, yMinLimit, yMaxLimit);

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 offset = new Vector3(offsetRight, offsetUp, -distance);
        Vector3 desiredPosition = rotation * offset + targetPoint.position;

        // Исключаем коллайдер игрока из проверки
        int layerMask = ~(1 << LayerMask.NameToLayer("Player"));
        RaycastHit hit;
        if (Physics.Linecast(targetPoint.position, desiredPosition, out hit, layerMask))
        {
            desiredPosition = hit.point + hit.normal * 0.3f;
        }

        transform.position = desiredPosition;
        transform.rotation = rotation;
    }

    // Ограничение угла (для вертикального поворота)
    public static float ClampAngle(float angle, float min, float max)
    {
        while (angle < -360f)
            angle += 360f;
        while (angle > 360f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
