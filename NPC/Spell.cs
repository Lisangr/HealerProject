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
                List<CompanionNPC> companions = player.GetCompanions();
                foreach (var companion in companions)
                {
                    ApplyEffectToTarget(companion.gameObject);
                }
            }
        }
        else
        {
            ApplyEffectToTarget(target);
        }
    }

    protected virtual void ApplyEffectToTarget(GameObject targetObject)
    {
        HealthSystem healthSystem = targetObject.GetComponent<HealthSystem>();
        if (healthSystem == null) return;

        if (spellData.isHealing)
        {
            healthSystem.Heal((int)spellData.power);
        }
        else if (spellData.isBuff || spellData.isDebuff)
        {
            // Здесь можно добавить логику для эффектов усиления/ослабления
            // Например, создание и применение баффов/дебаффов
        }
        else
        {
            healthSystem.TakeDamage((int)spellData.power);
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