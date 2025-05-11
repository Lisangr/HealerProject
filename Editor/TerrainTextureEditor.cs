using UnityEngine;
using UnityEditor;

/// <summary>
/// ��� ����-�������� ��������� ������������� ��������� �������� �� ������� � ����������� �� ������ � ������.
/// �������� �������� �� ������� ����:
///   - ���� 1 (������ 0): �����
///   - ���� 2 (������ 1): �����
///   - ���� 3 (������ 2): ������
///   - ���� 4 (������ 3): ����
/// ������ ����� ��������, ��������� ��������� ��������.
/// </summary>
public class TerrainTextureEditor : EditorWindow
{
    // ������ �� ������ Terrain, ��������� � ���� ���������.
    Terrain terrain;

    // ���������� ������, �� ����� �������������� ����� ���� ���������.
    [Header("��������� ��������")]
    [Tooltip("������, ������� � ������� �������� ���� (� ������)")]
    float snowHeightThreshold = 100f;

    [Tooltip("������������ ����� ��� ���������� �������� ����� (� ��������)")]
    float snowSlopeThreshold = 30f;

    [Tooltip("�����, ��� ���������� �������� ����������� �������� ����� (� ��������)")]
    float stoneSlopeThreshold = 30f;

    [Tooltip("������������ ������ ��� ���������� �������� ����� (� ������)")]
    float grassHeightThreshold = 30f;

    [MenuItem("Tools/Terrain Texture Editor")]
    public static void ShowWindow()
    {
        GetWindow(typeof(TerrainTextureEditor), false, "Terrain Texture");
    }

    void OnGUI()
    {
        GUILayout.Label("��������� ��������������� ��������������� ��������", EditorStyles.boldLabel);

        // ����� ������� Terrain
        terrain = (Terrain)EditorGUILayout.ObjectField("�������", terrain, typeof(Terrain), true);

        // ��������� �������
        snowHeightThreshold = EditorGUILayout.FloatField("����� ������ ��� �����", snowHeightThreshold);
        snowSlopeThreshold = EditorGUILayout.FloatField("����� ������ ��� �����", snowSlopeThreshold);
        stoneSlopeThreshold = EditorGUILayout.FloatField("����� ������ ��� �����", stoneSlopeThreshold);
        grassHeightThreshold = EditorGUILayout.FloatField("����� ������ ��� �����", grassHeightThreshold);

        if (GUILayout.Button("��������� ��������"))
        {
            if (terrain == null)
            {
                Debug.LogError("������� �� ������!");
                return;
            }
            ApplyTextures(terrain.terrainData);
        }
    }

    /// <summary>
    /// ������� �������� �����-����� �������� ��� ���������� ������� �� ������ ������ � ������.
    /// </summary>
    /// <param name="terrainData">������ ��������</param>
    void ApplyTextures(TerrainData terrainData)
    {
        int alphamapWidth = terrainData.alphamapWidth;
        int alphamapHeight = terrainData.alphamapHeight;

        int numTextures = 4; // ����� 4 ���������� ���� (�����, �����, ������, ����)

        // ������ �����-����: ����������� [������, ������, ����� ����]
        float[,,] splatmapData = new float[alphamapHeight, alphamapWidth, numTextures];

        // �������� �� ������� "�������" ����� �����
        for (int y = 0; y < alphamapHeight; y++)
        {
            for (int x = 0; x < alphamapWidth; x++)
            {
                // ��������������� ���������� � ��������� [0; 1]
                float normX = (float)x / (alphamapWidth - 1);
                float normY = (float)y / (alphamapHeight - 1);

                // �������� ������ (� ������) � ����� (� ��������) ��� �����
                float height = terrainData.GetInterpolatedHeight(normX, normY);
                float slope = terrainData.GetSteepness(normX, normY);

                // �������� ���� ��� ���� ����
                float[] splat = new float[numTextures];

                // ������ ���������� ��������:
                // 1. ���� ������ ��������� ����� ����� � ����� �� ��������� ������������ ��� �����,
                //    �� ��������� �������� ����� (���� 4, ������ 3).
                if (height >= snowHeightThreshold && slope <= snowSlopeThreshold)
                {
                    splat[3] = 1f; // ����
                }
                // 2. ���� ����� ��������� ����� ��� ����� (���������� �� ������),
                //    �� ����������� �������� ����� (���� 3, ������ 2).
                else if (slope > stoneSlopeThreshold)
                {
                    splat[2] = 1f; // ������
                }
                // 3. � ��������� ������� �������� ����� �� ������:
                //    - ���� ������ ���� ������ ��� �����, ��������� �������� ����� (���� 1, ������ 0).
                //    - ����� � ��������� �������� ����� (���� 2, ������ 1).
                else
                {
                    if (height < grassHeightThreshold)
                    {
                        splat[0] = 1f; // �����
                    }
                    else
                    {
                        splat[1] = 1f; // �����
                    }
                }

                // ��������� ������ �����-����� ��� ������� �����.
                for (int i = 0; i < numTextures; i++)
                {
                    splatmapData[y, x, i] = splat[i];
                }
            }
        }

        // ��������� ����������� �����-����� � ��������.
        terrainData.SetAlphamaps(0, 0, splatmapData);
        Debug.Log("��������������� �������� ���������.");
    }
}
