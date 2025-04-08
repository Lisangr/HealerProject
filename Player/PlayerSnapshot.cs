using UnityEngine;

[System.Serializable]
public class PlayerSnapshot
{
    public float timeStamp;
    public Vector3 position;
    public Quaternion rotation;
    public int currentHealth;
    public int currentExp;
    public int currentLevel;
    public int statPoints;
    public int expForLevelUp;
}
