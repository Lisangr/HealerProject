using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

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

        // Проверка типа цели
        if (targetObject.CompareTag("Enemy") && !spellData.canTargetEnemies)
            return false;

        if (targetObject.CompareTag("Ally") && !spellData.canTargetAllies)
            return false;

        if (targetObject == caster && !spellData.canTargetSelf)
            return false;

        // Проверка дальности
        float distance = Vector3.Distance(caster.transform.position, targetObject.transform.position);
        if (distance > spellData.range)
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

        // Эффект произнесения всегда спавнится на игроке
        Vector3 castPosition = caster.transform.position;
        Quaternion castRotation = caster.transform.rotation;

        if (spellData.castVFXPrefab != null)
        {
            // Можно также добавить автоудаление для CastVFX, если требуется
            GameObject castInstance = Instantiate(spellData.castVFXPrefab, castPosition, castRotation);
            SpellEffect castEffect = castInstance.GetComponent<SpellEffect>();
            if (castEffect == null)
                castEffect = castInstance.AddComponent<SpellEffect>();
            // Используем текущую позицию игрока без смещения (или задайте нужное смещение)
            castEffect.Initialize(caster.transform, Vector3.zero);
        }

        if (spellData.castSound != null)
        {
            AudioSource.PlayClipAtPoint(spellData.castSound, castPosition);
        }

        // Ожидание времени произнесения заклинания
        yield return new WaitForSeconds(spellData.castTime);

        // Применяем эффект – наносим урон или лечим
        ApplyEffect();

        // Создаём эффект попадания (ImpactVFX) через SpellEffect для автоудаления
        if (target != null)
        {
            if (spellData.impactVFXPrefab != null)
            {
                GameObject impactInstance = Instantiate(
                    spellData.impactVFXPrefab,
                    target.transform.position,
                    target.transform.rotation
                );
                // Добавляем компонент SpellEffect, если его нет, и инициализируем его,
                // чтобы через заданное время объект уничтожился
                SpellEffect impactEffect = impactInstance.GetComponent<SpellEffect>();
                if (impactEffect == null)
                {
                    impactEffect = impactInstance.AddComponent<SpellEffect>();
                }
                // Здесь можно задать смещение, например, поднять эффект немного вверх:
                impactEffect.Initialize(target.transform, Vector3.up);
            }

            if (spellData.impactSound != null)
            {
                AudioSource.PlayClipAtPoint(spellData.impactSound, target.transform.position);
            }
        }

        // Устанавливаем перезарядку и завершаем каст
        currentCooldown = spellData.cooldown;
        isCasting = false;
        StartCoroutine(CooldownCoroutine());
    }
    /// <summary>
    /// Изменённый метод: если заклинание наносит урон (не лечит) и цель – враг,
    /// то производится проверка всех объектов с тегом "Enemy" в заданой области вокруг цели.
    /// Каждый найденный враг получает урон, равный spellData.power.
    /// </summary>
    protected virtual void ApplyEffect()
    {
        // Если заклинание наносит урон и цель имеет тег "Enemy", делаем АОЕ повреждение
        if (!spellData.isHealing && target != null && target.CompareTag("Enemy"))
        {
            float aoeRadius = spellData.aoeRadius;
            Collider[] hitColliders = Physics.OverlapSphere(target.transform.position, aoeRadius);
            foreach (var collider in hitColliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    HealthSystem hs = collider.GetComponent<HealthSystem>();
                    if (hs != null)
                    {
                        hs.TakeDamage((int)spellData.power);
                        Debug.Log($"Урон {spellData.power} нанесён {hs.gameObject.name} (AOE)");
                    }
                }
            }
        }
        // Если заклинание предназначено для группы (например, исцеление всей группы)
        else if (spellData.affectAllGroupMembers)
        {
            Player player = caster.GetComponent<Player>();
            if (player != null)
            {
                // Применяем эффект к игроку
                HealthSystem playerHealthSystem = player.GetComponent<HealthSystem>();
                if (playerHealthSystem != null)
                {
                    ApplyHealthEffect(playerHealthSystem);
                    CreateEffects(player.gameObject, true); // Основной эффект для игрока
                }

                // Применяем эффект ко всем компаньонам
                List<CompanionNPC> companions = player.GetCompanions();
                foreach (var companion in companions)
                {
                    HealthSystem companionHealth = companion.GetComponent<HealthSystem>();
                    if (companionHealth != null)
                    {
                        ApplyHealthEffect(companionHealth);
                        CreateEffects(companion.gameObject, false); // Дополнительный эффект для компаньонов
                    }
                }
            }
        }
        // Иначе (например, если это лечебное заклинание или одиночное воздействие), применяем эффект к цели
        else
        {
            ApplyHealthEffect(target.GetComponent<HealthSystem>());
        }
    }

    protected virtual void ApplyEffectToTarget(GameObject targetObject)
    {
        HealthSystem targetHealthSystem = targetObject.GetComponent<HealthSystem>();

        // Применяем эффект к врагам
        if (targetObject.CompareTag("Enemy") && spellData.canTargetEnemies)
        {
            if (targetHealthSystem != null)
            {
                ApplyHealthEffect(targetHealthSystem);
                CreateEffects(targetObject, true);
            }
            return;
        }

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
                // Фолбэк: если по какой-то причине HealthSystem не найден, пытаемся получить его из объекта CompanionNPC
                CompanionNPC companion = caster.GetComponent<CompanionNPC>();
                if (companion != null)
                {
                    HealthSystem hs = companion.GetComponent<HealthSystem>();
                    if (hs != null)
                    {
                        hs.Heal((int)spellData.power);
                        Debug.Log($"(Fallback) Исцеление {spellData.power} здоровья применено к {hs.gameObject.name}");
                    }
                }
            }
        }
        else if (spellData.power > 0)
        {
            healthSystem.TakeDamage((int)spellData.power);
            Debug.Log($"Урон {spellData.power} нанесён {healthSystem.gameObject.name}");
        }
    }

    private void CreateEffects(GameObject target, bool isMainEffect)
    {
        // Создаем эффект попадания, если он есть
        if (spellData.impactVFXPrefab != null)
        {
            // Вычисляем смещение для эффекта с учетом типа цели
            Vector3 offset = Vector3.zero;
            float effectHeight = 1f;

            // Для врагов вычисляем смещение относительно их модели
            if (target.CompareTag("Enemy"))
            {
                Renderer renderer = target.GetComponent<Renderer>();
                if (renderer != null)
                {
                    // Берем половину высоты объекта из границ рендерера
                    effectHeight = renderer.bounds.extents.y;
                    offset = Vector3.up * effectHeight;
                }
            }
            else // Для других целей используем стандартное смещение
            {
                offset = Vector3.up * 1f;
            }

            // Создаем эффект и инициализируем его
            GameObject effectInstance = Instantiate(
                spellData.impactVFXPrefab,
                target.transform.position + offset,
                spellData.impactVFXPrefab.transform.rotation
            );

            // Настраиваем ориентацию эффекта для врагов
            if (target.CompareTag("Enemy"))
            {
                effectInstance.transform.LookAt(Camera.main.transform);
                effectInstance.transform.rotation = Quaternion.Euler(0, effectInstance.transform.eulerAngles.y, 0);
            }

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
                effectInstance.transform.localScale *= 0.7f;
            }
        }

        // Воспроизводим звук попадания, если он есть (только для основного эффекта)
        if (spellData.impactSound != null && isMainEffect)
        {
            AudioSource.PlayClipAtPoint(
                spellData.impactSound,
                target.transform.position
            );
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