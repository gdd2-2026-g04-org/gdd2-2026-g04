using UnityEngine;
using UnityEngine.SceneManagement;

public class MageQTECircleController : MonoBehaviour
{
    [Header("Staff Reference")]
    public Transform staffTip;

    [Header("Charge Particles")]
    public ParticleSystem chargeParticles;
    public float minParticleSize = 0.05f;
    public float maxParticleSize = 0.5f;
    public float minEmissionRate = 5f;
    public float maxEmissionRate = 80f;
    public Color chargeStartColor = new Color(0.3f, 0.5f, 1f);
    public Color chargeFullColor  = new Color(1f, 0.2f, 0.2f);

    [Header("Full Charge Burst")]
    [Tooltip("Optional one-shot ParticleSystem that fires the moment charge hits 100%.")]
    public ParticleSystem fullChargeBurst;

    [Header("Timing")]
    public float duration = 6f;
    public float fullChargeHoldDuration = 0.4f;
    public float overloadShrinkDuration = 0.4f;

    public bool TimeOut { get; private set; }
    public float Power { get; private set; }
    public bool IsAimInside => true;

    private bool isCharging;
    private bool isOverloading;
    private bool burstFired;
    private float timer;
    private float fullChargeTimer;
    private float overloadTimer;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        HideCircle();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HideCircle();
    }

    public void ShowCircle()
    {
        if (isCharging || isOverloading) return;

        isCharging = true;
        TimeOut = false;
        Power = 0f;
        timer = duration;
        fullChargeTimer = fullChargeHoldDuration;
        burstFired = false;

        if (chargeParticles != null)
        {
            ApplyParticleState(0f);
            if (!chargeParticles.isPlaying)
                chargeParticles.Play();
        }
    }

    public void HideCircle()
    {
        isCharging = false;
        isOverloading = false;

        if (chargeParticles != null)
            chargeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Update()
    {
        SyncParticlePosition();

        if (isOverloading)
            UpdateOverload();
        else if (isCharging)
            UpdateCharging();
    }

    private void UpdateCharging()
    {
        timer -= Time.deltaTime;
        Power = Mathf.Clamp01(1f - (timer / duration));

        if (chargeParticles != null)
            ApplyParticleState(Power);

        if (!burstFired && Power >= 1f)
        {
            burstFired = true;
            if (fullChargeBurst != null)
                fullChargeBurst.Play();
        }

        if (timer <= 0f)
        {
            fullChargeTimer -= Time.deltaTime;
            if (fullChargeTimer <= 0f)
            {
                isCharging = false;
                isOverloading = true;
                TimeOut = true;
                overloadTimer = overloadShrinkDuration;
            }
        }
        else
        {
            fullChargeTimer = fullChargeHoldDuration;
        }
    }

    private void UpdateOverload()
    {
        overloadTimer -= Time.deltaTime;
        float t = Mathf.Clamp01(overloadTimer / overloadShrinkDuration);
        ApplyParticleState(t);

        if (overloadTimer <= 0f)
        {
            isOverloading = false;
            HideCircle();
        }
    }

    private void SyncParticlePosition()
    {
        if (chargeParticles != null && staffTip != null)
            chargeParticles.transform.SetPositionAndRotation(staffTip.position, staffTip.rotation);
    }

    private void ApplyParticleState(float t)
    {
        if (chargeParticles == null) return;

        var main = chargeParticles.main;
        main.startSize = Mathf.Lerp(minParticleSize, maxParticleSize, t);
        main.startColor = Color.Lerp(chargeStartColor, chargeFullColor, t);

        var emission = chargeParticles.emission;
        emission.rateOverTime = Mathf.Lerp(minEmissionRate, maxEmissionRate, t);
    }
}
