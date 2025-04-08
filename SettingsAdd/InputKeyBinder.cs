using UnityEngine;
using Kamgam.UGUIComponentsForSettings;

public class InputKeyBinder : MonoBehaviour
{
    [Header("Settings")]
    public InputActionType actionType; // Тип действия для привязки

    private InputKeyUGUI _inputKeyComponent;

    void Awake()
    {
        // Получаем компонент InputKeyUGUI на этом объекте
        _inputKeyComponent = GetComponent<InputKeyUGUI>();

        // Проверяем, что компонент найден
        if (_inputKeyComponent == null)
        {
            Debug.LogError("InputKeyUGUI component not found!", this);
            return;
        }

        // Подписываемся на событие изменения клавиши
        _inputKeyComponent.OnChanged += HandleKeyChanged;
    }

    void Start()
    {
        // Инициализируем начальные значения из сохранённых настроек
        InitializeFromSavedSettings();
    }

    private void InitializeFromSavedSettings()
    {
        InputSettings.Instance.LoadSettings();
        Debug.Log($"Initializing {actionType} from saved settings");

        // Устанавливаем начальное значение для UI-компонента в зависимости от типа действия
        switch (actionType)
        {
            case InputActionType.MoveForward:
                _inputKeyComponent.Key = InputSettings.Instance.MoveForwardKey;
                break;
            case InputActionType.MoveBackward:
                _inputKeyComponent.Key = InputSettings.Instance.MoveBackwardKey;
                break;
            case InputActionType.MoveLeft:
                _inputKeyComponent.Key = InputSettings.Instance.MoveLeftKey;
                break;
            case InputActionType.MoveRight:
                _inputKeyComponent.Key = InputSettings.Instance.MoveRightKey;
                break;
            case InputActionType.Run:
                _inputKeyComponent.Key = InputSettings.Instance.RunModifierKey;
                break;
            case InputActionType.Jump:
                _inputKeyComponent.Key = InputSettings.Instance.JumpKey;
                break;
            case InputActionType.Settings:
                _inputKeyComponent.Key = InputSettings.Instance.SettingsKey;
                break;
            case InputActionType.Map:
                _inputKeyComponent.Key = InputSettings.Instance.MapKey;
                break;
            case InputActionType.Tasks:
                _inputKeyComponent.Key = InputSettings.Instance.TasksKey;
                break;
            case InputActionType.Menu:
                _inputKeyComponent.Key = InputSettings.Instance.MenuKey;
                break;
            case InputActionType.Shoot:
                _inputKeyComponent.Key = InputSettings.Instance.ShootKey;
                break;
            case InputActionType.Take:
                _inputKeyComponent.Key = InputSettings.Instance.TakeKey;
                break;
            case InputActionType.Action:
                _inputKeyComponent.Key = InputSettings.Instance.ActionKey;
                break;
            case InputActionType.Help:
                _inputKeyComponent.Key = InputSettings.Instance.HelpKey;
                break;
            case InputActionType.Character:
                _inputKeyComponent.Key = InputSettings.Instance.CharacterKey;
                break;
            case InputActionType.Inventory:
                _inputKeyComponent.Key = InputSettings.Instance.InventoryKey;
                break;
            case InputActionType.TakeMonster:
                _inputKeyComponent.Key = InputSettings.Instance.TakeMonsterKey;
                break;
        }

        _inputKeyComponent.Refresh();
    }

    private void HandleKeyChanged(UniversalKeyCode key, UniversalKeyCode modifier)
    {
        // Обновляем соответствующие настройки в зависимости от типа действия
        switch (actionType)
        {
            case InputActionType.MoveForward:
                InputSettings.Instance.MoveForwardKey = key;
                break;
            case InputActionType.MoveBackward:
                InputSettings.Instance.MoveBackwardKey = key;
                break;
            case InputActionType.MoveLeft:
                InputSettings.Instance.MoveLeftKey = key;
                break;
            case InputActionType.MoveRight:
                InputSettings.Instance.MoveRightKey = key;
                break;
            case InputActionType.Run:
                InputSettings.Instance.RunModifierKey = key;
                break;
            case InputActionType.Jump:
                InputSettings.Instance.JumpKey = key;
                break;
            case InputActionType.Settings:
                InputSettings.Instance.SettingsKey = key;
                break;
            case InputActionType.Map:
                InputSettings.Instance.MapKey = key;
                break;
            case InputActionType.Tasks:
                InputSettings.Instance.TasksKey = key;
                break;
            case InputActionType.Menu:
                InputSettings.Instance.MenuKey = key;
                break;
            case InputActionType.Shoot:
                InputSettings.Instance.ShootKey = key;
                break;
            case InputActionType.Take:
                InputSettings.Instance.TakeKey = key;
                break;
            case InputActionType.Action:
                InputSettings.Instance.ActionKey = key;
                break;
            case InputActionType.Help:
                InputSettings.Instance.HelpKey = key;
                break;
            case InputActionType.Character:
                InputSettings.Instance.CharacterKey = key;
                break;
            case InputActionType.Inventory:
                InputSettings.Instance.InventoryKey = key;
                break;
            case InputActionType.TakeMonster:
                InputSettings.Instance.TakeMonsterKey = key;
                break;
        }

        // Сохраняем изменения
        InputSettings.Instance.SaveSettings();
    }

    void OnDestroy()
    {
        // Отписываемся от события при уничтожении объекта
        if (_inputKeyComponent != null)
        {
            _inputKeyComponent.OnChanged -= HandleKeyChanged;
        }
    }
}

// Перечисление для идентификации действий (расширено для новых панелей и действий)
public enum InputActionType
{
    MoveForward,
    MoveBackward,
    MoveLeft,
    MoveRight,
    Run,
    Jump,
    Settings,
    Map,
    Tasks,
    Menu,
    Shoot,
    Take,
    Action,
    Help,
    Character,
    Inventory,
    TakeMonster
}
