using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    public enum FPSPreset
    {
        Sixty = 60,
        SeventyFive = 75,
        Hundred = 100
    }

    [SerializeField] private FPSPreset _targetFPS = FPSPreset.Sixty;
    [SerializeField] private bool _withoutLimites = true;

    private void Start()
    {
        ApplySettings();
    }

    private void ApplySettings()
    {
        // Установка оконного режима
        Screen.fullScreenMode = FullScreenMode.Windowed;

        // Настройка VSync и FPS
        if (_withoutLimites)
        {
            QualitySettings.vSyncCount = 1;
            // При включенном VSync targetFrameRate игнорируется
            Application.targetFrameRate = -1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = (int)_targetFPS;
        }
    }

    // Для переключения в редакторе (опционально)
    private void OnValidate()
    {
        ApplySettings();
    }
}