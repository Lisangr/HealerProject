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
        // Получаем все системы частиц в эффекте
        particleSystems = GetComponentsInChildren<ParticleSystem>();
        
        // Устанавливаем все системы частиц в режим мирового пространства
        foreach (var ps in particleSystems)
        {
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
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
        
        // Устанавливаем начальную позицию
        transform.position = target.position + offset;
        transform.rotation = target.rotation;

        // Запускаем корутину для отключения
        StartCoroutine(DeactivateAfterDelay());
    }

    private void Update()
    {
        // Если есть цель, следуем за ней
        if (target != null)
        {
            // Обновляем позицию и поворот
            transform.position = target.position + offset;
            transform.rotation = target.rotation;
        }
        else
        {
            // Если цель потеряна, уничтожаем эффект
            Destroy(gameObject);
        }
    }

    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(duration);     
        Destroy(gameObject);
    }
} 