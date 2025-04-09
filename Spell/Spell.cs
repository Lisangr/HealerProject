using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spell : MonoBehaviour
{
    protected SpellData spellData;
    protected float currentCooldown;
    protected bool isCasting;
    protected GameObject caster;
    protected GameObject target;

    public void Initialize(SpellData data, GameObject spellCaster)
    {
        spellData = data;
        caster = spellCaster;
        currentCooldown = 0f;
        isCasting = false;
    }

    public virtual bool CanCast(GameObject targetObject)
    {
        if (currentCooldown > 0 || isCasting)
            return false;

        if (targetObject == null)
            return false;

        // Проверка дальности
        float distance = Vector3.Distance(caster.transform.position, targetObject.transform.position);
        if (distance > spellData.range)
            return false;

        // Проверка типа цели
        if (targetObject == caster && !spellData.canTargetSelf)
            return false;

        // Проверка на групповое заклинание
        if (spellData.requireGroupTarget)
        {
            Player player = caster.GetComponent<Player>();
            if (player != null)
            {
                List<CompanionNPC> companions = player.GetCompanions();
                if (!companions.Exists(c => c.gameObject == targetObject))
                    return false;
            }
        }

        return true;
    }

    public virtual void Cast(GameObject targetObject)
    {
        if (!CanCast(targetObject))
            return;

        target = targetObject;
        StartCoroutine(CastCoroutine());
    }

    protected virtual IEnumerator CastCoroutine()
    {
        isCasting = true;

        // Воспроизведение эффекта произнесения
        if (spellData.castVFXPrefab != null)
        {
            Instantiate(spellData.castVFXPrefab, caster.transform.position, caster.transform.rotation);
        }

        if (spellData.castSound != null)
        {
            AudioSource.PlayClipAtPoint(spellData.castSound, caster.transform.position);
        }

        // Ожидание времени произнесения
        yield return new WaitForSeconds(spellData.castTime);

        // Применение эффекта
        ApplyEffect();

        // Воспроизведение эффекта попадания
        if (spellData.impactVFXPrefab != null)
        {
            Instantiate(spellData.impactVFXPrefab, target.transform.position, target.transform.rotation);
        }

        if (spellData.impactSound != null)
        {
            AudioSource.PlayClipAtPoint(spellData.impactSound, target.transform.position);
        }

        // Установка перезарядки
        currentCooldown = spellData.cooldown;
        isCasting = false;

        // Запуск корутины перезарядки
        StartCoroutine(CooldownCoroutine());
    }
    protected virtual void ApplyEffect()
    {
        if (spellData.affectAllGroupMembers)
        {
            Player player = caster.GetComponent<Player>();
            if (player != null)
            {
                // Heal the player
                HealthSystem playerHealthSystem = player.GetComponent<HealthSystem>();
                if (playerHealthSystem != null)
                {
                    ApplyHealthEffect(playerHealthSystem);
                    CreateEffects(player.gameObject, true); // Main effect for player
                }

                // Heal all companions
                List<CompanionNPC> companions = player.GetCompanions();
                foreach (var companion in companions)
                {
                    HealthSystem companionHealth = companion.GetComponent<HealthSystem>();
                    if (companionHealth != null)
                    {
                        ApplyHealthEffect(companionHealth);
                        CreateEffects(companion.gameObject, false); // Additional effect for companions
                    }
                }
            }
        }
        else
        {
            // Apply the healing effect to the target
            ApplyHealthEffect(target.GetComponent<HealthSystem>());
        }
    }

    protected virtual void ApplyEffectToTarget(GameObject targetObject)
    {
        // Получаем HealthSystem цели
        HealthSystem targetHealthSystem = targetObject.GetComponent<HealthSystem>();
        
        // Получаем Player для проверки расстояния и группы
        Player player = targetObject.GetComponent<Player>();
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }

        // Если это групповое заклинание и цель - игрок
        if (spellData.affectAllGroupMembers && targetObject.CompareTag("Player") && player != null)
        {
            // Применяем эффект к игроку
            if (targetHealthSystem != null)
            {
                ApplyHealthEffect(targetHealthSystem);
                CreateEffects(targetObject, true); // true - это основной эффект
            }

            // Применяем эффект ко всем компаньонам в радиусе
            List<CompanionNPC> companions = player.GetCompanions();
            foreach (var companion in companions)
            {
                // Проверяем расстояние до компаньона
                float distance = Vector3.Distance(player.transform.position, companion.transform.position);
                if (distance <= spellData.range)
                {
                    HealthSystem companionHealth = companion.GetComponent<HealthSystem>();
                    if (companionHealth != null)
                    {
                        ApplyHealthEffect(companionHealth);
                        CreateEffects(companion.gameObject, false); // false - это дополнительный эффект
                        Debug.Log($"Заклинание применено к компаньону {companion.name} на расстоянии {distance}");
                    }
                }
                else
                {
                    Debug.Log($"Компаньон {companion.name} вне радиуса действия заклинания ({distance} > {spellData.range})");
                }
            }
        }
        // Если это одиночное заклинание
        else if (targetHealthSystem != null)
        {
            // Проверяем расстояние для одиночного заклинания
            if (player != null)
            {
                float distance = Vector3.Distance(player.transform.position, targetObject.transform.position);
                if (distance <= spellData.range)
                {
                    ApplyHealthEffect(targetHealthSystem);
                    CreateEffects(targetObject, true); // true - это основной эффект
                    Debug.Log($"Заклинание применено к цели {targetObject.name} на расстоянии {distance}");
                }
                else
                {
                    Debug.Log($"Цель {targetObject.name} вне радиуса действия заклинания ({distance} > {spellData.range})");
                }
            }
            else
            {
                ApplyHealthEffect(targetHealthSystem);
                CreateEffects(targetObject, true);
            }
        }
    }
    private void ApplyHealthEffect(HealthSystem healthSystem)
    {
        if (spellData.isHealing)
        {
            if (healthSystem != null)
            {
                healthSystem.Heal((int)spellData.power);
                Debug.Log($"Исцеление {spellData.power} здоровья применено к {healthSystem.gameObject.name}");
            }
            else
            {
                // Если HealthSystem не найден, пробуем получить CompanionNPC
                CompanionNPC companion = caster.GetComponent<CompanionNPC>();
                if (companion != null)
                {
                    companion.Heal((int)spellData.power);
                    Debug.Log($"(Fallback) Исцеление {spellData.power} здоровья применено к {companion.gameObject.name}");
                }
            }
        }
        else if (spellData.power > 0)
        {
            healthSystem.TakeDamage((int)spellData.power);
            Debug.Log($"Урон {spellData.power} нанесен {healthSystem.gameObject.name}");
        }
    }

    private void CreateEffects(GameObject target, bool isMainEffect)
    {
        // Создаем эффект попадания, если он есть
        if (spellData.impactVFXPrefab != null)
        {
            // Вычисляем смещение для эффекта
            Vector3 offset = Vector3.up * 1f;
            
            // Создаем эффект и инициализируем его
            GameObject effectInstance = Instantiate(spellData.impactVFXPrefab, 
                target.transform.position + offset, 
                spellData.impactVFXPrefab.transform.rotation);
            
            // Добавляем компонент SpellEffect, если его нет
            SpellEffect spellEffect = effectInstance.GetComponent<SpellEffect>();
            if (spellEffect == null)
            {
                spellEffect = effectInstance.AddComponent<SpellEffect>();
            }
            
            // Инициализируем эффект
            spellEffect.Initialize(target.transform, offset);

            // Если это не основной эффект, уменьшаем его размер
            if (!isMainEffect)
            {
                effectInstance.transform.localScale *= 0.7f; // Уменьшаем размер для дополнительных эффектов
            }
        }

        // Воспроизводим звук попадания, если он есть (только для основного эффекта)
        if (spellData.impactSound != null && isMainEffect)
        {
            AudioSource.PlayClipAtPoint(spellData.impactSound, target.transform.position);
        }
    }

    protected virtual IEnumerator CooldownCoroutine()
    {
        while (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            yield return null;
        }
        currentCooldown = 0;
    }

    public float GetCooldownProgress()
    {
        return 1f - (currentCooldown / spellData.cooldown);
    }

    public bool IsReady()
    {
        return currentCooldown <= 0 && !isCasting;
    }
} 