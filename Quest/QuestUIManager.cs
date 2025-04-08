using UnityEngine;

public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance;
    public QuestDisplay questDisplay;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
