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
        
        // Плавно уменьшаем эмиссию частиц
        float fadeTime = 0.5f;
        float elapsedTime = 0f;
        
        // Сохраняем начальные значения эмиссии
        var initialEmissionRates = new float[particleSystems.Length];
        for (int i = 0; i < particleSystems.Length; i++)
        {
            var emission = particleSystems[i].emission;
            initialEmissionRates[i] = emission.rateOverTime.constant;
        }
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeTime;
            
            // Плавно уменьшаем эмиссию частиц
            for (int i = 0; i < particleSystems.Length; i++)
            {
                var emission = particleSystems[i].emission;
                emission.rateOverTime = initialEmissionRates[i] * (1f - t);
            }
            
            yield return null;
        }

        // Ждем, пока все частицы исчезнут
        bool anyParticlesAlive;
        do
        {
            anyParticlesAlive = false;
            foreach (var ps in particleSystems)
            {
                if (ps.particleCount > 0)
                {
                    anyParticlesAlive = true;
                    break;
                }
            }
            yield return null;
        } while (anyParticlesAlive);

        Destroy(gameObject);
    }
} 