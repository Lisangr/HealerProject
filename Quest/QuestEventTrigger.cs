using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class QuestEventTrigger : MonoBehaviour
{
    [System.Serializable]
    public class QuestEvent
    {
        public string questID;
        public GameObject[] objectsToActivate;
        public GameObject[] objectsToDeactivate;
        public float activationDelay = 0f;
        public bool wasTriggered = false;

        // Новые поля для управления сценами
        public bool loadNewScene = false;
        public string sceneToLoad = "";
        public bool loadAdditively = false;
        public float sceneLoadDelay = 0f;
        public bool unloadCurrentScene = false;
    }

    [SerializeField] private List<QuestEvent> questEvents = new List<QuestEvent>();
    [SerializeField] private List<QuestEvent> killQuestEvents = new List<QuestEvent>();
    [SerializeField] private bool checkCompletedQuestsOnStart = true;
    [SerializeField] private GameObject loadingScreenPrefab = null;

    private void Start()
    {
        SubscribeToQuestEvents();
        if (checkCompletedQuestsOnStart)
            StartCoroutine(CheckCompletedQuestsNextFrame());
    }

    private void OnEnable() => SubscribeToQuestEvents();
    private void OnDisable() => UnsubscribeFromQuestEvents();
    private void OnDestroy() => UnsubscribeFromQuestEvents();

    private void SubscribeToQuestEvents()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
        else
            StartCoroutine(TrySubscribeToQuestManager());

        if (QuestHunterManager.Instance != null)
            QuestHunterManager.Instance.OnKillQuestCompleted += HandleKillQuestCompleted;
        else
            StartCoroutine(TrySubscribeToQuestHunterManager());
    }

    private void UnsubscribeFromQuestEvents()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        if (QuestHunterManager.Instance != null)
            QuestHunterManager.Instance.OnKillQuestCompleted -= HandleKillQuestCompleted;
    }

    private IEnumerator TrySubscribeToQuestManager()
    {
        int attempts = 0;
        while (QuestManager.Instance == null && attempts++ < 10)
            yield return new WaitForSeconds(0.5f);

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
            if (checkCompletedQuestsOnStart) CheckCompletedQuests();
        }
    }

    private IEnumerator TrySubscribeToQuestHunterManager()
    {
        int attempts = 0;
        while (QuestHunterManager.Instance == null && attempts++ < 10)
            yield return new WaitForSeconds(0.5f);

        if (QuestHunterManager.Instance != null)
        {
            QuestHunterManager.Instance.OnKillQuestCompleted += HandleKillQuestCompleted;
            if (checkCompletedQuestsOnStart) CheckCompletedKillQuests();
        }
    }

    private IEnumerator CheckCompletedQuestsNextFrame()
    {
        yield return null;
        CheckCompletedQuests();
        CheckCompletedKillQuests();
    }

    private void CheckCompletedQuests()
    {
        if (QuestManager.Instance == null || QuestManager.Instance.completedQuests == null) return;
        foreach (var questEvent in questEvents)
        {
            if (!questEvent.wasTriggered && QuestManager.Instance.completedQuests.Exists(q => q.questID == questEvent.questID))
                TriggerEvent(questEvent);
        }
    }

    private void CheckCompletedKillQuests()
    {
        if (QuestHunterManager.Instance == null || QuestHunterManager.Instance.completedKillQuests == null) return;
        foreach (var questEvent in killQuestEvents)
        {
            if (!questEvent.wasTriggered && QuestHunterManager.Instance.completedKillQuests.Exists(q => q.questID == questEvent.questID))
                TriggerEvent(questEvent);
        }
    }

    private void HandleQuestCompleted(Quest quest)
    {
        QuestEvent questEvent = questEvents.Find(e => e.questID == quest.questID && !e.wasTriggered);
        if (questEvent != null) TriggerEvent(questEvent);
    }

    private void HandleKillQuestCompleted(KillQuestData quest)
    {
        QuestEvent questEvent = killQuestEvents.Find(e => e.questID == quest.questID && !e.wasTriggered);
        if (questEvent != null) TriggerEvent(questEvent);
    }

    private void TriggerEvent(QuestEvent questEvent)
    {
        if (questEvent.wasTriggered) return;
        questEvent.wasTriggered = true;

        if (questEvent.activationDelay > 0)
            StartCoroutine(TriggerEventWithDelay(questEvent));
        else
            ActivateDeactivateObjects(questEvent);
    }

    private IEnumerator TriggerEventWithDelay(QuestEvent questEvent)
    {
        yield return new WaitForSeconds(questEvent.activationDelay);
        ActivateDeactivateObjects(questEvent);
    }

    private void ActivateDeactivateObjects(QuestEvent questEvent)
    {
        foreach (var obj in questEvent.objectsToActivate)
            if (obj != null) obj.SetActive(true);

        foreach (var obj in questEvent.objectsToDeactivate)
            if (obj != null) obj.SetActive(false);

        if (questEvent.loadNewScene && !string.IsNullOrEmpty(questEvent.sceneToLoad))
        {
            if (questEvent.sceneLoadDelay > 0)
                StartCoroutine(LoadSceneWithDelay(questEvent));
            else
                LoadScene(questEvent);
        }
    }

    private IEnumerator LoadSceneWithDelay(QuestEvent questEvent)
    {
        yield return new WaitForSeconds(questEvent.sceneLoadDelay);
        LoadScene(questEvent);
    }

    private void LoadScene(QuestEvent questEvent)
    {
        // Вместо прямой загрузки указанной сцены сохраняем её название в PlayerPrefs
        // и запускаем асинхронную загрузку сцены‑загрузчика "SceneForAsincLoading".
        if (!SceneExists("SceneForAsincLoading"))
        {
            Debug.LogError("Сцена 'SceneForAsincLoading' не найдена в билде.");
            return;
        }

        // Сохраняем название следующей сцены в PlayerPrefs
        PlayerPrefs.SetString("nextSceneName", questEvent.sceneToLoad);

        GameObject loadingScreen = loadingScreenPrefab != null ? Instantiate(loadingScreenPrefab) : null;
        if (loadingScreen != null)
            DontDestroyOnLoad(loadingScreen);

        StartCoroutine(LoadAsyncLoaderScene(questEvent, loadingScreen));
    }

    private IEnumerator LoadAsyncLoaderScene(QuestEvent questEvent, GameObject loadingScreen)
    {
        // Загружаем сцену‑загрузчик асинхронно
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("SceneForAsincLoading", LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        if (questEvent.unloadCurrentScene)
        {
            // Выгружаем сцену, в которой находится этот объект (например, сцену "1")
            Scene sceneToUnload = gameObject.scene;
            yield return SceneManager.UnloadSceneAsync(sceneToUnload);
        }

        if (loadingScreen != null)
            Destroy(loadingScreen);
    }

    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (System.IO.Path.GetFileNameWithoutExtension(path) == sceneName)
                return true;
        }
        return false;
    }
}
