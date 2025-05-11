using UnityEngine;
using UnityEditor;

/// <summary>
/// Это окно-редактор позволяет автоматически назначать текстуры на террейн в зависимости от высоты и уклона.
/// Обратите внимание на порядок слоёв:
///   - Слой 1 (индекс 0): трава
///   - Слой 2 (индекс 1): земля
///   - Слой 3 (индекс 2): камень
///   - Слой 4 (индекс 3): снег
/// Логику можно изменить, подстроив пороговые значения.
/// </summary>
public class TerrainTextureEditor : EditorWindow
{
    // Ссылка на объект Terrain, выбранный в окне редактора.
    Terrain terrain;

    // Задаваемые пороги, их можно корректировать через окно редактора.
    [Header("Пороговые значения")]
    [Tooltip("Высота, начиная с которой возможен снег (в метрах)")]
    float snowHeightThreshold = 100f;

    [Tooltip("Максимальный уклон для применения текстуры снега (в градусах)")]
    float snowSlopeThreshold = 30f;

    [Tooltip("Уклон, при превышении которого назначается текстура камня (в градусах)")]
    float stoneSlopeThreshold = 30f;

    [Tooltip("Максимальная высота для назначения текстуры травы (в метрах)")]
    float grassHeightThreshold = 30f;

    [MenuItem("Tools/Terrain Texture Editor")]
    public static void ShowWindow()
    {
        GetWindow(typeof(TerrainTextureEditor), false, "Terrain Texture");
    }

    void OnGUI()
    {
        GUILayout.Label("Настройки автоматического текстурирования террейна", EditorStyles.boldLabel);

        // Выбор объекта Terrain
        terrain = (Terrain)EditorGUILayout.ObjectField("Террейн", terrain, typeof(Terrain), true);

        // Настройки порогов
        snowHeightThreshold = EditorGUILayout.FloatField("Порог высоты для снега", snowHeightThreshold);
        snowSlopeThreshold = EditorGUILayout.FloatField("Порог уклона для снега", snowSlopeThreshold);
        stoneSlopeThreshold = EditorGUILayout.FloatField("Порог уклона для камня", stoneSlopeThreshold);
        grassHeightThreshold = EditorGUILayout.FloatField("Порог высоты для травы", grassHeightThreshold);

        if (GUILayout.Button("Применить текстуры"))
        {
            if (terrain == null)
            {
                Debug.LogError("Террейн не выбран!");
                return;
            }
            ApplyTextures(terrain.terrainData);
        }
    }

    /// <summary>
    /// Функция перебора альфа-карты террейна для назначения текстур на основе высоты и уклона.
    /// </summary>
    /// <param name="terrainData">Данные террейна</param>
    void ApplyTextures(TerrainData terrainData)
    {
        int alphamapWidth = terrainData.alphamapWidth;
        int alphamapHeight = terrainData.alphamapHeight;

        int numTextures = 4; // Всего 4 текстурных слоя (трава, земля, камень, снег)

        // Массив альфа-карт: размерность [высота, ширина, номер слоя]
        float[,,] splatmapData = new float[alphamapHeight, alphamapWidth, numTextures];

        // Проходим по каждому "пикселю" альфа карты
        for (int y = 0; y < alphamapHeight; y++)
        {
            for (int x = 0; x < alphamapWidth; x++)
            {
                // Нормализованные координаты в диапазоне [0; 1]
                float normX = (float)x / (alphamapWidth - 1);
                float normY = (float)y / (alphamapHeight - 1);

                // Получаем высоту (в метрах) и уклон (в градусах) для точки
                float height = terrainData.GetInterpolatedHeight(normX, normY);
                float slope = terrainData.GetSteepness(normX, normY);

                // Обнуляем веса для всех слоёв
                float[] splat = new float[numTextures];

                // Логика назначения текстуры:
                // 1. Если высота превышает порог снега и уклон не превышает максимальный для снега,
                //    то применяем текстуру снега (слой 4, индекс 3).
                if (height >= snowHeightThreshold && slope <= snowSlopeThreshold)
                {
                    splat[3] = 1f; // снег
                }
                // 2. Если уклон превышает порог для камня (независимо от высоты),
                //    то назначается текстура камня (слой 3, индекс 2).
                else if (slope > stoneSlopeThreshold)
                {
                    splat[2] = 1f; // камень
                }
                // 3. В остальных случаях базируем выбор на высоте:
                //    - Если высота ниже порога для травы, применяем текстуру травы (слой 1, индекс 0).
                //    - Иначе — применяем текстуру земли (слой 2, индекс 1).
                else
                {
                    if (height < grassHeightThreshold)
                    {
                        splat[0] = 1f; // трава
                    }
                    else
                    {
                        splat[1] = 1f; // земля
                    }
                }

                // Заполняем массив альфа-карты для текущей точки.
                for (int i = 0; i < numTextures; i++)
                {
                    splatmapData[y, x, i] = splat[i];
                }
            }
        }

        // Применяем вычисленную альфа-карту к террейну.
        terrainData.SetAlphamaps(0, 0, splatmapData);
        Debug.Log("Текстурирование террейна выполнено.");
    }
}
