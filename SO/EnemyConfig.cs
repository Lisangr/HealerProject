using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Enemies Config", menuName = "Enemy Config", order = 54)]
public class EnemyConfig : ScriptableObject
{
    public List<EnemyData> enemies = new List<EnemyData>(); // Список данных врагов
}