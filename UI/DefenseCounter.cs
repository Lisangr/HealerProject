using TMPro;
using UnityEngine;

public class DefenseCounter : MonoBehaviour
{
    public TextMeshProUGUI counterText;
    private int currentDefense;
    private const string DefenseKey = "PlayerDefense"; // Ключ для сохранения данных в PlayerPrefs

    void Start()
    {
        // Получаем данные игрока из PlayerConfig
        PlayerData playerData = PlayerConfig.Instance.players[0]; // Пример: берём первого игрока
        currentDefense = playerData.defense;

        LoadDefense(); // Загрузка значения защиты при старте
        counterText.text = currentDefense.ToString();
    }

    public void AddDefense(int defense)
    {
        currentDefense += defense;
        UpdateDefense();
    }

    public void RemoveDefense(int defense)
    {
        currentDefense -= defense;
        if (currentDefense <= 0)
        {
            currentDefense = 0;
        }
        UpdateDefense();
    }

    private void UpdateDefense()
    {
        PlayerPrefs.SetInt(DefenseKey, currentDefense); // Сохранение значения защиты в PlayerPrefs
        PlayerPrefs.Save(); // Сохранение всех изменений PlayerPrefs на диск
        counterText.text = currentDefense.ToString();
    }

    private void LoadDefense()
    {
        currentDefense = PlayerPrefs.GetInt(DefenseKey, currentDefense); // Загрузка значения защиты из PlayerPrefs
    }

    public int GetCurrentDefense()
    {
        return currentDefense; // Возвращаем текущее значение защиты
    }
}