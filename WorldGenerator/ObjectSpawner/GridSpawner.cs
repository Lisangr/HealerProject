using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GridSpawner : MonoBehaviour
{
    [Header("��������� �����")]
    public float gridSpacing = 2.0f; // ���������� ����� ������� ����� �� ��� X � Z
    public int gridSizeX = 10; // ���������� ����� �� ��� X
    public int gridSizeZ = 10; // ���������� ����� �� ��� Z

    [Header("������� ��� ������")]
    public GameObject[] prefabArray; // ������ ��������
    public float spawnFrequency = 1f; // ������� ������ (� ��������)

    [Header("�����������")]
    public int maxObjectsToSpawn = 100; // ������������ ���������� �������� ��� ������

    // ��������� ��� ���������� ����������
    [Range(0f, 1f)]
    public float spawnChance = 0.5f; // ���� ��������� ������� � ����� �����
    [Range(0f, 1f)]
    public float skipChance = 0.5f; // ����������� �������� �����

    // ������� ���� ��� Ground ��������
    public LayerMask groundLayer;

    // ��������� ����� ��� ������������� ������ �����, ��������, Default
    public LayerMask ignoreLayers;

    private List<Vector3> validSpawnPositions = new List<Vector3>(); // ������ �������, ���� ����� ���������� �������
    private int spawnedObjectsCount = 0; // ������� ������������ ��������
    private Transform parentObject; // ������������ ������ ��� �������� ������������ ��������

    // ���� ����� ����� ���������� ������� � ���������
    public void SpawnPrefabsInEditor()
    {
        // ���������� ������� ������������ �������� ����� ����� ��������
        spawnedObjectsCount = 0;

        // ���������, �� ��������� �� ���������� �������� �� ����� ���������� ��������
        if (spawnedObjectsCount >= maxObjectsToSpawn)
        {
            Debug.LogWarning($"��������� ������������ ���������� �������� ��� ������! ��� ���������� {spawnedObjectsCount} �� {maxObjectsToSpawn}.");
            return;
        }

        validSpawnPositions.Clear(); // ������� ������ �������� ������� ����� ����� ��������

        // ������� ������������ ������ ��� �������� ������������ ��������
        if (parentObject == null)
        {
            parentObject = new GameObject("SpawnedObjects").transform;
        }
        else
        {
            // ������� ������������ ������, ���� �� ��� ����������
            foreach (Transform child in parentObject)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // �������� ��������� ������� ��� ����� �� ������� �������, �� ������� ����� ������
        Vector3 startPosition = transform.position;

        // ������ ����� � ������� �����
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                // ���������� ����� ��� ������ � ������ ������� �������
                Vector3 spawnCheckPosition = new Vector3(startPosition.x + x * gridSpacing, 1000f, startPosition.z + z * gridSpacing);

                // ������� ��� ���� �� ������ �������
                RaycastHit hit;
                if (Physics.Raycast(spawnCheckPosition, Vector3.down, out hit, Mathf.Infinity, groundLayer))
                {
                    // ���������, ��� ��� �� ���������� ������ ����, ��������� � ignoreLayers
                    if (((1 << hit.collider.gameObject.layer) & ignoreLayers) != 0)
                    {
                        continue;
                    }

                    // ���� ��� ���������� �����, ��������� ����� � ������
                    validSpawnPositions.Add(hit.point);
                    Debug.Log($"��� ������ ������ �� ������ Y: {hit.point.y} � ������� {hit.point}");
                }
            }
        }

        // ������ ������� ������� � ���� �������� � ������ ��������� � �����������
        foreach (Vector3 position in validSpawnPositions)
        {
            if (spawnedObjectsCount >= maxObjectsToSpawn)
            {
                Debug.LogWarning($"��������� ������������ ���������� �������� ��� ������! ��� ���������� {spawnedObjectsCount} �� {maxObjectsToSpawn}.");
                break;
            }

            // ���������� ����� � ������������ skipChance
            if (Random.value <= skipChance)
            {
                continue;
            }

            // ������� ������� � ������������ spawnChance
            if (Random.value <= spawnChance)
            {
                // ������ ��������� �������� ��� X � Z, ����� ������������� ������������ �������� � ���� �����
                float randomOffsetX = Random.Range(-gridSpacing / 2f, gridSpacing / 2f);
                float randomOffsetZ = Random.Range(-gridSpacing / 2f, gridSpacing / 2f);

                // ������������ ������� � ������ ��������
                Vector3 adjustedPosition = new Vector3(position.x + randomOffsetX, position.y, position.z + randomOffsetZ);

                // �������� ��������� ������
                GameObject prefabToSpawn = prefabArray[Random.Range(0, prefabArray.Length)];

#if UNITY_EDITOR
            // ������������ ������ � ����� � ������������� ��� ���������
            GameObject spawnedObject = PrefabUtility.InstantiatePrefab(prefabToSpawn, parentObject) as GameObject;
            if (spawnedObject != null)
            {
                spawnedObject.transform.position = adjustedPosition;
                spawnedObject.transform.rotation = Quaternion.identity;
            }
#endif

                // ����������� ������� ������������ ��������
                spawnedObjectsCount++;
            }
        }
    }


    // ������ ����� ��� ������������ � ���������
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        // �������� ��������� ������� ��� ����� �� ������� �������, �� ������� ����� ������
        Vector3 startPosition = transform.position;

        // ������ �����, ��������� ���������� �������� �������
        foreach (Vector3 position in validSpawnPositions)
        {
            Gizmos.DrawWireSphere(position, 0.1f); // ������ ��������� ������ � ������ �����
        }
    }
}





































