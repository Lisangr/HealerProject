using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class TerrainManager : MonoBehaviour
{
    // ���������� ���������
    public static TerrainManager Instance { get; private set; }

    void Awake()
    {
        // ���� ��������� ��� ����������, ���������� ��������
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // ���� ����� ��������� �������� ����� �������:
        DontDestroyOnLoad(gameObject);
    }

    // ����� ��� �������� ����� � � �������
    [System.Serializable]
    public class SceneNeighbors
    {
        public string sceneName;             // ��� �����
        public List<string> neighbors;       // ����� �������� ���� (��������, �� ������, ������� � �.�.)
    }

    // ���� ����, ������� ����� ������ ����� ���������
    public List<SceneNeighbors> sceneGraph;

    // ��� ������� �������� �����
    private string currentScene;

    // ������ ����������� ����
    private HashSet<string> loadedScenes = new HashSet<string>();

    void Start()
    {
        // ��������, �������� � "1"
        currentScene = "1";
        loadedScenes.Add(currentScene);
        // ��������� ������� ��� ��������� �����
        LoadNeighbors(currentScene);
    }

    // �����, ������� ���������� ��� �������� ������ � ����� �����
    public void OnPlayerSceneChange(string newScene)
    {
        if (newScene == currentScene)
            return;

        // ��������� �����, ������� �� ����� ��� ����� �������
        UnloadFarScenes(newScene);

        // ��������� ������� �����
        currentScene = newScene;

        // ��������� �������� ����� ��� ����� ������� �����
        LoadNeighbors(newScene);
    }

    // ��������� ������� ��������� �����
    void LoadNeighbors(string scene)
    {
        SceneNeighbors sn = sceneGraph.Find(s => s.sceneName == scene);
        if (sn != null)
        {
            foreach (var neighbor in sn.neighbors)
            {
                if (!loadedScenes.Contains(neighbor))
                {
                    StartCoroutine(LoadSceneAsync(neighbor));
                    loadedScenes.Add(neighbor);
                }
            }
        }
    }

    // ��������� �����, ������� �� �������� ������������ ��� ����� ������� ������
    void UnloadFarScenes(string newScene)
    {
        // ���������� ������ ����������� ����: ���� ����� ����� � � ������
        SceneNeighbors sn = sceneGraph.Find(s => s.sceneName == newScene);
        HashSet<string> requiredScenes = new HashSet<string>();
        requiredScenes.Add(newScene);
        if (sn != null)
        {
            foreach (var neighbor in sn.neighbors)
            {
                requiredScenes.Add(neighbor);
            }
        }

        // �������� ����� ��� �������� � ��, ��� ���������, �� �� ������ � requiredScenes
        List<string> scenesToUnload = new List<string>();
        foreach (var scene in loadedScenes)
        {
            if (!requiredScenes.Contains(scene))
            {
                scenesToUnload.Add(scene);
            }
        }
        foreach (var scene in scenesToUnload)
        {
            StartCoroutine(UnloadSceneAsync(scene));
            loadedScenes.Remove(scene);
        }
    }

    // ����������� �������� �����
    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncOp.isDone)
        {
            // ����� �������� ���������� UI ���������� ��������: asyncOp.progress
            yield return null;
        }
    }

    // ����������� �������� �����
    IEnumerator UnloadSceneAsync(string sceneName)
    {
        AsyncOperation asyncOp = SceneManager.UnloadSceneAsync(sceneName);
        while (!asyncOp.isDone)
        {
            yield return null;
        }
    }
}
