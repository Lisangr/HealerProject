using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Проверяем, что загружена нужная сцена
        if (scene.name == "2")
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        // Ищем объект с тегом "SpawnPoint" в загруженной сцене
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (spawnPoint != null)
        {
            // Предположим, что у вас игрок хранится в переменной player
            GameObject player = GameObject.FindWithTag("Player"); // либо сохранённая ссылка
            if (player != null)
            {
                player.transform.position = spawnPoint.transform.position;
                player.transform.rotation = spawnPoint.transform.rotation;
                Debug.Log("Игрок заспавнен в точке спавна сцены '2'.");
            }
            else
            {
                Debug.LogWarning("Игрок не найден! Проверьте тег 'Player' или сохранённую ссылку.");
            }
        }
        else
        {
            Debug.LogWarning("Точка спавна (SpawnPoint) не найдена в сцене '2'.");
        }
    }
}
