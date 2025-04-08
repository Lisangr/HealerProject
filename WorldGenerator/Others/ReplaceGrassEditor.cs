#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[System.Serializable]
public class ReplacementRule
{
    public string nameContains;
    public GameObject[] replacementPrefabs;
}

public class ReplaceGrassEditor : MonoBehaviour
{
    public ReplacementRule[] replacementRules;
    public bool searchInChildren = true;
    public bool showDebugLogs = true;

    [MenuItem("Tools/Replace Objects By Name")]
    static void ReplaceObjects()
    {
        ReplaceGrassEditor instance = FindObjectOfType<ReplaceGrassEditor>();
        if (instance == null)
        {
            Debug.LogError("ReplaceGrassEditor не найден в сцене!");
            return;
        }

        // Начинаем запись операций для Undo
        Undo.SetCurrentGroupName("Mass Replacement");
        int group = Undo.GetCurrentGroup();

        try
        {
            foreach (var rule in instance.replacementRules)
            {
                if (string.IsNullOrEmpty(rule.nameContains) || rule.replacementPrefabs == null || rule.replacementPrefabs.Length == 0)
                {
                    Debug.LogError($"Некорректное правило замены: {rule.nameContains}");
                    continue;
                }

                // Собираем все трансформы перед обработкой
                List<TransformData> targets = new List<TransformData>();
                foreach (GameObject root in GetSceneRoots())
                {
                    FindTransformsByNameRecursive(
                        root.transform,
                        rule.nameContains,
                        ref targets,
                        instance.searchInChildren
                    );
                }

                if (instance.showDebugLogs)
                    Debug.Log($"Найдено {targets.Count} объектов по паттерну '{rule.nameContains}'");

                // Обрабатываем все собранные трансформы
                foreach (var transformData in targets)
                {
                    ReplaceObject(transformData, rule.replacementPrefabs);
                }
            }
        }
        finally
        {
            // Объединяем все операции в одну группу
            Undo.CollapseUndoOperations(group);
        }
    }

    static void ReplaceObject(TransformData oldTransformData, GameObject[] prefabs)
    {
        GameObject newPrefab = prefabs[Random.Range(0, prefabs.Length)];

        // Создаем новый объект
        GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(newPrefab);

        // Применяем параметры трансформации
        newObj.transform.SetPositionAndRotation(
            oldTransformData.position,
            oldTransformData.rotation
        );
        newObj.transform.localScale = oldTransformData.scale;
        newObj.transform.SetParent(oldTransformData.parent);

        // Уничтожаем старый объект
        if (oldTransformData.gameObject != null)
        {
            Undo.DestroyObjectImmediate(oldTransformData.gameObject);
        }

        Undo.RegisterCreatedObjectUndo(newObj, "Replace object");
    }

    static List<GameObject> GetSceneRoots()
    {
        List<GameObject> roots = new List<GameObject>();
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (scene.isLoaded) roots.AddRange(scene.GetRootGameObjects());
        }
        return roots;
    }

    static void FindTransformsByNameRecursive(
        Transform current,
        string searchPattern,
        ref List<TransformData> results,
        bool searchInChildren
    )
    {
        if (current.name.IndexOf(searchPattern, System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            results.Add(new TransformData(current));
            if (!searchInChildren) return;
        }

        foreach (Transform child in current)
        {
            FindTransformsByNameRecursive(child, searchPattern, ref results, searchInChildren);
        }
    }

    // Класс для сохранения данных трансформации
    private struct TransformData
    {
        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly Vector3 scale;
        public readonly Transform parent;
        public readonly GameObject gameObject;

        public TransformData(Transform t)
        {
            position = t.position;
            rotation = t.rotation;
            scale = t.localScale;
            parent = t.parent;
            gameObject = t.gameObject;
        }
    }
}
#endif