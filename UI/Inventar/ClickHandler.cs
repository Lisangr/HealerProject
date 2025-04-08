using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YG;
using System.Linq;

public class ClickHandler : MonoBehaviour
{
    public static ClickHandler instance;
    public Text textMeshPro; // ������ �� UI �����
    public static Transform enemyPosition;
    public static float distance;
    public static string tempID;

    private int layerMask;

    private void Awake()
    {
        instance = this; // ��������� ������ ��� ������� �� ������ ��������
    }

    void Start()
    {
        layerMask = LayerMask.GetMask("Collectable", "Enemy");
    }

    void Update()
    {
        // ���� ������ ��� UI, �� ������������ �����
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0)) // �������� ������� ����� ������ ����
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject clickedObject = hit.collider.gameObject;

                // �������� ������ ���� ��� "Collectable" � "Enemy"
                int collectableLayer = LayerMask.NameToLayer("Collectable");
                int enemyLayer = LayerMask.NameToLayer("Enemy");

                // ���� ���� �� ���� Collectable ��� Enemy
                if (clickedObject.layer == collectableLayer || clickedObject.layer == enemyLayer)
                {
                    // ���������, �������� �� ������ ������
                    Enemy enemy = clickedObject.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        textMeshPro.text = GetLocalizedEnemyName(enemy);
                        enemyPosition = clickedObject.transform;
                    }
                    else
                    {
                        // ���������, �������� �� ������ ���������
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

                    // ��� ��� ����������� ���������� �� �������
                    if (Physics.Raycast(transform.position, clickedObject.transform.position - transform.position, out hit))
                    {
                        distance = hit.distance;
                        Debug.Log("���������� �� ����: " + distance);
                    }
                }
                else
                {
                    // ���� ���� �� ������� �� �� ������ ���� � ������� ��������� ����
                    textMeshPro.text = "";
                }
            }
        }
    }

    // ����� ��� ������� ���������� ����
    public static void ClearTextField()
    {
        if (instance != null && instance.textMeshPro != null)
        {
            instance.textMeshPro.text = "";
        }
    }

    // ����� ��� ����������� ���� ����� (��������� ������ YandexGame ��� Application.systemLanguage)
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

    // ����� ����������� ����� �����
    public string GetLocalizedEnemyName(Enemy enemy)
    {
        string currentLang = GetSystemLanguageCode().ToLower(); // ��������, "ru", "en" � �.�.
        if (enemy.enemyConfig.enemies.FirstOrDefault(e => e.enemyName == enemy.enemyName)?.localizations != null)
        {
            var enemyData = enemy.enemyConfig.enemies.FirstOrDefault(e => e.enemyName == enemy.enemyName);
            foreach (var loc in enemyData.localizations)
            {
                if (loc.languageCode.ToLower() == currentLang)
                    return loc.localizedName;
            }
        }
        return enemy.enemyName; // ���� ��� ��������������� �������� � ������� ��� �� ���������
    }
}