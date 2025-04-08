using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
[CustomEditor(typeof(GridSpawner))]

public class GridSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Отображаем стандартный интерфейс для инспектора

        // Добавляем кнопку в редакторе
        GridSpawner gridSpawner = (GridSpawner)target;
        if (GUILayout.Button("Spawn Prefabs in Editor"))
        {
            // Вызываем метод для спавна объектов
            gridSpawner.SpawnPrefabsInEditor();
        }
    }
}
#endif
