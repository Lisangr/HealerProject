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
        // ���������, ��� ��������� ������ �����
        if (scene.name == "2")
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        // ���� ������ � ����� "SpawnPoint" � ����������� �����
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (spawnPoint != null)
        {
            // �����������, ��� � ��� ����� �������� � ���������� player
            GameObject player = GameObject.FindWithTag("Player"); // ���� ���������� ������
            if (player != null)
            {
                player.transform.position = spawnPoint.transform.position;
                player.transform.rotation = spawnPoint.transform.rotation;
                Debug.Log("����� ��������� � ����� ������ ����� '2'.");
            }
            else
            {
                Debug.LogWarning("����� �� ������! ��������� ��� 'Player' ��� ���������� ������.");
            }
        }
        else
        {
            Debug.LogWarning("����� ������ (SpawnPoint) �� ������� � ����� '2'.");
        }
    }
}
