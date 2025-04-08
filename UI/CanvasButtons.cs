using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasButtons : MonoBehaviour
{
    #region Dataset
    [Header("Panels")]
    public GameObject helpPanel;       // H
    public GameObject settingsPanel;   // O
    public GameObject menuPanel;       // Tab
    public GameObject defeatPanel;
    public GameObject questPanel;      // L
    public GameObject inventoryPanel;  // I 
    public GameObject characterPanel;  // C 
    public GameObject mapPanel;        // M
    public GameObject craftPanel;
    public GameObject reciptePanel;

    [Header("Character Panel Data")]
    public Text lvlCountText;
    public Text expCountText;
    public Text statPointCounter;
    public Text powerCountText;
    public Text dexteryCountText;
    public Text staminaCountText;
    public Text defenceCountText;
    public Text intellectCountText;
    public Button powerCountButton;
    public Button dexteryCountButton;
    public Button staminaCountButton;
    public Button defenceCountButton;
    public Button intellectCountButton;

    [Header("Base Stats Display")]
    public Text baseHealthText;
    public Text baseDefenseText;
    public Text baseMoveSpeedText;
    public Text baseDamageText;
    public Text baseIntellectText;

    [Header("Setting Tabs")]
    public GameObject graphicsTabPanel;
    public GameObject audioTabtPanel;
    public GameObject controllsTabPanel;

    [Header("Toggles")]
    public Toggle toggleBushes;
    public Toggle toggleGrass;
    public Toggle toggleRocks;
    public Toggle toggleWeather;

    [Header("CraftChoise")]
    public Button craftButton;
    public Button recipteButton;

    [Header("Quests Panels")]
    public GameObject mainQuestsPanel;
    public GameObject additionalQuestPanel;
    public GameObject huntingQuestPanel;

    [Header("Tame Text")]
    public Text tameDone;
    public Text tameFalled;

    [Header("Menu Buttons")]
    public Button continueGameButton; // ������ " "
    public Button newGameButton;      //  " "

    private PlayerStat playerStat;
    private const string LvlKey = "PlayerLvl";
    private const string ExpKey = "PlayerExp";
    private const string PowerKey = "PlayerPower";
    private const string DexteryKey = "PlayerDextery";
    private const string StaminaKey = "PlayerStamina";
    private const string DefenceKey = "PlayerDefence";
    private const string IntellectKey = "PlayerIntellect";
    private const string StatPointKey = "StatPoints";

    private const string BaseHealthKey = "BaseHealthText";
    private const string BaseDamageKey = "BaseDamage";
    private const string BaseDefenseKey = "BaseDefense";
    private const string BaseMoveSpeedKey = "BaseMoveSpeed";
    private const string BaseIntellectKey = "BaseTimeIntellect";
    #endregion
    private void OnEnable()    {
      
        Player.OnEnemyTame += Player_OnEnemyTame;
        UpdateCursorLock();
        UpdateContinueGameButton(); // ��������� ������� ����������� ��������� ��� ���������
    }

    private void Player_OnEnemyTame(int num)
    {
        if (num == 1)
        {
            TameDone();
        }
        else
        {
            TameFalled();
        }
    }

    private void OnDisable()
    {       
        Player.OnEnemyTame -= Player_OnEnemyTame;
    }

    #region Initialize and Update
    private void Awake()
    {
        UpdateUI();
        OnCloseButtonClick();
        playerStat = FindObjectOfType<PlayerStat>();

        if (continueGameButton != null)
        {
            if (PlayerSaveSystem.HasSnapshots())
                continueGameButton.gameObject.SetActive(true);
            else
                continueGameButton.gameObject.SetActive(false);
        }
    }
    private void Start()
    {
        if (toggleBushes != null)
            toggleBushes.onValueChanged.AddListener(ToggleBushes);
        if (toggleGrass != null)
            toggleGrass.onValueChanged.AddListener(ToggleGrass);
        if (toggleRocks != null)
            toggleRocks.onValueChanged.AddListener(ToggleRocks);
        if (toggleWeather != null)
            toggleWeather.onValueChanged.AddListener(ToggleWeather);

        if (powerCountButton != null)
            powerCountButton.onClick.AddListener(PowerAdd);
        if (dexteryCountButton != null)
            dexteryCountButton.onClick.AddListener(DexteryAdd);
        if (staminaCountButton != null)
            staminaCountButton.onClick.AddListener(StaminaAdd);
        if (defenceCountButton != null)
            defenceCountButton.onClick.AddListener(DefenceAdd);
        if (intellectCountButton != null)
            intellectCountButton.onClick.AddListener(IntellectAdd);

        if (craftButton != null)
            craftButton.onClick.AddListener(CraftButtonAdd);
        if (recipteButton != null)
            recipteButton.onClick.AddListener(RecipteButtonAdd);

        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnPlayButtonClick);
        if (continueGameButton != null)
            continueGameButton.onClick.AddListener(OnContinueGameButtonClick);
    }

    private void UpdateUI()
    {
        if (tameFalled != null)
            tameFalled.gameObject.SetActive(false);

        if (tameDone != null)
            tameDone.gameObject.SetActive(false);

        lvlCountText.text = PlayerPrefs.GetInt(LvlKey, 1).ToString();
        expCountText.text = PlayerPrefs.GetInt(ExpKey, 0).ToString();
        statPointCounter.text = PlayerPrefs.GetInt(StatPointKey, 0).ToString();
        powerCountText.text = PlayerPrefs.GetInt(PowerKey).ToString();
        dexteryCountText.text = PlayerPrefs.GetInt(DexteryKey).ToString();
        staminaCountText.text = PlayerPrefs.GetInt(StaminaKey).ToString();
        defenceCountText.text = PlayerPrefs.GetInt(DefenceKey).ToString();
        intellectCountText.text = PlayerPrefs.GetInt(IntellectKey).ToString();

        baseHealthText.text = PlayerPrefs.GetInt(BaseHealthKey).ToString();
        baseDefenseText.text = PlayerPrefs.GetInt(BaseDefenseKey).ToString();
        baseMoveSpeedText.text = PlayerPrefs.GetFloat(BaseMoveSpeedKey).ToString();
        baseDamageText.text = PlayerPrefs.GetInt(BaseDamageKey).ToString();
        baseIntellectText.text = PlayerPrefs.GetInt(BaseIntellectKey).ToString();

        Debug.Log($"���������� UI: ������� {lvlCountText.text}, ���� {expCountText.text}, ���������� {statPointCounter.text}, " +
                  $"���� {powerCountText.text}, �������� {dexteryCountText.text}, ������������ {staminaCountText.text}, " +
                  $"������ {defenceCountText.text}, ��������� {intellectCountText.text}, �������� {baseHealthText.text}, " +
                  $"������ {baseDefenseText.text}, �������� {baseMoveSpeedText.text}, ���� {baseDamageText.text}, ��������� {baseIntellectText.text}");

        int statPoint = PlayerPrefs.GetInt(StatPointKey, 0);
        if (statPoint > 0)
        {
            powerCountButton.gameObject.SetActive(true);
            dexteryCountButton.gameObject.SetActive(true);
            staminaCountButton.gameObject.SetActive(true);
            defenceCountButton.gameObject.SetActive(true);
            intellectCountButton.gameObject.SetActive(true);
        }
        else
        {
            powerCountButton.gameObject.SetActive(false);
            dexteryCountButton.gameObject.SetActive(false);
            staminaCountButton.gameObject.SetActive(false);
            defenceCountButton.gameObject.SetActive(false);
            intellectCountButton.gameObject.SetActive(false);
        }
    }
    #endregion   

    #region ButtonsChecking
    private void Update()
    {
        // Обработка клавиши Escape для разблокировки курсора
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool isCursorLocked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = isCursorLocked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !isCursorLocked;
        }

        // Проверка нажатия клавиш для панелей
        if (InputUtils.GetKeyDown(InputSettings.Instance.MenuKey))
        {
            TogglePanel(menuPanel);
        }
        if (InputUtils.GetKeyDown(InputSettings.Instance.SettingsKey))
        {
            TogglePanel(settingsPanel);
        }
        if (InputUtils.GetKeyDown(InputSettings.Instance.TasksKey))
        {
            TogglePanel(questPanel);
            ShowMainQuestsPanelTab();
        }
        if (InputUtils.GetKeyDown(InputSettings.Instance.MapKey))
        {
            TogglePanel(mapPanel);
        }
        if (InputUtils.GetKeyDown(InputSettings.Instance.HelpKey))
        {
            TogglePanel(helpPanel);
        }
        if (InputUtils.GetKeyDown(InputSettings.Instance.CharacterKey))
        {
            TogglePanel(characterPanel);
            UpdateUI();
        }
        if (InputUtils.GetKeyDown(InputSettings.Instance.InventoryKey))
        {
            TogglePanel(inventoryPanel);
            UpdateCursorLock();
        }
    }

    // ����� ��� ������������ ��������� ������ � ����������� ���������� �������
    private void TogglePanel(GameObject panel)
    {
        if (panel == null) return;

        bool wasActive = panel.activeSelf;
        OnCloseButtonClick(); // ��������� ��� ������
        if (!wasActive)
        {
            panel.SetActive(true); // ��������� ������� ������, ���� ��� ���� �������
        }
        UpdateCursorLock(); // ����� �������� ���� ������� ����� ��������� ��������� �������
    }
    #endregion

    #region ButtonsListeners
    public void PowerAdd()
    {
        playerStat.CalculatePower();
        // Принудительно обновляем интерфейс после изменения
        int currentStatPoints = PlayerPrefs.GetInt("StatPoints", 0);
        Debug.Log($"После добавления силы, осталось очков: {currentStatPoints}");
        // Сохраняем в ExpBar, если он доступен
        ExpBar expBar = FindObjectOfType<ExpBar>();
        if (expBar != null)
        {
            expBar.UpdateStatPoints(currentStatPoints);
        }
        UpdateUI();
    }

    public void DexteryAdd()
    {
        playerStat.CalculateDextery();
        // Принудительно обновляем интерфейс после изменения
        int currentStatPoints = PlayerPrefs.GetInt("StatPoints", 0);
        Debug.Log($"После добавления ловкости, осталось очков: {currentStatPoints}");
        // Сохраняем в ExpBar, если он доступен
        ExpBar expBar = FindObjectOfType<ExpBar>();
        if (expBar != null)
        {
            expBar.UpdateStatPoints(currentStatPoints);
        }
        UpdateUI();
    }

    public void StaminaAdd()
    {
        playerStat.CalculateStamina();
        // Принудительно обновляем интерфейс после изменения
        int currentStatPoints = PlayerPrefs.GetInt("StatPoints", 0);
        Debug.Log($"После добавления выносливости, осталось очков: {currentStatPoints}");
        // Сохраняем в ExpBar, если он доступен
        ExpBar expBar = FindObjectOfType<ExpBar>();
        if (expBar != null)
        {
            expBar.UpdateStatPoints(currentStatPoints);
        }
        UpdateUI();
    }

    public void DefenceAdd()
    {
        playerStat.CalculateDefence();
        // Принудительно обновляем интерфейс после изменения
        int currentStatPoints = PlayerPrefs.GetInt("StatPoints", 0);
        Debug.Log($"После добавления защиты, осталось очков: {currentStatPoints}");
        // Сохраняем в ExpBar, если он доступен
        ExpBar expBar = FindObjectOfType<ExpBar>();
        if (expBar != null)
        {
            expBar.UpdateStatPoints(currentStatPoints);
        }
        UpdateUI();
    }

    public void IntellectAdd()
    {
        playerStat.CalculateIntellect();
        // Принудительно обновляем интерфейс после изменения
        int currentStatPoints = PlayerPrefs.GetInt("StatPoints", 0);
        Debug.Log($"После добавления интеллекта, осталось очков: {currentStatPoints}");
        // Сохраняем в ExpBar, если он доступен
        ExpBar expBar = FindObjectOfType<ExpBar>();
        if (expBar != null)
        {
            expBar.UpdateStatPoints(currentStatPoints);
        }
        UpdateUI();
    }

    public void CraftButtonAdd()
    {
        craftPanel.SetActive(true);
        reciptePanel.SetActive(false);
    }

    public void RecipteButtonAdd()
    {
        craftPanel.SetActive(false);
        reciptePanel.SetActive(true);
    }    

    #endregion

    #region TogglesMetods
    public void ToggleBushes(bool isOn)
    {
        if (GlobalToggleManager.Instance != null)
        {
            GlobalToggleManager.Instance.bushesEnabled = isOn;
        }

        // ���������� Resources.FindObjectsOfTypeAll ��� ������ ���� ���������� ��������
        BushToggle[] bushes = Resources.FindObjectsOfTypeAll<BushToggle>();
        foreach (BushToggle bush in bushes)
        {
            // ���������: ���������, ��� ������ ����������� ����������� �����
            if (bush.gameObject.scene.isLoaded)
            {
                bush.SetActiveState(isOn);
            }
        }
    }

    public void ToggleGrass(bool isOn)
    {
        if (GlobalToggleManager.Instance != null)
        {
            GlobalToggleManager.Instance.grassEnabled = isOn;
        }

        // ���������� Resources.FindObjectsOfTypeAll ��� ������ ���� ���������� ��������
        GrassToggle[] grases = Resources.FindObjectsOfTypeAll<GrassToggle>();
        foreach (GrassToggle grass in grases)
        {
            // ���������: ���������, ��� ������ ����������� ����������� �����
            if (grass.gameObject.scene.isLoaded)
            {
                grass.SetActiveState(isOn);
            }
        }
    }

    public void ToggleRocks(bool isOn)
    {
        if (GlobalToggleManager.Instance != null)
        {
            GlobalToggleManager.Instance.rocksEnabled = isOn;
        }

        // ���������� Resources.FindObjectsOfTypeAll ��� ������ ���� ���������� ��������
        RocksToggle[] rocks = Resources.FindObjectsOfTypeAll<RocksToggle>();
        foreach (RocksToggle rock in rocks)
        {
            // ���������: ���������, ��� ������ ����������� ����������� �����
            if (rock.gameObject.scene.isLoaded)
            {
                rock.SetActiveState(isOn);
            }
        }
    }
    public void ToggleWeather(bool isOn)
    {
        if (GlobalToggleManager.Instance != null)
        {
            GlobalToggleManager.Instance.rocksEnabled = isOn;
        }

        // ���������� Resources.FindObjectsOfTypeAll ��� ������ ���� ���������� ��������
        WeatherToggle[] weathers = Resources.FindObjectsOfTypeAll<WeatherToggle>();
        foreach (WeatherToggle weather in weathers)
        {
            // ���������: ���������, ��� ������ ����������� ����������� �����
            if (weather.gameObject.scene.isLoaded)
            {
                weather.SetActiveState(isOn);
            }
        }
    }
    #endregion

    #region ButtonsMetods
    public void OnCloseButtonClick()
    {
        if (helpPanel != null) helpPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (questPanel != null) questPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (characterPanel != null) characterPanel.SetActive(false);
        if (mapPanel != null) mapPanel.SetActive(false);
    }

    public void ShowQuestPanel() => questPanel.SetActive(true);
    public void ShowDefeatPanel() => defeatPanel.SetActive(true);
    public void OnHelpButtonClick() => helpPanel.SetActive(true);
    public void OnInventoryButtonClick() => inventoryPanel.SetActive(true);
    public void OnSettingsButtonClick() => settingsPanel.SetActive(true);
    public void OnPauseMenuClick() => menuPanel.SetActive(true);
    public void OnCharacteristicsPanelClick() => characterPanel.SetActive(true);
    public void OnShowMapButtonClick() => mapPanel.SetActive(true);
    public void ShowMainQuestsPanelTab()
    {
        mainQuestsPanel.SetActive(true);
        huntingQuestPanel.SetActive(false);
        additionalQuestPanel.SetActive(false);
    }
    public void ShowHuntingQuestPanelTab()
    {
        mainQuestsPanel.SetActive(false);
        huntingQuestPanel.SetActive(true);
        additionalQuestPanel.SetActive(false);
    }
    public void ShowAdditionalQuestPanelTab()
    {
        mainQuestsPanel.SetActive(false);
        huntingQuestPanel.SetActive(false);
        additionalQuestPanel.SetActive(true);
    }
    public void ShowGraphicsTab()
    { 
        graphicsTabPanel.SetActive(true);
        audioTabtPanel.SetActive(false);
        controllsTabPanel.SetActive(false);
    }
    public void ShowAudioTab() 
    {
        audioTabtPanel.SetActive(true);
        graphicsTabPanel.SetActive(false);
        controllsTabPanel.SetActive(false);
    }
    public void ShowControllsTab() 
    { 
        controllsTabPanel.SetActive(true);
        audioTabtPanel.SetActive(false);
        graphicsTabPanel.SetActive(false);
    }
    public void OnGoToMenuButton()
    {
        ExitAndSave();
    }
    private static void ExitAndSave()
    {
        // ������ ������ ��������� ������ � ��������� ���
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            PlayerSnapshot snapshot = new PlayerSnapshot
            {
                timeStamp = Time.time,
                position = player.transform.position,
                rotation = player.transform.rotation,
                currentHealth = player.currentHealth
            };

            ExpBar expBar = FindObjectOfType<ExpBar>();
            if (expBar != null)
            {
                snapshot.currentExp = expBar.GetCurrentExp();
                snapshot.currentLevel = expBar.GetCurrentLevel();
                snapshot.statPoints = expBar.GetStatPoints();
                snapshot.expForLevelUp = expBar.GetExpForLevelUp();
            }
            PlayerSaveSystem.StaticSnapshots.Add(snapshot);
        }

        // ������������� ���� ��� ����������� ����
        PlayerPrefs.SetInt("ContinueGame", 1);
        PlayerPrefs.Save();

        // ������� ��� ������� DontDestroyOnLoad (��������, ������� ������, ������� ������ � �.�.)
        ClearDontDestroyOnLoad();

        // ��������� ����� ���� � ������ Single, ��� ����������� �������� ���������� ����
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

    public void OnExitButtonClick()
    {
#if UNITY_EDITOR
        // ���� ���� �������� � ���������, ��������� �
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ���� ���� �������� � ������, ��������� ����������
        ExitAndSave();
        Application.Quit();
#endif
    }
    public void RestartCurrentLevel()
    {
       ////
    }
    public void OnContinueGameButtonClick()
    {
        // ������������� ���� ��� ����������� ����
        PlayerPrefs.SetInt("ContinueGame", 1);
        PlayerPrefs.Save();

        // ��������� ����� ����������� ����
        SceneManager.LoadScene("SceneForAsincLoading");
        Destroy(this.gameObject);
    }

    public void RestartCurrentScene()
    {       
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
    /// <summary>
    /// ����� ��� ������ "����� ����". �� ����� ���������� � OnPlayButtonClick.
    /// ����� ��������� ����� ���� ��������� ��� ����������� ��������.
    /// </summary>
    public void OnPlayButtonClick()
    {
        // ������� ����������� ���
        ClearAllSnapshots();
        PlayerPrefs.DeleteAll();

        // ��������� ����� ����� ���� (��� ����� � ����������� ���������)
        SceneManager.LoadScene("SceneForAsincLoading");
        Destroy(this.gameObject);
    }
    #endregion

    #region Tame Text
    private void TameDone()
    {
        StartCoroutine(TameDoneCo());
    }
    private void TameFalled()
    {
        StartCoroutine(TameFalledCo());
    }
    private IEnumerator TameDoneCo()
    {
        tameDone.gameObject.SetActive(true);
        tameFalled.gameObject.SetActive(false);
        yield return new WaitForSeconds(3f);
        tameDone.gameObject.SetActive(false);
    }
    private IEnumerator TameFalledCo()
    {
        tameDone.gameObject.SetActive(false);
        tameFalled.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        tameFalled.gameObject.SetActive(false);
    }
    #endregion
    /// <summary>
    /// ����� ��� ���������� ���������� �������
    /// ����� �� ��������� ����������, ���� ������� ��������� ��� ���� (��� ������������� ����� ��������� �������)
    /// </summary>   
    private void UpdateCursorLock()
    {
        // Если текущая сцена "Menu", разблокируем курсор
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // Если курсор заблокирован и нажата Escape, разблокируем
        if (Cursor.lockState == CursorLockMode.Locked && Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // Если открыта любая из панелей, разблокируем курсор
        if ((settingsPanel != null && settingsPanel.activeSelf)
            || (menuPanel != null && menuPanel.activeSelf)
            || (questPanel != null && questPanel.activeSelf)
            || (characterPanel != null && characterPanel.activeSelf)
            || (inventoryPanel != null && inventoryPanel.activeSelf))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // В остальных случаях блокируем курсор
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// ��������� ������� ����������� ��������� � ������� ���������� � ���������� ��� ������������ ������ "���������� ����".
    /// </summary>
    private void UpdateContinueGameButton()
    {
        if (continueGameButton != null)
            continueGameButton.gameObject.SetActive(PlayerSaveSystem.HasSnapshots());
    }

    /// <summary>
    /// ������� ��� ����������� ��������.
    /// </summary>
    private void ClearAllSnapshots()
    {
        PlayerSaveSystem.ClearSnapshots();
    }
    public static void ClearDontDestroyOnLoad()
    {
        // ������ ��������� ������, ����� �������� ������ �� ����� DontDestroyOnLoad
        GameObject temp = new GameObject("Temp");
        DontDestroyOnLoad(temp);

        // �������� �����, � ������� �������� ��� ������� DontDestroyOnLoad
        Scene dontDestroyScene = temp.scene;

        // ������� ��������� ������
        Destroy(temp);

        // ���������� ��� �������� ������� � ���� ����� � ���������� ��
        GameObject[] rootObjects = dontDestroyScene.GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            Destroy(obj);
        }
    }

}