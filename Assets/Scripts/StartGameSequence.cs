using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class StartGameSequence : MonoBehaviour
{
    [Header("Player")]
    public Transform playerRoot;
    public PlayerController playerController;

    [Header("Camera")]
    public CameraFollow cameraFollow;

    [Header("Lightning")]
    public GameObject lightningObject;
    public Light lightningFlashLight;
    public AudioSource lightningAudio;

    [Header("Orb")]
    public GameObject possessionOrb;

    [Header("Flashlight Plasma")]
    public FlashlightPlasmaGlow flashlightPlasmaGlow;

    [Header("Orb / Beacon Visibility")]
    public string visibleLayerName = "ImportantVFX";
    public string beaconObjectName = "Orb_Beacon";

    [Range(0f, 1f)]
    public float beaconLineAlpha = 0.08f;

    public float beaconLineWidth = 0.12f;
    public bool forceBeaconParticlesOn = true;

    [Header("Timing")]
    public float delayBeforeFlash = 0.5f;
    public float lightningVisibleTime = 1.0f;
    public float flashIntensity = 14f;
    public float flashFadeTime = 0.75f;
    public float delayBeforeOrb = 0.5f;

    [Header("Facing")]
    public bool facePlayerTowardLightning = true;

    private int visibleLayer = -1;

    private void Start()
    {
        visibleLayer = LayerMask.NameToLayer(visibleLayerName);

        if (visibleLayer < 0)
        {
            Debug.LogWarning("StartGameSequence: Layer '" + visibleLayerName + "' was not found.");
        }

        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        if (lightningObject != null)
        {
            lightningObject.SetActive(false);
        }

        if (possessionOrb != null)
        {
            SetLayerRecursively(possessionOrb, visibleLayer);
            SetOrbRenderersEnabled(false);
            possessionOrb.SetActive(false);
        }

        if (lightningFlashLight != null)
        {
            lightningFlashLight.intensity = 0f;
        }

        if (flashlightPlasmaGlow != null)
        {
            flashlightPlasmaGlow.DeactivateGlow();
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (facePlayerTowardLightning)
        {
            FacePlayerTowardLightning();
        }

        yield return new WaitForSeconds(delayBeforeFlash);

        if (lightningObject != null)
        {
            lightningObject.SetActive(true);
            RestartParticles(lightningObject);
        }

        if (lightningAudio != null)
        {
            lightningAudio.Play();
        }

        if (lightningFlashLight != null)
        {
            lightningFlashLight.intensity = flashIntensity;
        }

        yield return new WaitForSeconds(lightningVisibleTime);

        if (lightningObject != null)
        {
            lightningObject.SetActive(false);
        }

        float timer = 0f;

        while (timer < flashFadeTime)
        {
            timer += Time.deltaTime;
            float t = timer / flashFadeTime;

            if (lightningFlashLight != null)
            {
                lightningFlashLight.intensity = Mathf.Lerp(flashIntensity, 0f, t);
            }

            yield return null;
        }

        if (lightningFlashLight != null)
        {
            lightningFlashLight.intensity = 0f;
        }

        yield return new WaitForSeconds(delayBeforeOrb);

        if (possessionOrb != null)
        {
            possessionOrb.SetActive(true);

            SetLayerRecursively(possessionOrb, visibleLayer);
            EnsureNamedChildActive(beaconObjectName);

            SetOrbRenderersEnabled(true);
            ConfigureBeaconLine();
            RestartParticles(possessionOrb);

            if (forceBeaconParticlesOn)
            {
                ForceBeaconParticles();
            }
        }

        // Turn on the flashlight plasma at the same time the orb appears.
        if (flashlightPlasmaGlow != null)
        {
            flashlightPlasmaGlow.ActivateGlow();
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }

    private void SetOrbRenderersEnabled(bool enabled)
    {
        if (possessionOrb == null)
        {
            return;
        }

        Renderer[] renderers = possessionOrb.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = enabled;

            if (enabled)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }
    }

    private void ConfigureBeaconLine()
    {
        Transform beacon = FindChildByName(possessionOrb, beaconObjectName);

        if (beacon == null)
        {
            return;
        }

        LineRenderer line = beacon.GetComponent<LineRenderer>();

        if (line == null)
        {
            return;
        }

        line.enabled = true;
        line.widthMultiplier = beaconLineWidth;

        Color start = line.startColor;
        Color end = line.endColor;

        start.a = beaconLineAlpha;
        end.a = beaconLineAlpha;

        line.startColor = start;
        line.endColor = end;

        SetRendererMaterialAlpha(line, beaconLineAlpha);
    }

    private void ForceBeaconParticles()
    {
        Transform beacon = FindChildByName(possessionOrb, beaconObjectName);

        if (beacon == null)
        {
            Debug.LogWarning("StartGameSequence: Could not find Orb_Beacon under PossessionOrb.");
            return;
        }

        beacon.gameObject.SetActive(true);
        SetLayerRecursively(beacon.gameObject, visibleLayer);

        ParticleSystem[] particleSystems = beacon.GetComponentsInChildren<ParticleSystem>(true);

        if (particleSystems.Length == 0)
        {
            Debug.LogWarning("StartGameSequence: No ParticleSystem found on Orb_Beacon or its children.");
            return;
        }

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.gameObject.SetActive(true);

            ParticleSystem.MainModule main = particleSystem.main;
            main.loop = true;
            main.playOnAwake = true;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = true;

            ParticleSystemRenderer particleRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();

            if (particleRenderer != null)
            {
                particleRenderer.enabled = true;
                particleRenderer.sortingFudge = 10f;
                particleRenderer.shadowCastingMode = ShadowCastingMode.Off;
                particleRenderer.receiveShadows = false;
            }

            particleSystem.Clear(true);
            particleSystem.Play(true);
        }
    }

    private void SetRendererMaterialAlpha(Renderer renderer, float alpha)
    {
        if (renderer == null || renderer.material == null)
        {
            return;
        }

        Material material = renderer.material;

        if (material.HasProperty("_Color"))
        {
            Color color = material.GetColor("_Color");
            color.a = alpha;
            material.SetColor("_Color", color);
        }

        if (material.HasProperty("_TintColor"))
        {
            Color color = material.GetColor("_TintColor");
            color.a = alpha;
            material.SetColor("_TintColor", color);
        }

        if (material.HasProperty("_BaseColor"))
        {
            Color color = material.GetColor("_BaseColor");
            color.a = alpha;
            material.SetColor("_BaseColor", color);
        }
    }

    private void EnsureNamedChildActive(string childName)
    {
        Transform child = FindChildByName(possessionOrb, childName);

        if (child != null)
        {
            child.gameObject.SetActive(true);
        }
    }

    private Transform FindChildByName(GameObject root, string childName)
    {
        if (root == null || string.IsNullOrEmpty(childName))
        {
            return null;
        }

        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }

    private void SetLayerRecursively(GameObject root, int layer)
    {
        if (root == null || layer < 0)
        {
            return;
        }

        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            child.gameObject.layer = layer;
        }
    }

    private void FacePlayerTowardLightning()
    {
        if (playerRoot == null || lightningObject == null)
        {
            return;
        }

        Vector3 direction = lightningObject.transform.position - playerRoot.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
        {
            return;
        }

        playerRoot.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
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
            particle.Clear(true);
            particle.Play(true);
        }
    }
}