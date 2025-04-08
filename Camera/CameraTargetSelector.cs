using UnityEngine;

public class CameraTargetSelector : MonoBehaviour
{
    [Header("Настройки поиска цели")]
    [Tooltip("Максимальная дистанция для поиска цели")]
    public float maxTargetDistance = 100f;
    [Tooltip("Слой, в котором находятся враги")]
    public LayerMask targetLayerMask;

    // Текущая выбранная цель
    public Enemy CurrentTarget { get; private set; }

    void Update()
    {
        SelectTarget();
    }

    // Выбирает цель, на которую указывает центр экрана
    private void SelectTarget()
    {
        // Создаем луч из центра экрана
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        // Если луч пересекается с объектом в заданном слое
        if (Physics.Raycast(ray, out hit, maxTargetDistance, targetLayerMask))
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                CurrentTarget = enemy;
                return;
            }
        }
        // Если цель не найдена, сбрасываем значение
        CurrentTarget = null;
    }

    // Для наглядности можно отрисовать луч в редакторе
    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;
        Gizmos.color = Color.red;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * maxTargetDistance);
    }
}
