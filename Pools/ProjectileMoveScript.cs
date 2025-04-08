using UnityEngine;
using System;

public class ProjectileMoveScript : MonoBehaviour
{
    [Header("Настройки движения")]
    [Tooltip("Скорость движения снаряда")]
    public float speed = 20f;

    [Tooltip("Время жизни снаряда (в секундах)")]
    public float lifeTime = 5f;

    [Tooltip("Скорость поворота (для плавного наведения)")]
    public float rotateSpeed = 10f;

    [Tooltip("Цель, к которой направляется снаряд (если не задана, движется по направлению transform.forward)")]
    public Transform target;

    [Tooltip("Если включено, снаряд будет плавно поворачиваться в сторону цели")]
    public bool homing = false;

    // Делегат для возврата снаряда в пул
    public Action<ProjectileMoveScript> ReturnToPoolCallback;

    // Время активации снаряда
    private float spawnTime;

    /// <summary>
    /// Метод, который вызывается при включении объекта. Запоминает время спавна.
    /// </summary>
    private void OnEnable()
    {
        spawnTime = Time.time;
    }

    /// <summary>
    /// Основной метод обновления. Двигает снаряд в направлении цели или по transform.forward.
    /// При истечении времени жизни снаряд возвращается в пул.
    /// </summary>
    private void Update()
    {
        // Если время жизни истекло – возвращаем снаряд в пул.
        if (Time.time > spawnTime + lifeTime)
        {
            ReturnToPool();
            return;
        }

        Vector3 moveDirection;

        if (target != null)
        {
            // Вычисляем направление к цели
            Vector3 targetDirection = (target.position - transform.position).normalized;

            if (homing)
            {
                // Плавное поворот снаряда в сторону цели
                Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDirection, rotateSpeed * Time.deltaTime, 0f);
                moveDirection = newDir.normalized;
                transform.rotation = Quaternion.LookRotation(newDir);
            }
            else
            {
                // Мгновенное направление в сторону цели
                moveDirection = targetDirection;
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }
        }
        else
        {
            // Если цели нет, движемся по направлению transform.forward
            moveDirection = transform.forward;
        }

        // Перемещаем снаряд
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    /// <summary>
    /// Метод обработки столкновений.
    /// Здесь можно добавить дополнительные проверки (например, исключить столкновения с игроком).
    /// После столкновения снаряд возвращается в пул.
    /// </summary>
    /// <param name="collision">Информация о столкновении</param>
    private void OnCollisionEnter(Collision collision)
    {
        ReturnToPool();
    }

    /// <summary>
    /// Метод возвращает снаряд в пул.
    /// Если делегат ReturnToPoolCallback задан, он вызывается для возврата объекта в пул,
    /// иначе объект просто деактивируется.
    /// </summary>
    public void ReturnToPool()
    {
        // Сбрасываем цель, чтобы избежать дальнейшего наведения
        target = null;

        if (ReturnToPoolCallback != null)
        {
            ReturnToPoolCallback(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Устанавливает цель для снаряда.
    /// Метод принимает объект Enemy (или любой другой объект, содержащий Transform)
    /// и назначает его в качестве цели для наведения.
    /// </summary>
    /// <param name="enemy">Цель, к которой должен лететь снаряд</param>
    public void SetTarget(Enemy enemy)
    {
        if (enemy != null)
        {
            target = enemy.transform;
        }
        else
        {
            target = null;
        }
    }
}
