using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveSceneLoader : MonoBehaviour
{
    [SerializeField] private string additiveSceneName = "PlayerAndCanvas"; // ����� ��� ���������� ��������
    [SerializeField] private string mainSceneName = "1"; // ��� �������� �����, ����� �������� ������� ����������� ���������� ��������

    private bool additiveLoaded = false;

    void Awake()
    {
        // ����� ���� ������ �� ����������� ��� ����� ����� (���� ��� ����������)
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ���������� ����� �������� ����� �����
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ��������� �������� ������ ���� ��������� ������ �������� ����� � ���������� ��� �� ���������
        if (scene.name == mainSceneName && !additiveLoaded)
        {
            StartCoroutine(LoadAdditiveScene());
        }
    }

    IEnumerator LoadAdditiveScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(additiveSceneName, LoadSceneMode.Additive);
        // ����, ���� �������� ��������� ����������
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        additiveLoaded = true;
        Debug.Log($"����� {additiveSceneName} ������� ��������� ���������.");
    }
}
