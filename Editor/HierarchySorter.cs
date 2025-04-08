using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class HierarchySorter : EditorWindow
{
    [MenuItem("Tools/����������� ��������� ������� �� ��������")]
    public static void ShowWindow()
    {
        GetWindow<HierarchySorter>("���������� � ��������");
    }

    private void OnGUI()
    {
        GUILayout.Label("���������� �������� � �������� �� ��������", EditorStyles.boldLabel);
        if (GUILayout.Button("����������� ����� ��������� ��������"))
        {
            SortSelectedObjects();
        }
    }

    private void SortSelectedObjects()
    {
        // �������� ��������� ������� � ��������
        Transform[] selectedTransforms = Selection.transforms;
        if (selectedTransforms.Length == 0)
        {
            EditorUtility.DisplayDialog("��������", "�������� ������(�) ��� ���������� �������� ��������", "OK");
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
        // ���������� �� �����
        children.Sort((a, b) => string.Compare(a.name, b.name));

        // ������������� ����� ���������� ������ ��� ������� �������
        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetSiblingIndex(i);
        }
    }
}
