using UnityEngine;

public class RocksToggle : MonoBehaviour
{
    public void SetActiveState(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    private void Start()
    {
        if (GlobalToggleManager.Instance != null)
        {
            gameObject.SetActive(GlobalToggleManager.Instance.rocksEnabled);
        }
    }
}
