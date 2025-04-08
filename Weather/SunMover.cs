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
    [Tooltip("Sun intensity at noon (when timeOfDay = 90°).")]
    public float sunMaxIntensity = 2f;

    [Tooltip("Sun intensity at dawn/dusk (when timeOfDay = 30° or 150°).")]
    public float sunMinIntensity = 0.5f;

    [Tooltip("Moon intensity at its minimum (at timeOfDay = 180° or 360°).")]
    public float moonMinIntensity = 0f;

    [Tooltip("Moon intensity at midnight (when timeOfDay = 270°).")]
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
                Debug.LogError("character не найден в сцене! Проверьте тег 'Player'.");
                return;
            }
        }
    }

    void Update()
    {
        // Проверяем, что ссылки установлены и период больше нуля.
        if (sun == null || character == null || rotationPeriod <= 0f)
            return;

        // Добавляем фазовый сдвиг, чтобы при Time.time = 0 угол был 90° (12:00)
        float phaseOffset = rotationPeriod / 4f;
        float timeOfDay = ((Time.time + phaseOffset) % rotationPeriod) / rotationPeriod * 360f;
        float rad = timeOfDay * Mathf.Deg2Rad;

        // Вычисляем позицию солнца по орбите.
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * radius;
        offset = Quaternion.AngleAxis(tiltAngle, Vector3.right) * offset;
        sun.position = character.position + offset;
        sun.LookAt(character.position);

        // Вычисляем позицию луны на противоположной стороне.
        if (moon != null)
        {
            float moonAngle = timeOfDay + 180f; // противоположный угол
            float radMoon = moonAngle * Mathf.Deg2Rad;
            Vector3 offsetMoon = new Vector3(Mathf.Cos(radMoon), 0, Mathf.Sin(radMoon)) * radius;
            offsetMoon = Quaternion.AngleAxis(tiltAngle, Vector3.right) * offsetMoon;
            moon.position = character.position + offsetMoon;
            moon.LookAt(character.position);
        }

        // Обновляем интенсивность солнца.
        Light sunLight = sun.GetComponent<Light>();
        if (sunLight != null)
        {
            float sunIntensity = 0f;
            // Солнце активно от 0° до 180°.
            if (timeOfDay >= 0f && timeOfDay <= 180f)
            {
                // Используем функцию, которая дает sunMinIntensity при 30° и 150° и sunMaxIntensity при 90°.
                float a = 2f * sunMinIntensity - sunMaxIntensity; // a = 2*0.5 - 2 = -1 (если sunMin=0.5, sunMax=2)
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

        // Обновляем интенсивность луны.
        if (moon != null)
        {
            Light moonLight = moon.GetComponent<Light>();
            if (moonLight != null)
            {
                float moonIntensity = 0f;
                // Луна активна от 180° до 360°.
                if (timeOfDay >= 180f && timeOfDay <= 360f)
                {
                    // t = время от 180° до 360°; при t=90 (270°) интенсивность максимальна.
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
