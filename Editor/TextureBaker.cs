using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class TextureBaker : EditorWindow
{
    // ������, ������� ��� �������� ����� ������
    GameObject targetObject;
    // ������, � ������� ����� ������������� ���������
    Camera renderCamera;
    // ���������� ��������� �����������
    int resolutionWidth = 1024;
    int resolutionHeight = 1024;
    // ���� ��� ���������� �����
    string savePath = "Assets/BakedTexture.png";
    // ������ ���������� ���� (���������, ��� ���� ���� �� ������������ � �����)
    int bakeLayer = 31;

    [MenuItem("Tools/Texture Baker")]
    static void Init()
    {
        TextureBaker window = (TextureBaker)GetWindow(typeof(TextureBaker));
        window.titleContent = new GUIContent("Texture Baker");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("��������� ��������", EditorStyles.boldLabel);

        targetObject = (GameObject)EditorGUILayout.ObjectField("������� ������", targetObject, typeof(GameObject), true);
        renderCamera = (Camera)EditorGUILayout.ObjectField("������ ��� �������", renderCamera, typeof(Camera), true);
        resolutionWidth = EditorGUILayout.IntField("������", resolutionWidth);
        resolutionHeight = EditorGUILayout.IntField("������", resolutionHeight);
        savePath = EditorGUILayout.TextField("���� ����������", savePath);

        if (GUILayout.Button("������ ��������"))
        {
            BakeTexture();
        }
    }
    void BakeTexture()
    {
        if (targetObject == null)
        {
            Debug.LogError("������� ������ �� �����.");
            return;
        }
        if (renderCamera == null)
        {
            Debug.LogError("������ ��� ������� �� ������.");
            return;
        }

        // ��������� �������� ���� �������� ������� � ��� ��������
        Dictionary<Transform, int> originalLayers = new Dictionary<Transform, int>();
        SaveOriginalLayers(targetObject.transform, originalLayers);
        SetLayerRecursively(targetObject.transform, bakeLayer);

        // ��������� �������� ��������� ������
        Vector3 originalCamPos = renderCamera.transform.position;
        Quaternion originalCamRot = renderCamera.transform.rotation;
        bool originalOrthographic = renderCamera.orthographic;
        float originalOrthoSize = renderCamera.orthographicSize;
        int originalCullingMask = renderCamera.cullingMask;
        CameraClearFlags originalClearFlags = renderCamera.clearFlags;
        Color originalBG = renderCamera.backgroundColor;

        // ����������� ������ ��� ���������� ������ ������� ����
        renderCamera.cullingMask = 1 << bakeLayer;
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = Color.clear; // ���������� ���
        renderCamera.orthographic = true;

        // ��������� ������� �������� �������
        Bounds bounds = CalculateBounds(targetObject);

        // ����������� ������ ��� top-down ������:
        // ������������ ������ ��������������� �������� � ������� (margin), �������� ������� ������� �� ���� X � Z.
        float margin = 1.1f;
        float orthoSize = Mathf.Max(bounds.extents.z, bounds.extents.x / renderCamera.aspect) * margin;
        renderCamera.orthographicSize = orthoSize;

        // ������������� ������� ������ ����� ��� ��������. �� X � Z � ����� �������, �� Y � ������� ���� ��� ������� �������.
        renderCamera.transform.position = new Vector3(bounds.center.x, bounds.max.y + 10f, bounds.center.z);

        // ������������ ������ ���, ����� ��� �������� ������ ���� (�� ��� Y)
        renderCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // ���� ������ �� �������, ���������� ��� ��������
        bool wasActive = targetObject.activeSelf;
        if (!wasActive)
            targetObject.SetActive(true);

        // ������� ��������� RenderTexture
        RenderTexture rt = new RenderTexture(resolutionWidth, resolutionHeight, 24);
        renderCamera.targetTexture = rt;

        // �������� ����� � ����������� �������
        renderCamera.Render();

        // ��������� ������� �� RenderTexture
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);
        texture.Apply();

        // ��������������� ��������� ������ � RenderTexture
        renderCamera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        // ��������������� �������� ���� ������� � ��� ��������
        RestoreOriginalLayers(originalLayers);
        // ��������������� �������� ��������� ������
        renderCamera.transform.position = originalCamPos;
        renderCamera.transform.rotation = originalCamRot;
        renderCamera.orthographic = originalOrthographic;
        renderCamera.orthographicSize = originalOrthoSize;
        renderCamera.cullingMask = originalCullingMask;
        renderCamera.clearFlags = originalClearFlags;
        renderCamera.backgroundColor = originalBG;

        // ���� ������ ��� ���������, ��������� ��� �������
        if (!wasActive)
            targetObject.SetActive(false);

        // ��������� �������� � ���� PNG
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(savePath, bytes);

        // ��������� ���� ������� ��� ����������� ������ ����� � ���� �������
        AssetDatabase.Refresh();

        Debug.Log("�������� �������� � ��������� �� ����: " + savePath);
    }

    // ���������� ��������� ������� (����������� ��� Renderer-� � ��������)
    Bounds CalculateBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(obj.transform.position, Vector3.zero);
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);
        return bounds;
    }

    // ���������� ��������� �������� ���� ������� � ��� ��������
    void SaveOriginalLayers(Transform obj, Dictionary<Transform, int> dict)
    {
        dict[obj] = obj.gameObject.layer;
        foreach (Transform child in obj)
        {
            SaveOriginalLayers(child, dict);
        }
    }

    // ���������� ������������� �������� ���� ��� ������� � ��� ��������
    void SetLayerRecursively(Transform obj, int layer)
    {
        obj.gameObject.layer = layer;
        foreach (Transform child in obj)
        {
            SetLayerRecursively(child, layer);
        }
    }

    // ��������������� �������� ����
    void RestoreOriginalLayers(Dictionary<Transform, int> dict)
    {
        foreach (var kvp in dict)
        {
            if (kvp.Key != null)
                kvp.Key.gameObject.layer = kvp.Value;
        }
    }
}
