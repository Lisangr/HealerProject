using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveSceneLoader : MonoBehaviour
{
    [SerializeField] private string additiveSceneName = "PlayerAndCanvas"; // Сцена для аддитивной загрузки
    [SerializeField] private string mainSceneName = "1"; // Имя основной сцены, после загрузки которой запускается аддитивная загрузка

    private bool additiveLoaded = false;

    void Awake()
    {
        // Чтобы этот объект не уничтожался при смене сцены (если это необходимо)
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

    // Вызывается после загрузки любой сцены
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Запускаем корутину только если загружена нужная основная сцена и аддитивная ещё не загружена
        if (scene.name == mainSceneName && !additiveLoaded)
        {
            StartCoroutine(LoadAdditiveScene());
        }
    }

    IEnumerator LoadAdditiveScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(additiveSceneName, LoadSceneMode.Additive);
        // Ждем, пока загрузка полностью завершится
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        additiveLoaded = true;
        Debug.Log($"Сцена {additiveSceneName} успешно загружена аддитивно.");
    }
}
