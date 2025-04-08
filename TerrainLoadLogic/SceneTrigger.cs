using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    public string sceneName; // Название сцены, которую необходимо считать текущей при входе

    private TerrainManager terrainManager;

    void Start()
    {
        terrainManager = FindObjectOfType<TerrainManager>();
        if (terrainManager == null)
            Debug.LogError("TerrainManager не найден в сцене!");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Предполагается, что у игрока установлен тег "Player"
        if (other.CompareTag("Player"))
        {
            // Сообщаем менеджеру, что игрок перешёл в новую сцену (зону)
            terrainManager.OnPlayerSceneChange(sceneName);
        }
    }
}
