using UnityEngine;
using System.Collections;

public class SpellEffect : MonoBehaviour
{
    [SerializeField] private float duration = 2f; // Длительность эффекта
    private Transform target; // Цель, за которой следует эффект
    private Vector3 offset; // Смещение относительно цели
    private ParticleSystem[] particleSystems; // Все системы частиц в эффекте

    private void Awake()
    {
        // Получаем все системы частиц, находящиеся в дочерних объектах
        particleSystems = GetComponentsInChildren<ParticleSystem>();

        // Переключаем все системы частиц в режим мирового пространства и отключаем Looping
        foreach (var ps in particleSystems)
        {
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.loop = false; // Отключаем режим Looping, чтобы эффект проигрался один раз
        }
    }

    public void Initialize(Transform targetTransform, Vector3 spawnOffset)
    {
        if (targetTransform == null)
        {
            Debug.LogError("SpellEffect: Цель не может быть null!");
            Destroy(gameObject);
            return;
        }

        target = targetTransform;
        offset = spawnOffset;

        // Устанавливаем начальную позицию и поворот
        transform.position = target.position + offset;
        transform.rotation = target.rotation;

        // Запускаем корутину, которая через duration секунд остановит эффект и уничтожит объект
        StartCoroutine(DeactivateAfterDelay());
    }

    void Update()
    {
        // Если цель существует, обновляем позицию эффекта относительно цели
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(duration);

        // Останавливаем и очищаем все системы частиц
        foreach (var ps in particleSystems)
        {
            if (ps != null)
            {
                // Сразу останавливаем эмиссию и очищаем оставшиеся частицы
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        // Ждём один кадр, чтобы системы частиц обновились
        yield return null;

        // Удаляем игровой объект с эффектом
        Destroy(gameObject);
    }
}
