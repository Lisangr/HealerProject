using UnityEngine;
using UnityEditor;

public class DeleteInactiveChildren : EditorWindow
{
    // Массив объектов, которые будем проверять
    public GameObject[] targetObjects;

    [MenuItem("Tools/Delete Inactive Children")]
    public static void ShowWindow()
    {
        GetWindow<DeleteInactiveChildren>("Delete Inactive Children");
    }

    private void OnGUI()
    {
        // Проверка инициализации массива
        if (targetObjects == null)
        {
            targetObjects = new GameObject[0];
        }

        // Отображаем поле для массива объектов, куда можно перетаскивать объекты
        EditorGUILayout.LabelField("Parent Objects");

        // Используем ObjectField для отображения каждого элемента массива
        for (int i = 0; i < targetObjects.Length; i++)
        {
            targetObjects[i] = (GameObject)EditorGUILayout.ObjectField($"Object {i + 1}", targetObjects[i], typeof(GameObject), true);
        }

        // Кнопка для добавления новых объектов в массив
        if (GUILayout.Button("Add Object"))
        {
            // Увеличиваем размер массива на 1 и добавляем новый объект
            ArrayUtility.Add(ref targetObjects, null);
        }

        // Кнопка для удаления неактивных детей
        if (GUILayout.Button("Delete Inactive Children"))
        {
            if (targetObjects != null && targetObjects.Length > 0)
            {
                foreach (var targetObject in targetObjects)
                {
                    if (targetObject != null)
                    {
                        DeleteInactiveObjects(targetObject);
                    }
                    else
                    {
                        Debug.LogWarning("One of the objects in the array is null.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Please assign parent objects to scan.");
            }
        }
    }

    private static void DeleteInactiveObjects(GameObject parentObject)
    {
        // Проверка, что объект существует
        if (parentObject == null)
        {
            Debug.LogError("Parent object is null.");
            return;
        }

        // Получаем все дочерние объекты
        Transform[] children = parentObject.GetComponentsInChildren<Transform>(true);

        // Проходим по всем дочерним объектам
        foreach (Transform child in children)
        {
            // Если объект не активен в иерархии, удаляем его
            if (!child.gameObject.activeInHierarchy)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        Debug.Log($"Inactive children for {parentObject.name} have been deleted.");
    }
}
