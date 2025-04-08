using UnityEngine;
using System.Collections.Generic;
using Kamgam.UGUIComponentsForSettings;

public static class InputUtils
{
    private static Dictionary<UniversalKeyCode, KeyCode> _keyCodeMap;

    static InputUtils()
    {
        InitializeKeyCodeMap();
    }

    private static void InitializeKeyCodeMap()
    {
        _keyCodeMap = new Dictionary<UniversalKeyCode, KeyCode>
        {
            // Буквы
            { UniversalKeyCode.A, KeyCode.A },
            { UniversalKeyCode.B, KeyCode.B },
            { UniversalKeyCode.C, KeyCode.C },
            { UniversalKeyCode.D, KeyCode.D },
            { UniversalKeyCode.E, KeyCode.E },
            { UniversalKeyCode.F, KeyCode.F },
            { UniversalKeyCode.G, KeyCode.G },
            { UniversalKeyCode.H, KeyCode.H },
            { UniversalKeyCode.I, KeyCode.I },
            { UniversalKeyCode.J, KeyCode.J },
            { UniversalKeyCode.K, KeyCode.K },
            { UniversalKeyCode.L, KeyCode.L },
            { UniversalKeyCode.M, KeyCode.M },
            { UniversalKeyCode.N, KeyCode.N },
            { UniversalKeyCode.O, KeyCode.O },
            { UniversalKeyCode.P, KeyCode.P },
            { UniversalKeyCode.Q, KeyCode.Q },
            { UniversalKeyCode.R, KeyCode.R },
            { UniversalKeyCode.S, KeyCode.S },
            { UniversalKeyCode.T, KeyCode.T },
            { UniversalKeyCode.U, KeyCode.U },
            { UniversalKeyCode.V, KeyCode.V },
            { UniversalKeyCode.W, KeyCode.W },
            { UniversalKeyCode.X, KeyCode.X },
            { UniversalKeyCode.Y, KeyCode.Y },
            { UniversalKeyCode.Z, KeyCode.Z },

            // Цифры
            { UniversalKeyCode.Digit0, KeyCode.Alpha0 },
            { UniversalKeyCode.Digit1, KeyCode.Alpha1 },
            { UniversalKeyCode.Digit2, KeyCode.Alpha2 },
            { UniversalKeyCode.Digit3, KeyCode.Alpha3 },
            { UniversalKeyCode.Digit4, KeyCode.Alpha4 },
            { UniversalKeyCode.Digit5, KeyCode.Alpha5 },
            { UniversalKeyCode.Digit6, KeyCode.Alpha6 },
            { UniversalKeyCode.Digit7, KeyCode.Alpha7 },
            { UniversalKeyCode.Digit8, KeyCode.Alpha8 },
            { UniversalKeyCode.Digit9, KeyCode.Alpha9 },

            // Специальные клавиши
            { UniversalKeyCode.Space, KeyCode.Space },
            { UniversalKeyCode.LeftShift, KeyCode.LeftShift },
            { UniversalKeyCode.RightShift, KeyCode.RightShift },
            { UniversalKeyCode.LeftControl, KeyCode.LeftControl },
            { UniversalKeyCode.RightControl, KeyCode.RightControl },
            { UniversalKeyCode.LeftAlt, KeyCode.LeftAlt },
            { UniversalKeyCode.RightAlt, KeyCode.RightAlt },
            { UniversalKeyCode.Escape, KeyCode.Escape },
            { UniversalKeyCode.Return, KeyCode.Return },
            { UniversalKeyCode.Backspace, KeyCode.Backspace },
            { UniversalKeyCode.Tab, KeyCode.Tab },

            // F1-F12
            { UniversalKeyCode.F1, KeyCode.F1 },
            { UniversalKeyCode.F2, KeyCode.F2 },
            { UniversalKeyCode.F3, KeyCode.F3 },
            { UniversalKeyCode.F4, KeyCode.F4 },
            { UniversalKeyCode.F5, KeyCode.F5 },
            { UniversalKeyCode.F6, KeyCode.F6 },
            { UniversalKeyCode.F7, KeyCode.F7 },
            { UniversalKeyCode.F8, KeyCode.F8 },
            { UniversalKeyCode.F9, KeyCode.F9 },
            { UniversalKeyCode.F10, KeyCode.F10 },
            { UniversalKeyCode.F11, KeyCode.F11 },
            { UniversalKeyCode.F12, KeyCode.F12 },
        };
    }
    /// <summary>
    /// Проверяет удерживание клавиши
    /// </summary>
    public static bool GetKey(UniversalKeyCode key)
    {
        var keyCode = ConvertUniversalKeyCode(key);
        return Input.GetKey(keyCode);
    }
    /// <summary>
    /// Проверяет момент нажатия клавиши
    /// </summary>
    public static bool GetKeyDown(UniversalKeyCode key)
    {
        return Input.GetKeyDown(ConvertUniversalKeyCode(key));
    }
    /// <summary>
    /// Проверяет момент отпускания клавиши
    /// </summary>
    private static KeyCode ConvertUniversalKeyCode(UniversalKeyCode universalKey)
    {
        if (_keyCodeMap.TryGetValue(universalKey, out KeyCode keyCode))
        {
            return keyCode;
        }
        return KeyCode.None;
    }
    /// <summary>
    /// Сбрасывает состояния "залипших" клавиш (для новой Input System)
    /// </summary>
    public static void ResetStuckKeyStates()
    {
#if ENABLE_INPUT_SYSTEM
        var inputSystem = UnityEngine.InputSystem.InputSystem.instance;
        if (inputSystem != null)
        {
            foreach (var device in inputSystem.devices)
            {
                if (device != null) device.RequestReset();
            }
        }
#endif
    }

    /// <summary>
    /// Проверяет нажатие любой клавиши или кнопки мыши
    /// </summary>
    public static bool AnyKey()
    {
        return Input.anyKey;
    }

    /// <summary>
    /// Проверяет нажатие отмены (обычно Escape или кнопка B на геймпаде)
    /// </summary>
    public static bool CancelDown()
    {
        return Input.GetKeyDown(KeyCode.Escape) ||
               Input.GetKeyDown(KeyCode.JoystickButton1);
    }
}