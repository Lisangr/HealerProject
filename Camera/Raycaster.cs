using UnityEngine;
using UnityEngine.UI;

public class Raycaster : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float rayDistance = 100f; // для обычного raycast (Action/Take)
    public Text textRay;
        
    private Player player;

    private void Start()
    {
        // Блокировка курсора в центре экрана
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        player = FindObjectOfType<Player>();
        if (player == null)
            Debug.LogError("Player не найден на сцене!");
    }

    private void Update()
    {
        ProcessInput();       
    }

    // Обработка ввода для разных действий
    void ProcessInput()
    {
        // Если нажата клавиша действия (ActionKey) – обычный raycast
        if (Input.GetKeyDown((KeyCode)InputSettings.Instance.ActionKey))
        {
            CastAndDisplay("Action");
        }

        // Если нажата клавиша взятия (TakeKey) – обычный raycast
        if (Input.GetKeyDown((KeyCode)InputSettings.Instance.TakeKey))
        {
            CastAndDisplay("Take");
        }
       
    }

    // Метод для выполнения обычного raycast (для Action и Take)
    void CastAndDisplay(string action)
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            textRay.text = $"{action}: {hit.collider.gameObject.name}";
        }
        else
        {
            textRay.text = "";
        }
    }
    
    
}
