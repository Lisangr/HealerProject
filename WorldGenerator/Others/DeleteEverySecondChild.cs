using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(DeleteEverySecondChild))]
public class DeleteEverySecondChildEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DeleteEverySecondChild script = (DeleteEverySecondChild)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Delete Every Second Child", GUILayout.Height(30)))
        {

            script.DeleteEverySecondChildObjects();

        }

        EditorGUILayout.HelpBox("������� �������� ������� � ��������� ���������\n(������� � 0)", MessageType.Info);
    }
}

public class DeleteEverySecondChild : MonoBehaviour
{
    [ContextMenu("Delete Every Second Child")]
    public void DeleteEverySecondChildObjects() // ������� ����� ���������
    {
        int childCount = transform.childCount;

        // �������� ������� ��� ��������
        for (int i = childCount - 1; i >= 0; i--)
        {
            if (i % 2 == 1)
            {
                Undo.DestroyObjectImmediate(transform.GetChild(i).gameObject); // �������� Undo
            }
        }
    }
}
#endif
