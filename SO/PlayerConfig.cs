using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Config", menuName = "Player Config", order = 55)]
public class PlayerConfig : ScriptableObject
{
    public List<PlayerData> players = new List<PlayerData>(); // Список данных игроков

    private static PlayerConfig _instance;
    public static PlayerConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<PlayerConfig>("PlayerConfig");
            }
            return _instance;
        }
    }
}