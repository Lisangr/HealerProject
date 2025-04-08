using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    public string sceneName; // �������� �����, ������� ���������� ������� ������� ��� �����

    private TerrainManager terrainManager;

    void Start()
    {
        terrainManager = FindObjectOfType<TerrainManager>();
        if (terrainManager == null)
            Debug.LogError("TerrainManager �� ������ � �����!");
    }

    private void OnTriggerEnter(Collider other)
    {
        // ��������������, ��� � ������ ���������� ��� "Player"
        if (other.CompareTag("Player"))
        {
            // �������� ���������, ��� ����� ������� � ����� ����� (����)
            terrainManager.OnPlayerSceneChange(sceneName);
        }
    }
}
