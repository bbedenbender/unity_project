using System.Collections;
using UnityEngine;

public class LightningFXController : MonoBehaviour
{
    [Header("Lightning VFX")]
    public GameObject lightningObject;

    [Header("Flash Light")]
    public Light flashLight;
    public float flashIntensity = 12f;
    public float flashRange = 25f;
    public float flashFadeTime = 0.4f;

    [Header("Timing")]
    public float visibleTime = 0.35f;
    public bool autoStrike = true;
    public float minDelay = 8f;
    public float maxDelay = 18f;

    [Header("Audio")]
    public AudioSource lightningAudio;
    public bool randomizePitch = true;
    public float minPitch = 0.9f;
    public float maxPitch = 1.15f;

    private Coroutine strikeRoutine;

    private void Start()
    {
        if (lightningObject != null)
        {
            lightningObject.SetActive(false);
        }

        if (flashLight != null)
        {
            flashLight.intensity = 0f;
            flashLight.range = flashRange;
        }

        if (autoStrike)
        {
            StartCoroutine(AutoStrikeLoop());
        }
    }

    private IEnumerator AutoStrikeLoop()
    {
        while (true)
        {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            Strike();
        }
    }

    public void Strike()
    {
        if (strikeRoutine != null)
        {
            StopCoroutine(strikeRoutine);
        }

        strikeRoutine = StartCoroutine(StrikeRoutine());
    }

    private IEnumerator StrikeRoutine()
    {
        if (lightningObject != null)
        {
            lightningObject.SetActive(true);
            RestartParticles(lightningObject);
        }

        if (flashLight != null)
        {
            flashLight.range = flashRange;
            flashLight.intensity = flashIntensity;
        }

        if (lightningAudio != null)
        {
            if (randomizePitch)
            {
                lightningAudio.pitch = Random.Range(minPitch, maxPitch);
            }

            lightningAudio.Play();
        }

        yield return new WaitForSeconds(visibleTime);

        if (lightningObject != null)
        {
            lightningObject.SetActive(false);
        }

        float timer = 0f;

        while (timer < flashFadeTime)
        {
            timer += Time.deltaTime;
            float t = timer / flashFadeTime;

            if (flashLight != null)
            {
                flashLight.intensity = Mathf.Lerp(flashIntensity, 0f, t);
            }

            yield return null;
        }

        if (flashLight != null)
        {
            flashLight.intensity = 0f;
        }
    }

    private void RestartParticles(GameObject root)
    {
        ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particle in particles)
        {
            particle.Clear(true);
            particle.Play(true);
        }
    }
}