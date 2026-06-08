using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class OpeningGameSequence : MonoBehaviour
{
    [Header("Player")]
    public Transform playerRoot;
    public PlayerController playerController;
    public CameraFollow cameraFollow;

    [Header("Skyboxes")]
    public Material daytimeSkybox;
    public Material blackSkybox;
    public Material currentGameSkybox;

    [Header("Lighting")]
    public Light directionalLight;

    public Color daytimeAmbientColor = new Color(0.65f, 0.65f, 0.65f);
    public Color blackAmbientColor = Color.black;
    public Color currentGameAmbientColor = Color.black;

    public Color daytimeFogColor = new Color(0.55f, 0.65f, 0.75f);
    public Color blackFogColor = Color.black;
    public Color currentGameFogColor = Color.black;

    public float daytimeFogDensity = 0.005f;
    public float blackSkyFogDensity = 0f;
    public float currentGameFogDensity = 0.333f;

    [Header("Screen Fades")]
    public Image blackFadeImage;
    public Image whiteFlashImage;

    [Header("Timing")]
    public float daylightHoldTime = 3f;
    public float fadeToBlackTime = 2.5f;
    public float blackSkyRevealTime = 1f;
    public float blackSkyHoldTime = 2f;
    public float lightningDuration = 4f;
    public float whiteFlashHoldTime = 0.75f;
    public float whiteFlashFadeTime = 1.5f;
    public float delayBeforeReturnControl = 1.5f;

    [Header("Lightning")]
    public GameObject lightningObject;
    public Light lightningFlashLight;
    public AudioSource lightningAudio;

    public float lightningIntensity = 30000f;
    public float lightningBlastIntensity = 60000f;
    public float lightningRange = 5000f;

    public bool scaleLightningAtRuntime = true;
    public float lightningScaleMultiplier = 15f;

    public bool pulseLightning = true;
    public float pulseSpeed = 10f;
    public float pulseVariation = 7000f;

    [Header("Orb")]
    public GameObject possessionOrb;
    public float orbFadeTime = 1.5f;
    public bool fadeOrbByScale = true;

    [Header("Flashlight Plasma")]
    public FlashlightPlasmaGlow flashlightPlasmaGlow;
    public bool activatePlasmaWithOrb = true;

    [Header("Ragdoll / Knockback")]
    public OpeningRagdollKnockback ragdollKnockback;
    public bool useFallbackKnockbackIfNoRagdoll = true;
    public float fallbackKnockbackDistance = 2f;
    public float fallbackKnockbackUp = 0.4f;
    public float fallbackKnockbackTime = 0.35f;

    [Header("Control")]
    public bool lockPlayerDuringIntro = true;
    public bool returnPlayerControlAfterIntro = true;

    private Vector3 originalLightningScale;
    private bool storedLightningScale;

    private Vector3 originalOrbScale = Vector3.one;
    private bool storedOrbScale;

    private void Start()
    {
        StartCoroutine(OpeningSequence());
    }

    private IEnumerator OpeningSequence()
    {
        if (cameraFollow != null)
        {
            cameraFollow.enabled = true;
            cameraFollow.SnapCameraToTarget();
        }

        if (lockPlayerDuringIntro)
        {
            LockPlayerMovementOnly();
        }

        SetupDayScene();

        yield return new WaitForSeconds(daylightHoldTime);

        yield return FadeToBlackScreen();

        ApplyBlackSkyScene();

        yield return RevealBlackSky();

        yield return new WaitForSeconds(blackSkyHoldTime);

        yield return PlayLightningOnBlackSky();

        yield return WhiteFlashSwitchToCurrentSkyAndOrb();

        yield return new WaitForSeconds(delayBeforeReturnControl);

        if (returnPlayerControlAfterIntro)
        {
            UnlockPlayerMovement();
        }

        if (cameraFollow != null)
        {
            cameraFollow.SnapCameraToTarget();
        }
    }

    private void SetupDayScene()
    {
        if (daytimeSkybox != null)
        {
            RenderSettings.skybox = daytimeSkybox;
        }

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = daytimeAmbientColor;

        RenderSettings.fog = true;
        RenderSettings.fogColor = daytimeFogColor;
        RenderSettings.fogDensity = daytimeFogDensity;

        if (directionalLight != null)
        {
            directionalLight.enabled = true;
            directionalLight.intensity = 1f;
        }

        if (lightningObject != null)
        {
            lightningObject.SetActive(false);

            if (!storedLightningScale)
            {
                originalLightningScale = lightningObject.transform.localScale;
                storedLightningScale = true;
            }

            if (scaleLightningAtRuntime)
            {
                lightningObject.transform.localScale = originalLightningScale * lightningScaleMultiplier;
            }
        }

        if (lightningFlashLight != null)
        {
            lightningFlashLight.enabled = true;
            lightningFlashLight.range = lightningRange;
            lightningFlashLight.intensity = 0f;
        }

        if (possessionOrb != null)
        {
            if (!storedOrbScale)
            {
                originalOrbScale = possessionOrb.transform.localScale;
                storedOrbScale = true;
            }

            possessionOrb.SetActive(false);

            if (fadeOrbByScale)
            {
                possessionOrb.transform.localScale = Vector3.zero;
            }
        }

        if (flashlightPlasmaGlow != null)
        {
            flashlightPlasmaGlow.DeactivateGlow();
        }

        SetImageAlpha(blackFadeImage, 0f);
        SetImageAlpha(whiteFlashImage, 0f);
    }

    private IEnumerator FadeToBlackScreen()
    {
        float elapsed = 0f;

        while (elapsed < fadeToBlackTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeToBlackTime);

            RenderSettings.ambientLight = Color.Lerp(daytimeAmbientColor, Color.black, t);
            RenderSettings.fogColor = Color.Lerp(daytimeFogColor, Color.black, t);
            RenderSettings.fogDensity = Mathf.Lerp(daytimeFogDensity, blackSkyFogDensity, t);

            if (directionalLight != null)
            {
                directionalLight.intensity = Mathf.Lerp(1f, 0f, t);
            }

            SetImageAlpha(blackFadeImage, t);

            if (cameraFollow != null)
            {
                cameraFollow.SnapCameraToTarget();
            }

            yield return null;
        }

        SetImageAlpha(blackFadeImage, 1f);
    }

    private void ApplyBlackSkyScene()
    {
        if (blackSkybox != null)
        {
            RenderSettings.skybox = blackSkybox;
        }

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = blackAmbientColor;

        RenderSettings.fog = true;
        RenderSettings.fogColor = blackFogColor;
        RenderSettings.fogDensity = blackSkyFogDensity;

        if (directionalLight != null)
        {
            directionalLight.intensity = 0f;
            directionalLight.enabled = false;
        }

        if (lightningObject != null)
        {
            lightningObject.SetActive(false);
        }

        if (lightningFlashLight != null)
        {
            lightningFlashLight.enabled = true;
            lightningFlashLight.intensity = 0f;
            lightningFlashLight.range = lightningRange;
        }

        Debug.Log("OPENING: BLACK SKYBOX APPLIED");
    }

    private IEnumerator RevealBlackSky()
    {
        ApplyBlackSkyScene();

        float elapsed = 0f;

        while (elapsed < blackSkyRevealTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / blackSkyRevealTime);

            ApplyBlackSkyScene();
            SetImageAlpha(blackFadeImage, Mathf.Lerp(1f, 0f, t));

            yield return null;
        }

        ApplyBlackSkyScene();
        SetImageAlpha(blackFadeImage, 0f);
    }

    private IEnumerator PlayLightningOnBlackSky()
    {
        ApplyBlackSkyScene();

        SetImageAlpha(blackFadeImage, 0f);
        SetImageAlpha(whiteFlashImage, 0f);

        if (lightningObject != null)
        {
            lightningObject.SetActive(true);
            RestartParticles(lightningObject);
        }

        if (lightningFlashLight != null)
        {
            lightningFlashLight.enabled = true;
            lightningFlashLight.range = lightningRange;
            lightningFlashLight.intensity = lightningIntensity;
        }

        Debug.Log("OPENING: LIGHTNING STARTED ON BLACK SKYBOX");

        float elapsed = 0f;

        while (elapsed < lightningDuration)
        {
            elapsed += Time.deltaTime;

            // Hard-lock the black sky for the full lightning VFX duration.
            if (blackSkybox != null)
            {
                RenderSettings.skybox = blackSkybox;
            }

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.black;

            RenderSettings.fog = true;
            RenderSettings.fogColor = Color.black;
            RenderSettings.fogDensity = blackSkyFogDensity;

            if (directionalLight != null)
            {
                directionalLight.enabled = false;
                directionalLight.intensity = 0f;
            }

            if (pulseLightning && lightningFlashLight != null)
            {
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
                lightningFlashLight.intensity = lightningIntensity + pulse * pulseVariation;
            }

            yield return null;
        }
    }

    private IEnumerator WhiteFlashSwitchToCurrentSkyAndOrb()
    {
        SetImageAlpha(whiteFlashImage, 1f);

        if (lightningFlashLight != null)
        {
            lightningFlashLight.intensity = lightningBlastIntensity;
            lightningFlashLight.range = lightningRange;
        }

        if (lightningAudio != null)
        {
            lightningAudio.Play();
        }

        TriggerRagdollOrFallbackKnockback();

        ApplyCurrentGameSkyScene();
        StartCoroutine(FadeInOrbAndPlasma());

        Debug.Log("OPENING: CURRENT/NIGHT SKYBOX APPLIED DURING WHITE FLASH");

        yield return new WaitForSeconds(whiteFlashHoldTime);

        float elapsed = 0f;

        while (elapsed < whiteFlashFadeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / whiteFlashFadeTime);

            SetImageAlpha(whiteFlashImage, Mathf.Lerp(1f, 0f, t));

            yield return null;
        }

        SetImageAlpha(whiteFlashImage, 0f);

        if (lightningObject != null)
        {
            lightningObject.SetActive(false);
        }

        if (lightningFlashLight != null)
        {
            lightningFlashLight.intensity = 0f;
        }
    }

    private void ApplyCurrentGameSkyScene()
    {
        if (currentGameSkybox != null)
        {
            RenderSettings.skybox = currentGameSkybox;
        }

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = currentGameAmbientColor;

        RenderSettings.fog = true;
        RenderSettings.fogColor = currentGameFogColor;
        RenderSettings.fogDensity = currentGameFogDensity;

        if (directionalLight != null)
        {
            directionalLight.intensity = 0f;
            directionalLight.enabled = false;
        }
    }

    private IEnumerator FadeInOrbAndPlasma()
    {
        if (possessionOrb != null)
        {
            possessionOrb.SetActive(true);
            RestartParticles(possessionOrb);

            if (fadeOrbByScale)
            {
                possessionOrb.transform.localScale = Vector3.zero;
            }

            float elapsed = 0f;

            while (elapsed < orbFadeTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / orbFadeTime);
                float eased = Mathf.SmoothStep(0f, 1f, t);

                if (fadeOrbByScale)
                {
                    possessionOrb.transform.localScale = Vector3.Lerp(Vector3.zero, originalOrbScale, eased);
                }

                yield return null;
            }

            if (fadeOrbByScale)
            {
                possessionOrb.transform.localScale = originalOrbScale;
            }
        }

        if (activatePlasmaWithOrb && flashlightPlasmaGlow != null)
        {
            flashlightPlasmaGlow.ActivateGlow();
        }
    }

    private void TriggerRagdollOrFallbackKnockback()
    {
        if (ragdollKnockback != null)
        {
            ragdollKnockback.TriggerRagdollBackward();
            return;
        }

        if (useFallbackKnockbackIfNoRagdoll && playerRoot != null)
        {
            StartCoroutine(FallbackKnockbackRoutine());
        }
    }

    private IEnumerator FallbackKnockbackRoutine()
    {
        Vector3 start = playerRoot.position;
        Vector3 backward = -playerRoot.forward.normalized;
        Vector3 end = start + backward * fallbackKnockbackDistance + Vector3.up * fallbackKnockbackUp;

        float elapsed = 0f;

        while (elapsed < fallbackKnockbackTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fallbackKnockbackTime);
            float eased = Mathf.Sin(t * Mathf.PI * 0.5f);

            playerRoot.position = Vector3.Lerp(start, end, eased);

            yield return null;
        }

        playerRoot.position = end;
    }

    private void LockPlayerMovementOnly()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (cameraFollow != null)
        {
            cameraFollow.enabled = true;
            cameraFollow.SnapCameraToTarget();
        }
    }

    private void UnlockPlayerMovement()
    {
        if (cameraFollow != null)
        {
            cameraFollow.enabled = true;
            cameraFollow.SnapCameraToTarget();
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }

    private void RestartParticles(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particle in particles)
        {
            if (particle != null)
            {
                particle.Clear(true);
                particle.Play(true);
            }
        }
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
        {
            return;
        }

        Color color = image.color;
        color.a = Mathf.Clamp01(alpha);
        image.color = color;
        image.raycastTarget = false;
    }
}