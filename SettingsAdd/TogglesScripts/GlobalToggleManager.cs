using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalToggleManager : MonoBehaviour
{
    public static GlobalToggleManager Instance { get; private set; }

    // ���������� ��������� ��� ��������
    public bool bushesEnabled = true;
    public bool grassEnabled = true;
    public bool rocksEnabled = true;
    public bool weatherEnable = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ��� �������� ����� ����� ��������� ��������� ��������
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (BushToggle bush in FindObjectsOfType<BushToggle>())
        {
            bush.SetActiveState(bushesEnabled);
        }

        foreach (GrassToggle grass in FindObjectsOfType<GrassToggle>())
        {
            grass.SetActiveState(grassEnabled);
        }

        foreach (RocksToggle rock in FindObjectsOfType<RocksToggle>())
        {
            rock.SetActiveState(rocksEnabled);
        }

        // ����� ���������� ���������� ���� weatherEnable
        foreach (WeatherToggle weather in FindObjectsOfType<WeatherToggle>())
        {
            weather.SetActiveState(weatherEnable);
        }
    }
}
