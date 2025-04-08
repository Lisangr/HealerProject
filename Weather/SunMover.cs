using UnityEngine;

public class SunMover : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the Sun object (e.g., Directional Light).")]
    public Transform sun;

    [Tooltip("Reference to the Moon object (e.g., Directional Light).")]
    public Transform moon;

    [Tooltip("Reference to the character around which the Sun and Moon move.")]
    public Transform character;

    [Header("Orbit Settings")]
    [Tooltip("Period of full revolution (in seconds).")]
    public float rotationPeriod = 60f;

    [Tooltip("Radius of the orbit around the character.")]
    public float radius = 50f;

    [Tooltip("Tilt angle (in degrees) for the orbit around the X axis.")]
    public float tiltAngle = 45f;

    [Header("Light Intensity Settings")]
    [Tooltip("Sun intensity at noon (when timeOfDay = 90�).")]
    public float sunMaxIntensity = 2f;

    [Tooltip("Sun intensity at dawn/dusk (when timeOfDay = 30� or 150�).")]
    public float sunMinIntensity = 0.5f;

    [Tooltip("Moon intensity at its minimum (at timeOfDay = 180� or 360�).")]
    public float moonMinIntensity = 0f;

    [Tooltip("Moon intensity at midnight (when timeOfDay = 270�).")]
    public float moonMaxIntensity = 0.8f;

    private void Start()
    {
        if (character == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                character = playerObj.transform;
            }
            else
            {
                Debug.LogError("character �� ������ � �����! ��������� ��� 'Player'.");
                return;
            }
        }
    }

    void Update()
    {
        // ���������, ��� ������ ����������� � ������ ������ ����.
        if (sun == null || character == null || rotationPeriod <= 0f)
            return;

        // ��������� ������� �����, ����� ��� Time.time = 0 ���� ��� 90� (12:00)
        float phaseOffset = rotationPeriod / 4f;
        float timeOfDay = ((Time.time + phaseOffset) % rotationPeriod) / rotationPeriod * 360f;
        float rad = timeOfDay * Mathf.Deg2Rad;

        // ��������� ������� ������ �� ������.
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * radius;
        offset = Quaternion.AngleAxis(tiltAngle, Vector3.right) * offset;
        sun.position = character.position + offset;
        sun.LookAt(character.position);

        // ��������� ������� ���� �� ��������������� �������.
        if (moon != null)
        {
            float moonAngle = timeOfDay + 180f; // ��������������� ����
            float radMoon = moonAngle * Mathf.Deg2Rad;
            Vector3 offsetMoon = new Vector3(Mathf.Cos(radMoon), 0, Mathf.Sin(radMoon)) * radius;
            offsetMoon = Quaternion.AngleAxis(tiltAngle, Vector3.right) * offsetMoon;
            moon.position = character.position + offsetMoon;
            moon.LookAt(character.position);
        }

        // ��������� ������������� ������.
        Light sunLight = sun.GetComponent<Light>();
        if (sunLight != null)
        {
            float sunIntensity = 0f;
            // ������ ������� �� 0� �� 180�.
            if (timeOfDay >= 0f && timeOfDay <= 180f)
            {
                // ���������� �������, ������� ���� sunMinIntensity ��� 30� � 150� � sunMaxIntensity ��� 90�.
                float a = 2f * sunMinIntensity - sunMaxIntensity; // a = 2*0.5 - 2 = -1 (���� sunMin=0.5, sunMax=2)
                float b = sunMaxIntensity - a;                  // b = 2 - (-1) = 3
                sunIntensity = a + b * Mathf.Sin(timeOfDay * Mathf.Deg2Rad);
                sunIntensity = Mathf.Max(0f, sunIntensity);
            }
            else
            {
                sunIntensity = 0f;
            }
            sunLight.intensity = sunIntensity;
        }

        // ��������� ������������� ����.
        if (moon != null)
        {
            Light moonLight = moon.GetComponent<Light>();
            if (moonLight != null)
            {
                float moonIntensity = 0f;
                // ���� ������� �� 180� �� 360�.
                if (timeOfDay >= 180f && timeOfDay <= 360f)
                {
                    // t = ����� �� 180� �� 360�; ��� t=90 (270�) ������������� �����������.
                    float t = timeOfDay - 180f;
                    moonIntensity = moonMinIntensity + (moonMaxIntensity - moonMinIntensity) * Mathf.Sin(t * Mathf.Deg2Rad);
                    moonIntensity = Mathf.Max(0f, moonIntensity);
                }
                else
                {
                    moonIntensity = 0f;
                }
                moonLight.intensity = moonIntensity;
            }
        }
    }
}
