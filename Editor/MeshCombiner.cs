using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MeshCombiner : EditorWindow
{
    GameObject parentObject;
    string savePath = "Assets/CombinedMesh.asset";

    [MenuItem("Tools/Mesh Combiner")]
    static void Init()
    {
        MeshCombiner window = (MeshCombiner)GetWindow(typeof(MeshCombiner));
        window.titleContent = new GUIContent("Mesh Combiner");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Объединение мешей", EditorStyles.boldLabel);
        parentObject = (GameObject)EditorGUILayout.ObjectField("Родительский объект", parentObject, typeof(GameObject), true);
        savePath = EditorGUILayout.TextField("Путь сохранения", savePath);

        if (GUILayout.Button("Объединить"))
        {
            CombineMeshes();
        }
    }

    void CombineMeshes()
    {
        if (parentObject == null)
        {
            Debug.LogError("Родительский объект не задан.");
            return;
        }

        MeshFilter[] meshFilters = parentObject.GetComponentsInChildren<MeshFilter>();
        List<CombineInstance> combineInstances = new List<CombineInstance>();

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.sharedMesh == null)
                continue;

            CombineInstance ci = new CombineInstance();
            ci.mesh = mf.sharedMesh;
            // Преобразуем координаты меша в пространство родителя
            ci.transform = mf.transform.localToWorldMatrix;
            combineInstances.Add(ci);
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // если много вершин
        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);

        AssetDatabase.CreateAsset(combinedMesh, savePath);
        AssetDatabase.SaveAssets();

        Debug.Log("Объединённый меш сохранён по пути: " + savePath);
    }
}
