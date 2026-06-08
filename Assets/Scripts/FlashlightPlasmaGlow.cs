using System.Collections;
using UnityEngine;

public class FlashlightPlasmaGlow : MonoBehaviour
{
    [Header("VFX")]
    public GameObject plasmaVFX;
    public Light plasmaLight;

    [Header("Light Settings")]
    public float targetIntensity = 2.5f;
    public float targetRange = 3f;
    public float fadeInTime = 0.75f;
    public float pulseAmount = 0.7f;
    public float pulseSpeed = 4f;

    private bool isActive = false;
    private Coroutine fadeRoutine;

    private void Start()
    {
        if (plasmaVFX != null)
        {
            plasmaVFX.SetActive(false);
        }

        if (plasmaLight != null)
        {
            plasmaLight.intensity = 0f;
            plasmaLight.range = targetRange;
        }
    }

    private void Update()
    {
        if (!isActive || plasmaLight == null)
        {
            return;
        }

        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        plasmaLight.intensity = targetIntensity + pulse * pulseAmount;
    }

    public void ActivateGlow()
    {
        isActive = true;

        if (plasmaVFX != null)
        {
            plasmaVFX.SetActive(true);
            RestartParticles(plasmaVFX);
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeLightIn());
    }

    public void DeactivateGlow()
    {
        isActive = false;

        if (plasmaVFX != null)
        {
            plasmaVFX.SetActive(false);
        }

        if (plasmaLight != null)
        {
            plasmaLight.intensity = 0f;
        }
    }

    private IEnumerator FadeLightIn()
    {
        if (plasmaLight == null)
        {
            yield break;
        }

        float timer = 0f;

        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            float t = timer / fadeInTime;

            plasmaLight.intensity = Mathf.Lerp(0f, targetIntensity, t);
            plasmaLight.range = Mathf.Lerp(0f, targetRange, t);

            yield return null;
        }

        plasmaLight.intensity = targetIntensity;
        plasmaLight.range = targetRange;
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