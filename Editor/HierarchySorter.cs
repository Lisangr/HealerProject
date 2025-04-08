using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class HierarchySorter : EditorWindow
{
    [MenuItem("Tools/Сортировать выбранные объекты по алфавиту")]
    public static void ShowWindow()
    {
        GetWindow<HierarchySorter>("Сортировка в Иерархии");
    }

    private void OnGUI()
    {
        GUILayout.Label("Сортировка объектов в иерархии по алфавиту", EditorStyles.boldLabel);
        if (GUILayout.Button("Сортировать детей выбранных объектов"))
        {
            SortSelectedObjects();
        }
    }

    private void SortSelectedObjects()
    {
        // Получаем выбранные объекты в иерархии
        Transform[] selectedTransforms = Selection.transforms;
        if (selectedTransforms.Length == 0)
        {
            EditorUtility.DisplayDialog("Внимание", "Выберите объект(ы) для сортировки дочерних объектов", "OK");
            return;
        }

        foreach (Transform parent in selectedTransforms)
        {
            SortChildrenAlphabetically(parent);
        }
    }

    private void SortChildrenAlphabetically(Transform parent)
    {
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < parent.childCount; i++)
        {
            children.Add(parent.GetChild(i));
        }
        // Сортировка по имени
        children.Sort((a, b) => string.Compare(a.name, b.name));

        // Устанавливаем новый порядковый индекс для каждого ребенка
        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetSiblingIndex(i);
        }
    }
}
