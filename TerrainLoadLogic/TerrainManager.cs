using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class TerrainManager : MonoBehaviour
{
    // Реализация синглтона
    public static TerrainManager Instance { get; private set; }

    void Awake()
    {
        // Если экземпляр уже существует, уничтожаем дубликат
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Если нужно сохранить менеджер между сценами:
        DontDestroyOnLoad(gameObject);
    }

    // Класс для описания сцены и её соседей
    [System.Serializable]
    public class SceneNeighbors
    {
        public string sceneName;             // Имя сцены
        public List<string> neighbors;       // Имена соседних сцен (например, на севере, востоке и т.д.)
    }

    // Граф сцен, который можно задать через инспектор
    public List<SceneNeighbors> sceneGraph;

    // Имя текущей активной сцены
    private string currentScene;

    // Список загруженных сцен
    private HashSet<string> loadedScenes = new HashSet<string>();

    void Start()
    {
        // Например, начинаем с "1"
        currentScene = "1";
        loadedScenes.Add(currentScene);
        // Загружаем соседей для стартовой сцены
        LoadNeighbors(currentScene);
    }

    // Метод, который вызывается при переходе игрока в новую сцену
    public void OnPlayerSceneChange(string newScene)
    {
        if (newScene == currentScene)
            return;

        // Выгружаем сцены, которые не нужны для новой позиции
        UnloadFarScenes(newScene);

        // Обновляем текущую сцену
        currentScene = newScene;

        // Загружаем соседние сцены для новой текущей сцены
        LoadNeighbors(newScene);
    }

    // Загружает соседей указанной сцены
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

    // Выгружает сцены, которые не являются необходимыми для новой позиции игрока
    void UnloadFarScenes(string newScene)
    {
        // Составляем список необходимых сцен: сама новая сцена и её соседи
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

        // Выбираем сцены для выгрузки — те, что загружены, но не входят в requiredScenes
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

    // Асинхронная загрузка сцены
    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncOp.isDone)
        {
            // Можно добавить обновление UI индикатора загрузки: asyncOp.progress
            yield return null;
        }
    }

    // Асинхронная выгрузка сцены
    IEnumerator UnloadSceneAsync(string sceneName)
    {
        AsyncOperation asyncOp = SceneManager.UnloadSceneAsync(sceneName);
        while (!asyncOp.isDone)
        {
            yield return null;
        }
    }
}
