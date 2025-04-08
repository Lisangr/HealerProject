using Kamgam.UGUIComponentsForSettings;
using UnityEngine;

public class InputSettings : MonoBehaviour
{
    public static InputSettings Instance;

    public UniversalKeyCode MoveForwardKey = UniversalKeyCode.W;
    public UniversalKeyCode MoveBackwardKey = UniversalKeyCode.S;
    public UniversalKeyCode MoveLeftKey = UniversalKeyCode.A;
    public UniversalKeyCode MoveRightKey = UniversalKeyCode.D;
    public UniversalKeyCode RunModifierKey = UniversalKeyCode.LeftShift;
    public UniversalKeyCode JumpKey = UniversalKeyCode.Space;
    public UniversalKeyCode SettingsKey = UniversalKeyCode.O;
    public UniversalKeyCode MapKey = UniversalKeyCode.M;
    public UniversalKeyCode TasksKey = UniversalKeyCode.L;
    public UniversalKeyCode MenuKey = UniversalKeyCode.Tab;
    public UniversalKeyCode ShootKey = UniversalKeyCode.MouseLeft;
    public UniversalKeyCode TakeKey = UniversalKeyCode.E;
    public UniversalKeyCode ActionKey = UniversalKeyCode.F;
    public UniversalKeyCode HelpKey = UniversalKeyCode.H;
    public UniversalKeyCode CharacterKey = UniversalKeyCode.C;
    public UniversalKeyCode InventoryKey = UniversalKeyCode.I;
    public UniversalKeyCode TakeMonsterKey = UniversalKeyCode.T;

    public bool InvertCameraX = false;
    public bool InvertCameraY = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("MoveForward", (int)MoveForwardKey);
        PlayerPrefs.SetInt("MoveBackward", (int)MoveBackwardKey);
        PlayerPrefs.SetInt("MoveLeft", (int)MoveLeftKey);
        PlayerPrefs.SetInt("MoveRight", (int)MoveRightKey);
        PlayerPrefs.SetInt("RunModifier", (int)RunModifierKey);
        PlayerPrefs.SetInt("Jump", (int)JumpKey);
        PlayerPrefs.SetInt("SettingsKey", (int)SettingsKey);
        PlayerPrefs.SetInt("MapKey", (int)MapKey);
        PlayerPrefs.SetInt("TasksKey", (int)TasksKey);
        PlayerPrefs.SetInt("MenuKey", (int)MenuKey);
        PlayerPrefs.SetInt("ShootKey", (int)ShootKey);
        PlayerPrefs.SetInt("TakeKey", (int)TakeKey);
        PlayerPrefs.SetInt("ActionKey", (int)ActionKey);
        PlayerPrefs.SetInt("HelpKey", (int)HelpKey);
        PlayerPrefs.SetInt("CharacterKey", (int)CharacterKey);
        PlayerPrefs.SetInt("InventoryKey", (int)InventoryKey);
        PlayerPrefs.SetInt("TakeMonster", (int)TakeMonsterKey);

        PlayerPrefs.SetInt("InvertCameraX", InvertCameraX ? 1 : 0);
        PlayerPrefs.SetInt("InvertCameraY", InvertCameraY ? 1 : 0);

        PlayerPrefs.Save();
        Debug.Log("Settings saved: " + JsonUtility.ToJson(this));
    }

    public void LoadSettings()
    {
        MoveForwardKey = (UniversalKeyCode)PlayerPrefs.GetInt("MoveForward", (int)UniversalKeyCode.W);
        MoveBackwardKey = (UniversalKeyCode)PlayerPrefs.GetInt("MoveBackward", (int)UniversalKeyCode.S);
        MoveLeftKey = (UniversalKeyCode)PlayerPrefs.GetInt("MoveLeft", (int)UniversalKeyCode.A);
        MoveRightKey = (UniversalKeyCode)PlayerPrefs.GetInt("MoveRight", (int)UniversalKeyCode.D);
        RunModifierKey = (UniversalKeyCode)PlayerPrefs.GetInt("RunModifier", (int)UniversalKeyCode.LeftShift);
        JumpKey = (UniversalKeyCode)PlayerPrefs.GetInt("Jump", (int)UniversalKeyCode.Space);
        SettingsKey = (UniversalKeyCode)PlayerPrefs.GetInt("SettingsKey", (int)UniversalKeyCode.O);
        MapKey = (UniversalKeyCode)PlayerPrefs.GetInt("MapKey", (int)UniversalKeyCode.M);
        TasksKey = (UniversalKeyCode)PlayerPrefs.GetInt("TasksKey", (int)UniversalKeyCode.L);
        MenuKey = (UniversalKeyCode)PlayerPrefs.GetInt("MenuKey", (int)UniversalKeyCode.Tab);
        ShootKey = (UniversalKeyCode)PlayerPrefs.GetInt("ShootKey", (int)UniversalKeyCode.MouseLeft);
        TakeKey = (UniversalKeyCode)PlayerPrefs.GetInt("TakeKey", (int)UniversalKeyCode.E);
        ActionKey = (UniversalKeyCode)PlayerPrefs.GetInt("ActionKey", (int)UniversalKeyCode.F);
        HelpKey = (UniversalKeyCode)PlayerPrefs.GetInt("HelpKey", (int)UniversalKeyCode.H);
        CharacterKey = (UniversalKeyCode)PlayerPrefs.GetInt("CharacterKey", (int)UniversalKeyCode.C);
        InventoryKey = (UniversalKeyCode)PlayerPrefs.GetInt("InventoryKey", (int)UniversalKeyCode.I);
        TakeMonsterKey = (UniversalKeyCode)PlayerPrefs.GetInt("TakeMonster", (int)UniversalKeyCode.T);

        InvertCameraX = PlayerPrefs.GetInt("InvertCameraX", 0) == 1;
        InvertCameraY = PlayerPrefs.GetInt("InvertCameraY", 0) == 1;

        Debug.Log("Settings loaded: " + JsonUtility.ToJson(this));
    }
}
