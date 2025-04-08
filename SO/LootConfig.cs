using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Loot Config", menuName = "Loot Config", order = 56)]
public class LootConfig : ScriptableObject
{
    public List<LootItem> items = new List<LootItem>();
}