/*using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GridSpawner : MonoBehaviour
{
    [Header("��������� �����")]
    public float gridSpacing = 2.0f; // ���������� ����� ������� ����� �� ��� X � Z
    public int gridSizeX = 10; // ���������� ����� �� ��� X
    public int gridSizeZ = 10; // ���������� ����� �� ��� Z

    [Header("������� ��� ������")]
    public GameObject[] prefabArray; // ������ ��������
    public float spawnFrequency = 1f; // ������� ������ (� ��������)

    [Header("�����������")]
    public int maxObjectsToSpawn = 100; // ������������ ���������� �������� ��� ������

    // ��������� ��� ���������� ����������
    [Range(0f, 1f)]
    public float spawnChance = 0.5f; // ���� ��������� ������� � ����� �����
    [Range(0f, 1f)]
    public float skipChance = 0.5f; // ����������� �������� �����

    // ������� ���� ��� Ground ��������
    public LayerMask groundLayer;

    // ��������� ����� ��� ������������� ������ �����, ��������, Default
    public LayerMask ignoreLayers;

    private List<Vector3> validSpawnPositions = new List<Vector3>(); // ������ �������, ���� ����� ���������� �������
    private int spawnedObjectsCount = 0; // ������� ������������ ��������
    private Transform parentObject; // ������������ ������ ��� �������� ������������ ��������

    // ���� ����� ����� ���������� ������� � ���������
    public void SpawnPrefabsInEditor()
    {
        // ���������� ������� ������������ �������� ����� ����� ��������
        spawnedObjectsCount = 0;

        // ���������, �� ��������� �� ���������� �������� �� ����� ���������� ��������
        if (spawnedObjectsCount >= maxObjectsToSpawn)
        {
            Debug.LogWarning($"��������� ������������ ���������� �������� ��� ������! ��� ���������� {spawnedObjectsCount} �� {maxObjectsToSpawn}.");
            return;
        }

        validSpawnPositions.Clear(); // ������� ������ �������� ������� ����� ����� ��������

        // ������� ������������ ������ ��� �������� ������������ ��������
        if (parentObject == null)
        {
            parentObject = new GameObject("SpawnedObjects").transform;
        }
        else
        {
            // ������� ������������ ������, ���� �� ��� ����������
            foreach (Transform child in parentObject)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // �������� ��������� ������� ��� ����� �� ������� �������, �� ������� ����� ������
        Vector3 startPosition = transform.position;

        // ������ ����� � ������� �����
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                // ���������� ����� ��� ������ � ������ ������� �������
                Vector3 spawnCheckPosition = new Vector3(startPosition.x + x * gridSpacing, 1000f, startPosition.z + z * gridSpacing);

                // ������� ��� ���� �� ������ �������
                RaycastHit hit;
                if (Physics.Raycast(spawnCheckPosition, Vector3.down, out hit, Mathf.Infinity, groundLayer))
                {
                    // ���������, ��� ��� �� ���������� ������ ����, ��������� � ignoreLayers
                    if (((1 << hit.collider.gameObject.layer) & ignoreLayers) != 0)
                    {
                        // ���� ����������, ���������� ���� ����
                        continue;
                    }

                    // ���� ��� ���������� �����, ��������� ����� � ������
                    validSpawnPositions.Add(hit.point);
                    Debug.Log($"��� ������ ������ �� ������ Y: {hit.point.y} � ������� {hit.point}");
                }
            }
        }

        // ������ ������� ������� � ���� �������� � ������ ��������� � �����������
        foreach (Vector3 position in validSpawnPositions)
        {
            if (spawnedObjectsCount >= maxObjectsToSpawn)
            {
                Debug.LogWarning($"��������� ������������ ���������� �������� ��� ������! ��� ���������� {spawnedObjectsCount} �� {maxObjectsToSpawn}.");
                break;
            }

            // ���������� ����� � ������������ skipChance
            if (Random.value <= skipChance)
            {
                continue;
            }

            // ������� ������� � ������������ spawnChance
            if (Random.value <= spawnChance)
            {
                // ������ ��������� �������� ��� X � Z, ����� ������������� ������������ �������� � ���� �����
                float randomOffsetX = Random.Range(-gridSpacing / 2f, gridSpacing / 2f);
                float randomOffsetZ = Random.Range(-gridSpacing / 2f, gridSpacing / 2f);

                // ������������ ������� � ������ ��������
                Vector3 adjustedPosition = new Vector3(position.x + randomOffsetX, position.y, position.z + randomOffsetZ);

                // �������� ��������� ������
                GameObject prefabToSpawn = prefabArray[Random.Range(0, prefabArray.Length)];

                // ������� ������ � ������������� ��� ���������
                GameObject spawnedObject = Instantiate(prefabToSpawn, adjustedPosition, Quaternion.identity);
                spawnedObject.transform.SetParent(parentObject); // ������������� ������������ ������ ��� ����������

                // ����������� ������� ������������ ��������
                spawnedObjectsCount++;
            }
        }
    }

    // ������ ����� ��� ������������ � ���������
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        // �������� ��������� ������� ��� ����� �� ������� �������, �� ������� ����� ������
        Vector3 startPosition = transform.position;

        // ������ �����, ��������� ���������� �������� �������
        foreach (Vector3 position in validSpawnPositions)
        {
            Gizmos.DrawWireSphere(position, 0.1f); // ������ ��������� ������ � ������ �����
        }
    }
}*/