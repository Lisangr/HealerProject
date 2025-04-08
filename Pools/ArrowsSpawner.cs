using UnityEngine;

public class ArrowsSpawner : MonoBehaviour
{
    [SerializeField] private Transform arrowSpawnPoint;
    [SerializeField] private Camera mainCamera; // Камера для определения направления

    // Метод для спавна стрелы в направлении курсора
    public void SpawnArrow()
    {
        // Получаем стрелу из пула
        ProjectileMoveScript arrow = ArrowsPool.Instance.GetArrow();

        // Задаем позицию для стрелы
        arrow.transform.position = arrowSpawnPoint.position;

        // Вычисляем направление до курсора
        Vector3 direction = GetCursorDirection();

        // Задаем направление стрелы
        arrow.transform.rotation = Quaternion.LookRotation(direction);
    }

    // Метод для вычисления направления движения стрелы в сторону курсора
    private Vector3 GetCursorDirection()
    {
        // Получаем позицию курсора в пространстве экрана
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Определяем, попадает ли луч в сцену (например, на поверхность земли или другой объект)
        if (Physics.Raycast(ray, out hit))
        {
            // Вычисляем направление от точки спавна до позиции, куда указывает курсор
            Vector3 direction = hit.point - arrowSpawnPoint.position;
            direction.y = 0; // Игнорируем вертикальную компоненту, чтобы стрела летела горизонтально
            return direction.normalized; // Возвращаем нормализованное направление
        }

        // Если луч никуда не попал, направляем стрелу вперёд по умолчанию
        return arrowSpawnPoint.forward;
    }
}
