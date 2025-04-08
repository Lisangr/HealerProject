using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
[CustomEditor(typeof(GridSpawner))]

public class GridSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // ���������� ����������� ��������� ��� ����������

        // ��������� ������ � ���������
        GridSpawner gridSpawner = (GridSpawner)target;
        if (GUILayout.Button("Spawn Prefabs in Editor"))
        {
            // �������� ����� ��� ������ ��������
            gridSpawner.SpawnPrefabsInEditor();
        }
    }
}
#endif
