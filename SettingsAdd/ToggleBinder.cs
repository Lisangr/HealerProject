using UnityEngine;
using Kamgam.UGUIComponentsForSettings;

public enum ToggleType
{
    InvertCameraX,
    InvertCameraY
}

public class ToggleBinder : MonoBehaviour
{
    public ToggleType toggleType; // ������� ��� ������������� � ����������

    private ToggleUGUI _toggleUGUI;

    private void Awake()
    {
        _toggleUGUI = GetComponent<ToggleUGUI>();
        if (_toggleUGUI == null)
        {
            Debug.LogError("ToggleUGUI component not found!", this);
            return;
        }

        _toggleUGUI.OnValueChanged += OnToggleChanged;
    }

    private void Start()
    {
        // ������������� �������� ������������� �� ��������
        switch (toggleType)
        {
            case ToggleType.InvertCameraX:
                _toggleUGUI.Value = InputSettings.Instance.InvertCameraX;
                break;
            case ToggleType.InvertCameraY:
                _toggleUGUI.Value = InputSettings.Instance.InvertCameraY;
                break;
        }
    }

    private void OnToggleChanged(bool value)
    {
        // ���������� �������� � ����������� �� ���� �������������
        switch (toggleType)
        {
            case ToggleType.InvertCameraX:
                InputSettings.Instance.InvertCameraX = value;
                break;
            case ToggleType.InvertCameraY:
                InputSettings.Instance.InvertCameraY = value;
                break;
        }

        // ���������� ���������
        InputSettings.Instance.SaveSettings();
    }

    private void OnDestroy()
    {
        if (_toggleUGUI != null)
        {
            _toggleUGUI.OnValueChanged -= OnToggleChanged;
        }
    }
}
