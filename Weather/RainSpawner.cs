using UnityEngine;
using System.Collections;

public class RainSpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("GameObject with the rain particle system to be toggled on/off.")]
    public GameObject rainEffect;

    [Tooltip("AudioSource used for playing rain sound effects.")]
    public AudioSource rainAudioSource;

    [Tooltip("Array of audio clips that will be played cyclically during rain.")]
    public AudioClip[] rainClips;

    [Header("Timing Settings")]
    [Tooltip("Interval between rain activations (in seconds).")]
    public float spawnInterval = 480f;

    [Tooltip("Minimum duration for which the rain will be active (in seconds).")]
    public float minRainDuration = 120f;

    [Tooltip("Maximum duration for which the rain will be active (in seconds).")]
    public float maxRainDuration = 240f;

    [Header("Particle Settings")]
    [Tooltip("Maximum 'Rate over Time' value for the rain particle system.")]
    public float maxEmission = 1500f;

    [Tooltip("If enabled, a random maxEmission value will be chosen each cycle.")]
    public bool randomizeMaxEmission = false;

    [Tooltip("Minimum random maxEmission value (used if randomizeMaxEmission is true).")]
    public float minMaxEmission = 1500f;

    [Tooltip("Maximum random maxEmission value (used if randomizeMaxEmission is true).")]
    public float maxMaxEmission = 2500f;

    // Reference to the audio coroutine so it can be stopped.
    private Coroutine audioCoroutine;

    void OnEnable()
    {
        if (rainEffect != null)
        {
            // Ensure the rain effect is initially turned off.
            rainEffect.SetActive(false);

            if (rainAudioSource == null)
            {
                Debug.LogError("AudioSource for rain (rainAudioSource) is not assigned!");
            }

            StartCoroutine(ToggleRainRoutine());
        }
        else
        {
            Debug.LogError("Rain effect GameObject (rainEffect) is not assigned!");
        }
    }

    IEnumerator ToggleRainRoutine()
    {
        while (true)
        {
            // Wait for the specified interval before activating rain.
            yield return new WaitForSeconds(spawnInterval);

            // If randomization is enabled, choose a random maxEmission value.
            if (randomizeMaxEmission)
            {
                maxEmission = Random.Range(minMaxEmission, maxMaxEmission);
            }

            // Determine a random duration for the rain within the specified range.
            float currentRainDuration = Random.Range(minRainDuration, maxRainDuration);

            // Activate the rain effect.
            rainEffect.SetActive(true);

            // Start the coroutine to adjust the particle emission rate over the rain duration.
            StartCoroutine(AdjustEmissionRate(currentRainDuration));

            // If the AudioSource and audio clips are set, start playing rain audio cyclically.
            if (rainAudioSource != null && rainClips != null && rainClips.Length > 0)
            {
                audioCoroutine = StartCoroutine(PlayRainAudio());
            }

            // Wait for the current rain duration.
            yield return new WaitForSeconds(currentRainDuration);

            // Deactivate the rain effect.
            rainEffect.SetActive(false);

            // Stop the audio coroutine if it's running.
            if (audioCoroutine != null)
            {
                StopCoroutine(audioCoroutine);
                audioCoroutine = null;
            }
            if (rainAudioSource != null)
            {
                rainAudioSource.Stop();
            }
        }
    }

    // Coroutine to smoothly adjust the particle system's emission rate:
    // increases from 0 to maxEmission, then decreases back to 0.
    IEnumerator AdjustEmissionRate(float duration)
    {
        ParticleSystem ps = rainEffect.GetComponent<ParticleSystem>();
        if (ps == null)
            yield break;

        ParticleSystem.EmissionModule emission = ps.emission;
        float half = duration / 2f;
        float t = 0f;
        while (t < duration)
        {
            float currentRate;
            if (t <= half)
            {
                // First half: ramp up from 0 to maxEmission.
                currentRate = Mathf.Lerp(0, maxEmission, t / half);
            }
            else
            {
                // Second half: ramp down from maxEmission to 0.
                currentRate = Mathf.Lerp(maxEmission, 0, (t - half) / half);
            }
            emission.rateOverTime = currentRate;
            t += Time.deltaTime;
            yield return null;
        }
        // Ensure emission is set to 0 at the end.
        emission.rateOverTime = 0f;
    }

    // Coroutine to cyclically play rain audio clips from the provided array.
    IEnumerator PlayRainAudio()
    {
        int index = 0;
        while (true)
        {
            if (rainClips[index] != null)
            {
                rainAudioSource.clip = rainClips[index];
                rainAudioSource.Play();
                yield return new WaitForSeconds(rainClips[index].length);
            }
            else
            {
                yield return null;
            }
            index = (index + 1) % rainClips.Length;
        }
    }
    // При отключении компонента гарантируем, что все корутины остановятся и эффект дождя выключится.
    void OnDisable()
    {
        StopAllCoroutines();
        if (rainEffect != null)
            rainEffect.SetActive(false);
        if (rainAudioSource != null)
            rainAudioSource.Stop();
    }
}
