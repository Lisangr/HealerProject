using UnityEngine;

public class BushToggle : MonoBehaviour
{
    // Вызывается при изменении состояния через GlobalToggleManager
    public void SetActiveState(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    // При старте можно синхронизировать состояние с глобальным, если менеджер уже существует
    private void Start()
    {
        if (GlobalToggleManager.Instance != null)
        {
            gameObject.SetActive(GlobalToggleManager.Instance.bushesEnabled);
        }
    }
}
