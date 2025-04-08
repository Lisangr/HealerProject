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
        // 1. ��������� ����� "1" ���������, ���� ��� ��� �� ���������.
        Scene scene1 = SceneManager.GetSceneByName("1");
        if (!scene1.isLoaded)
        {
            AsyncOperation loadScene1 = SceneManager.LoadSceneAsync("1", LoadSceneMode.Additive);
            while (!loadScene1.isDone)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        // 2. ��������� ����� "PlayerAndCanvas" ���������, ���� ��� ��� �� ���������.
        Scene canvasScene = SceneManager.GetSceneByName("PlayerAndCanvas");
        if (!canvasScene.isLoaded)
        {
            AsyncOperation loadPlayerAndCanvas = SceneManager.LoadSceneAsync("PlayerAndCanvas", LoadSceneMode.Additive);
            while (!loadPlayerAndCanvas.isDone)
            {
                // ��������� ������� �������� (��� �������)
                float progress = Mathf.Clamp01(loadPlayerAndCanvas.progress / 0.9f);
                if (_slider != null)
                    _slider.value = progress;
                if (_sliderValueText != null)
                    _sliderValueText.text = (progress * 100).ToString("F0") + "%";
                yield return null;
            }
        }

        // 3. ������������� ����� "PlayerAndCanvas" ��������,
        // ����� ������ ������� (�����, UI) �������� ��� ���������� �������� ������ ����.
        Scene playerAndCanvasScene = SceneManager.GetSceneByName("PlayerAndCanvas");
        if (playerAndCanvasScene.IsValid())
        {
            SceneManager.SetActiveScene(playerAndCanvasScene);
        }
        else
        {
            Debug.LogError("����� 'PlayerAndCanvas' �� ���������!");
        }

        // 4. ��������� �������� ��������� ����� �� PlayerPrefs.
        // ���� ���� �����������, �� ��������� ����� "1".
        string nextScene = PlayerPrefs.GetString("nextSceneName", "1");

        // ���� �������� ����� �� ������ � �� ����� "1" (�.�. ������ �����),
        // �� ��������� ����� ����� ��������� � ��������� ����� "1".
        if (!string.IsNullOrEmpty(nextScene) && nextScene != "1")
        {
            // ���� ����� � ����� ������ ��� �� ���������, ��������� �.
            Scene targetScene = SceneManager.GetSceneByName(nextScene);
            if (!targetScene.isLoaded)
            {
                AsyncOperation loadNext = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
                while (!loadNext.isDone)
                {
                    // ��� ������������� ����� ��������� �������� �������� ���� �����.
                    yield return null;
                }
            }

            // ��������� ����� "1", ��� ��� ��� ������ �� �����.
            AsyncOperation unloadScene1 = SceneManager.UnloadSceneAsync("1");
            while (!unloadScene1.isDone)
            {
                yield return null;
            }
        }
        else
        {
            // ���� nextScene ����� "1", ������, ������� ����������� ����� "1".
            Debug.Log("����� ��� �������� ����������� ��� '1'. �������������� �������� �� �����������.");
        }

        // 5. ��������� ����������� ����� "SceneForAsincLoading", ���� ��� ���������.
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
