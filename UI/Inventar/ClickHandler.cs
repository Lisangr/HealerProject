using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YG;
using System.Linq;

public class ClickHandler : MonoBehaviour
{
    public static ClickHandler instance;
    public Text textMeshPro; // Ссылка на UI текст
    public static Transform enemyPosition;
    public static float distance;
    public static string tempID;

    private int layerMask;

    private void Awake()
    {
        instance = this; // Сохраняем ссылку для доступа из других скриптов
    }

    void Start()
    {
        layerMask = LayerMask.GetMask("Collectable", "Enemy");
    }

    void Update()
    {
        // Если курсор над UI, не обрабатываем клики
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0)) // Проверка нажатия левой кнопки мыши
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject clickedObject = hit.collider.gameObject;

                // Получаем номера слоёв для "Collectable" и "Enemy"
                int collectableLayer = LayerMask.NameToLayer("Collectable");
                int enemyLayer = LayerMask.NameToLayer("Enemy");

                // Если клик по слою Collectable или Enemy
                if (clickedObject.layer == collectableLayer || clickedObject.layer == enemyLayer)
                {
                    // Проверяем, является ли объект врагом
                    Enemy enemy = clickedObject.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        textMeshPro.text = GetLocalizedEnemyName(enemy);
                        enemyPosition = clickedObject.transform;
                    }
                    else
                    {
                        // Проверяем, является ли объект предметом
                        ItemPickup itemPickup = clickedObject.GetComponent<ItemPickup>();
                        if (itemPickup != null)
                        {
                            textMeshPro.text = itemPickup.item.GetLocalizedItemName();
                            tempID = itemPickup.uniqueID;
                        }
                        else
                        {
                            textMeshPro.text = clickedObject.name;
                        }
                    }

                    // Луч для определения расстояния до объекта
                    if (Physics.Raycast(transform.position, clickedObject.transform.position - transform.position, out hit))
                    {
                        distance = hit.distance;
                        Debug.Log("Расстояние до цели: " + distance);
                    }
                }
                else
                {
                    // Если клик по объекту не из нужных слоёв – очищаем текстовое поле
                    textMeshPro.text = "";
                }
            }
        }
    }

    // Метод для очистки текстового поля
    public static void ClearTextField()
    {
        if (instance != null && instance.textMeshPro != null)
        {
            instance.textMeshPro.text = "";
        }
    }

    // Метод для определения кода языка (используя данные YandexGame или Application.systemLanguage)
    private string GetSystemLanguageCode()
    {
        if (YandexGame.EnvironmentData.language != null)
        {
            string currentLang = YandexGame.EnvironmentData.language.ToLower();
            switch (currentLang)
            {
                case "ru":
                    return "Ru";
                case "en":
                    return "en";
                case "tr":
                    return "tr";
                case "de":
                    return "de";
                case "es":
                    return "es";
                case "it":
                    return "it";
                case "fr":
                    return "fr";
                default:
                    return "en";
            }
        }
        else
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Russian:
                    return "Ru";
                case SystemLanguage.English:
                    return "en";
                case SystemLanguage.Turkish:
                    return "tr";
                case SystemLanguage.German:
                    return "de";
                case SystemLanguage.Spanish:
                    return "es";
                case SystemLanguage.Italian:
                    return "it";
                case SystemLanguage.French:
                    return "fr";
                default:
                    return "en";
            }
        }
    }

    // Метод локализации имени врага
    public string GetLocalizedEnemyName(Enemy enemy)
    {
        string currentLang = GetSystemLanguageCode().ToLower(); // например, "ru", "en" и т.д.
        if (enemy.enemyConfig.enemies.FirstOrDefault(e => e.enemyName == enemy.enemyName)?.localizations != null)
        {
            var enemyData = enemy.enemyConfig.enemies.FirstOrDefault(e => e.enemyName == enemy.enemyName);
            foreach (var loc in enemyData.localizations)
            {
                if (loc.languageCode.ToLower() == currentLang)
                    return loc.localizedName;
            }
        }
        return enemy.enemyName; // если нет локализованного варианта – вернуть имя по умолчанию
    }
}