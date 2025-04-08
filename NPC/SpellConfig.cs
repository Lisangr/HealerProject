using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SpellConfig", menuName = "Game/Spell Config")]
public class SpellConfig : ScriptableObject
{
    public List<SpellData> spells = new List<SpellData>();

    // Вспомогательный метод для получения данных заклинания по ID
    public SpellData GetSpellDataByID(string id)
    {
        return spells.Find(spell => spell.spellID == id);
    }

    // Вспомогательный метод для получения списка заклинаний по типу
    public List<SpellData> GetSpellsByType(bool isHealing = false, bool isBuff = false, bool isDebuff = false)
    {
        return spells.FindAll(spell => 
            (isHealing && spell.isHealing) || 
            (isBuff && spell.isBuff) || 
            (isDebuff && spell.isDebuff));
    }

    // Вспомогательный метод для получения списка заклинаний, которые можно применить на группу
    public List<SpellData> GetGroupSpells()
    {
        return spells.FindAll(spell => spell.affectAllGroupMembers);
    }
} 