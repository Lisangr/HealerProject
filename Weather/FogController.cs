using UnityEngine;

public class FogController : MonoBehaviour
{
    [Header("Fog Animation Settings")]
    [Tooltip("Duration of the density change cycle when fog is active (from 0 to max and back to 0).")]
    public float cycleDuration = 480f;

    [Tooltip("Minimum emission rate for fog particles.")]
    public float minEmissionRate = 0f;

    [Tooltip("Maximum emission rate for fog particles.")]
    public float maxEmissionRate = 70f;

    [Header("Fog Cycle Timing")]
    [Tooltip("Overall duration of the fog on/off cycle (e.g., 300 seconds for 5 minutes).")]
    public float fogCycle = 300f;

    [Tooltip("Time (in seconds) when fog starts to activate (e.g., 120 seconds for 2 minutes).")]
    public float fogActiveStart = 120f;

    [Tooltip("Time (in seconds) when fog deactivates (e.g., 240 seconds for 4 minutes).")]
    public float fogActiveEnd = 240f;

    [Header("Fog Prefab")]
    [Tooltip("Prefab � VFX ������ (������� ������), ������� ����� �������������� ������ ������������ ������.")]
    public GameObject fogPrefab;

    private GameObject fogInstance;
    private ParticleSystem fogParticleSystem;

    void Start()
    {
        // ��������� ����������� ����� Unity.
        RenderSettings.fog = false;
    }

    void Update()
    {
        if (cycleDuration <= 0f || fogCycle <= 0f)
            return;

        // ��������� ����� � ����� ������.
        float currentCycleTime = Time.time % fogCycle;

        if (currentCycleTime >= fogActiveStart && currentCycleTime <= fogActiveEnd)
        {
            // ���� ����� ��� �� ������, ������������� ������.
            if (fogInstance == null && fogPrefab != null)
            {
                // ����� ����� ������ ������ �������, ��������, ����� ����� ��� ��������� � ������.
                fogInstance = Instantiate(fogPrefab, transform.position, Quaternion.identity);
                fogParticleSystem = fogInstance.GetComponent<ParticleSystem>();
                if (fogParticleSystem == null)
                {
                    Debug.LogError("������ ������ �� �������� ���������� ParticleSystem!");
                }
            }

            // ���� ������� ������ �������, �������� � ������������� �������.
            if (fogParticleSystem != null)
            {
                // ������������ �������� ��� �������� ��������� ������� (0 -> max -> 0)
                float halfCycle = cycleDuration / 2f;
                float pingPong = Mathf.PingPong(Time.time, halfCycle);
                float normalized = pingPong / halfCycle;
                float currentEmissionRate = Mathf.Lerp(minEmissionRate, maxEmissionRate, normalized);

                var emission = fogParticleSystem.emission;
                emission.rateOverTime = currentEmissionRate;
            }
        }
        else
        {
            // ��� ��������� ��������� ������ � ���������/���������� ���������.
            if (fogInstance != null)
            {
                Destroy(fogInstance);
                fogInstance = null;
                fogParticleSystem = null;
            }
        }
    }

    void OnDisable()
    {
        // ��� ���������� ���������� ������� �����, ����� �� �� ������� �� �����.
        if (fogInstance != null)
        {
            Destroy(fogInstance);
        }
    }
}
