using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class TextureBaker : EditorWindow
{
    // Объект, внешний вид которого нужно запечь
    GameObject targetObject;
    // Камера, с которой будет производиться рендеринг
    Camera renderCamera;
    // Разрешение итогового изображения
    int resolutionWidth = 1024;
    int resolutionHeight = 1024;
    // Путь для сохранения файла
    string savePath = "Assets/BakedTexture.png";
    // Индекс временного слоя (убедитесь, что этот слой не используется в сцене)
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
        GUILayout.Label("Запекание текстуры", EditorStyles.boldLabel);

        targetObject = (GameObject)EditorGUILayout.ObjectField("Целевой объект", targetObject, typeof(GameObject), true);
        renderCamera = (Camera)EditorGUILayout.ObjectField("Камера для рендера", renderCamera, typeof(Camera), true);
        resolutionWidth = EditorGUILayout.IntField("Ширина", resolutionWidth);
        resolutionHeight = EditorGUILayout.IntField("Высота", resolutionHeight);
        savePath = EditorGUILayout.TextField("Путь сохранения", savePath);

        if (GUILayout.Button("Запечь текстуру"))
        {
            BakeTexture();
        }
    }
    void BakeTexture()
    {
        if (targetObject == null)
        {
            Debug.LogError("Целевой объект не задан.");
            return;
        }
        if (renderCamera == null)
        {
            Debug.LogError("Камера для рендера не задана.");
            return;
        }

        // Сохраняем исходные слои целевого объекта и его потомков
        Dictionary<Transform, int> originalLayers = new Dictionary<Transform, int>();
        SaveOriginalLayers(targetObject.transform, originalLayers);
        SetLayerRecursively(targetObject.transform, bakeLayer);

        // Сохраняем исходные настройки камеры
        Vector3 originalCamPos = renderCamera.transform.position;
        Quaternion originalCamRot = renderCamera.transform.rotation;
        bool originalOrthographic = renderCamera.orthographic;
        float originalOrthoSize = renderCamera.orthographicSize;
        int originalCullingMask = renderCamera.cullingMask;
        CameraClearFlags originalClearFlags = renderCamera.clearFlags;
        Color originalBG = renderCamera.backgroundColor;

        // Настраиваем камеру для рендеринга только нужного слоя
        renderCamera.cullingMask = 1 << bakeLayer;
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = Color.clear; // прозрачный фон
        renderCamera.orthographic = true;

        // Вычисляем границы целевого объекта
        Bounds bounds = CalculateBounds(targetObject);

        // Настраиваем камеру для top-down обзора:
        // Рассчитываем размер орфографической проекции с запасом (margin), учитывая размеры объекта по осям X и Z.
        float margin = 1.1f;
        float orthoSize = Mathf.Max(bounds.extents.z, bounds.extents.x / renderCamera.aspect) * margin;
        renderCamera.orthographicSize = orthoSize;

        // Устанавливаем позицию камеры прямо над объектом. По X и Z — центр объекта, по Y — немного выше его верхней границы.
        renderCamera.transform.position = new Vector3(bounds.center.x, bounds.max.y + 10f, bounds.center.z);

        // Поворачиваем камеру так, чтобы она смотрела строго вниз (на ось Y)
        renderCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Если объект не активен, активируем его временно
        bool wasActive = targetObject.activeSelf;
        if (!wasActive)
            targetObject.SetActive(true);

        // Создаем временный RenderTexture
        RenderTexture rt = new RenderTexture(resolutionWidth, resolutionHeight, 24);
        renderCamera.targetTexture = rt;

        // Рендерим сцену с настроенной камерой
        renderCamera.Render();

        // Считываем пиксели из RenderTexture
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);
        texture.Apply();

        // Восстанавливаем настройки камеры и RenderTexture
        renderCamera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        // Восстанавливаем исходные слои объекта и его потомков
        RestoreOriginalLayers(originalLayers);
        // Восстанавливаем исходные настройки камеры
        renderCamera.transform.position = originalCamPos;
        renderCamera.transform.rotation = originalCamRot;
        renderCamera.orthographic = originalOrthographic;
        renderCamera.orthographicSize = originalOrthoSize;
        renderCamera.cullingMask = originalCullingMask;
        renderCamera.clearFlags = originalClearFlags;
        renderCamera.backgroundColor = originalBG;

        // Если объект был неактивен, отключаем его обратно
        if (!wasActive)
            targetObject.SetActive(false);

        // Сохраняем текстуру в файл PNG
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(savePath, bytes);

        // Обновляем базу ассетов для отображения нового файла в окне проекта
        AssetDatabase.Refresh();

        Debug.Log("Текстура запечена и сохранена по пути: " + savePath);
    }

    // Вычисление габаритов объекта (учитываются все Renderer-ы в потомках)
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

    // Рекурсивно сохраняем исходные слои объекта и его потомков
    void SaveOriginalLayers(Transform obj, Dictionary<Transform, int> dict)
    {
        dict[obj] = obj.gameObject.layer;
        foreach (Transform child in obj)
        {
            SaveOriginalLayers(child, dict);
        }
    }

    // Рекурсивно устанавливаем заданный слой для объекта и его потомков
    void SetLayerRecursively(Transform obj, int layer)
    {
        obj.gameObject.layer = layer;
        foreach (Transform child in obj)
        {
            SetLayerRecursively(child, layer);
        }
    }

    // Восстанавливаем исходные слои
    void RestoreOriginalLayers(Dictionary<Transform, int> dict)
    {
        foreach (var kvp in dict)
        {
            if (kvp.Key != null)
                kvp.Key.gameObject.layer = kvp.Value;
        }
    }
}
