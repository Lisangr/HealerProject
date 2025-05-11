using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpellManager : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private SpellConfig spellConfig; // Конфиг всех заклинаний
    [SerializeField] private GameObject spellSlotPrefab; // Префаб слота заклинания
    [SerializeField] private Transform spellSlotsParent; // Родительский объект для слотов

    [Header("Начальные заклинания")]
    [SerializeField] private string[] initialSpells = new string[5]; // ID заклинаний для каждого слота

    [Header("Слоты заклинаний")]
    [SerializeField] private List<Spell> spellSlots = new List<Spell>(); // Активные заклинания в слотах

    private GameObject player;
    private const int MAX_SLOTS = 5;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        InitializeSpellSlots();
        InitializeInitialSpells();
    }

    private void InitializeInitialSpells()
    {
        for (int i = 0; i < initialSpells.Length; i++)
        {
            if (!string.IsNullOrEmpty(initialSpells[i]))
            {
                AssignSpell(initialSpells[i], i);
            }
        }
    }

    private void InitializeSpellSlots()
    {
        // Создаем слоты для заклинаний
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            GameObject slotObject = Instantiate(spellSlotPrefab, spellSlotsParent);
            Spell spellComponent = slotObject.GetComponent<Spell>();

            if (spellComponent != null)
            {
                spellSlots.Add(spellComponent);

                // Настраиваем UI элементы слота (если есть)
                Image iconImage = slotObject.GetComponentInChildren<Image>();
                Text hotKeyText = slotObject.GetComponentInChildren<Text>();
                if (hotKeyText != null)
                {
                    hotKeyText.text = (i + 1).ToString();
                }
            }
        }
    }

    private void Update()
    {
        // Проверяем нажатие клавиш 1-5
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                CastSpell(i);
            }
        }
    }

    // Применить заклинание из слота
    private void CastSpell(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= spellSlots.Count)
            return;

        Spell spell = spellSlots[slotIndex];
        if (spell != null && spell.IsReady())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            GameObject target = null;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                target = hit.collider.gameObject;

                // Особенная обработка для врагов
                if (target.CompareTag("Enemy"))
                {
                    if (spell.CanCast(target))
                    {
                        spell.Cast(target);
                        return;
                    }
                }
            }

            // Если не нашли врага, пробуем применить на игрока
            if (spell.CanCast(player))
            {
                spell.Cast(player);
            }
        }
    }
    // Назначить заклинание в слот
    public void AssignSpell(string spellID, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= spellSlots.Count)
            return;

        SpellData spellData = spellConfig.GetSpellDataByID(spellID);
        if (spellData != null)
        {
            Spell spell = spellSlots[slotIndex];
            spell.Initialize(spellData, player);

            // Обновляем UI слота (если есть)
            Image iconImage = spell.GetComponentInChildren<Image>();
            if (iconImage != null && spellData.castVFXPrefab != null)
            {
                // Здесь можно установить иконку заклинания
                // iconImage.sprite = spellData.icon;
            }
        }
    }

    // Очистить слот заклинания
    public void ClearSpell(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= spellSlots.Count)
            return;

        spellSlots[slotIndex].Initialize(null, null);

        // Очищаем UI слота (если есть)
        Image iconImage = spellSlots[slotIndex].GetComponentInChildren<Image>();
        if (iconImage != null)
        {
            iconImage.sprite = null;
        }
    }
}