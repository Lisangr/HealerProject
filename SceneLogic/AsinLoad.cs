using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AsinLoad : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private Text _sliderValueText;

    void Start()
    {
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        // 1. Загружаем сцену "1" аддитивно, если она ещё не загружена.
        Scene scene1 = SceneManager.GetSceneByName("1");
        if (!scene1.isLoaded)
        {
            AsyncOperation loadScene1 = SceneManager.LoadSceneAsync("1", LoadSceneMode.Additive);
            while (!loadScene1.isDone)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        // 2. Загружаем сцену "PlayerAndCanvas" аддитивно, если она ещё не загружена.
        Scene canvasScene = SceneManager.GetSceneByName("PlayerAndCanvas");
        if (!canvasScene.isLoaded)
        {
            AsyncOperation loadPlayerAndCanvas = SceneManager.LoadSceneAsync("PlayerAndCanvas", LoadSceneMode.Additive);
            while (!loadPlayerAndCanvas.isDone)
            {
                // Обновляем слайдер загрузки (для примера)
                float progress = Mathf.Clamp01(loadPlayerAndCanvas.progress / 0.9f);
                if (_slider != null)
                    _slider.value = progress;
                if (_sliderValueText != null)
                    _sliderValueText.text = (progress * 100).ToString("F0") + "%";
                yield return null;
            }
        }

        // 3. Устанавливаем сцену "PlayerAndCanvas" активной,
        // чтобы важные объекты (игрок, UI) остались при дальнейшей выгрузке других сцен.
        Scene playerAndCanvasScene = SceneManager.GetSceneByName("PlayerAndCanvas");
        if (playerAndCanvasScene.IsValid())
        {
            SceneManager.SetActiveScene(playerAndCanvasScene);
        }
        else
        {
            Debug.LogError("Сцена 'PlayerAndCanvas' не загружена!");
        }

        // 4. Считываем название следующей сцены из PlayerPrefs.
        // Если ключ отсутствует, по умолчанию будет "1".
        string nextScene = PlayerPrefs.GetString("nextSceneName", "1");

        // Если название сцены не пустое и не равно "1" (т.е. меняем сцену),
        // то загружаем новую сцену аддитивно и выгружаем сцену "1".
        if (!string.IsNullOrEmpty(nextScene) && nextScene != "1")
        {
            // Если сцена с таким именем ещё не загружена, загружаем её.
            Scene targetScene = SceneManager.GetSceneByName(nextScene);
            if (!targetScene.isLoaded)
            {
                AsyncOperation loadNext = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
                while (!loadNext.isDone)
                {
                    // При необходимости можно обновлять прогресс загрузки этой сцены.
                    yield return null;
                }
            }

            // Выгружаем сцену "1", так как она больше не нужна.
            AsyncOperation unloadScene1 = SceneManager.UnloadSceneAsync("1");
            while (!unloadScene1.isDone)
            {
                yield return null;
            }
        }
        else
        {
            // Если nextScene равно "1", значит, остаётся загруженной сцена "1".
            Debug.Log("Сцена для загрузки установлена как '1'. Дополнительная загрузка не выполняется.");
        }

        // 5. Выгружаем загрузочную сцену "SceneForAsincLoading", если она загружена.
        Scene loadingScene = SceneManager.GetSceneByName("SceneForAsincLoading");
        if (loadingScene.isLoaded)
        {
            AsyncOperation unloadLoader = SceneManager.UnloadSceneAsync("SceneForAsincLoading");
            while (!unloadLoader.isDone)
            {
                yield return null;
            }
        }
    }
}
