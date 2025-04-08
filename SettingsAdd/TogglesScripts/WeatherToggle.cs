using UnityEngine;

public class WeatherToggle : MonoBehaviour
{
    [Header("Weather Systems")]
    [Tooltip("Reference to the FogController script that manages fog effects.")]
    private FogController fogController;

    [Tooltip("Reference to the RainSpawner script that manages rain activation and countdown.")]
    private RainSpawner rainSpawner;

    /// <summary>
    /// When weather is active, enable fog and rain components and disable the sun object.
    /// When weather is off, disable fog and rain components and enable the sun object.
    /// </summary>
    /// <param name="isActive">If true, weather systems are enabled; if false, they are disabled.</param>
    public void SetActiveState(bool isActive)
    {
        // Не деактивируем сам объект, а только компоненты
        if (fogController != null)
        {
            fogController.enabled = isActive;
            RenderSettings.fog = isActive;
        }
        else
        {
            RenderSettings.fog = isActive;
        }

        if (rainSpawner != null)
        {
            rainSpawner.enabled = isActive;
        }
    }

    private void Start()
    {
        // Синхронизируем состояние погоды с глобальным переключателем
        if (GlobalToggleManager.Instance != null)
        {
            SetActiveState(GlobalToggleManager.Instance.weatherEnable);
        }
    }

    private void OnEnable()
    {
        fogController = GetComponent<FogController>();
        rainSpawner = GetComponent<RainSpawner>();
    }
}
