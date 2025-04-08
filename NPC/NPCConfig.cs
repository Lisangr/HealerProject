using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NPCConfig", menuName = "Game/NPC Config")]
public class NPCConfig : ScriptableObject
{

    public List<NPCData> npcs = new List<NPCData>();

    // Вспомогательный метод для получения данных НПС по ID
    public NPCData GetNPCDataByID(string id)
    {
        return npcs.Find(npc => npc.npcID == id);
    }
} 