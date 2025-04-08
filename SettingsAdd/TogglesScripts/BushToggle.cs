using UnityEngine;

public class BushToggle : MonoBehaviour
{
    // ���������� ��� ��������� ��������� ����� GlobalToggleManager
    public void SetActiveState(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    // ��� ������ ����� ���������������� ��������� � ����������, ���� �������� ��� ����������
    private void Start()
    {
        if (GlobalToggleManager.Instance != null)
        {
            gameObject.SetActive(GlobalToggleManager.Instance.bushesEnabled);
        }
    }
}
