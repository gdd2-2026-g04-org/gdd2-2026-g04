using GameAssets.Health;
using UnityEngine;
using UnityEngine.InputSystem;

public class HealerStaff : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionProperty triggerAction;

    [Header("Charge Settings")]
    [SerializeField] private float chargeDuration = 1.5f;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject energyBallPrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Visual Feedback")]
    [SerializeField] private ParticleSystem chargeParticles;
    [SerializeField] private ParticleSystem shootParticles;
    [SerializeField] private GameObject staffGlowObject;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip chargeSound;
    [SerializeField] private AudioClip chargeFinishSound;
    [SerializeField] private AudioClip shootSound;

    private HealthSystemManager healthManager;
    private float currentCharge;
    private bool isFullyCharged;
    private bool isCharging;

    private void OnEnable()
    {
        healthManager = FindFirstObjectByType<HealthSystemManager>();
        ResetCharge();
    }

    private void OnDisable()
    {
        ResetCharge();
    }

    private void Update()
    {
        if (healthManager == null || healthManager.Boss == null) return;

        if (!CanAct())
        {
            ResetCharge();
            return;
        }

        bool triggerHeld = triggerAction.action != null && triggerAction.action.ReadValue<float>() > 0.5f;

        if (triggerHeld)
        {
            if (!isCharging)
            {
                isCharging = true;
                if (chargeParticles)
                {
                    chargeParticles.Play();
                    AudioManager.PlaySoundAtSource(chargeSound, audioSource);
                }
            }

            if (!isFullyCharged)
            {
                currentCharge += Time.deltaTime;
                if (currentCharge >= chargeDuration)
                {
                    isFullyCharged = true;
                    if (staffGlowObject) staffGlowObject.SetActive(true);
                    audioSource.Stop();
                    AudioManager.PlaySoundAtSource(chargeFinishSound, audioSource);
                    Debug.Log("(HealerStaff): Fully Charged! Release trigger to shoot.");
                }
            }
        }
        else
        {
            if (isCharging)
            {
                if (isFullyCharged)
                {
                    FireEnergyBall();
                }
                ResetCharge();
            }
        }
    }

    private void FireEnergyBall()
    {
        if (!CanAct()) return;
        
        if (energyBallPrefab && projectileSpawnPoint)
        {
            GameObject projectileObj = Instantiate(energyBallPrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
            HealerProjectile projectile = projectileObj.GetComponent<HealerProjectile>();
            if (projectile)
            {
                projectile.Initialize(healthManager.Boss.transform, healthManager, true);

                if (NetworkManager.Instance && NetworkManager.Instance.LocalAvatar)
                {
                    NetworkManager.Instance.LocalAvatar.RPC_HealerProjectileVisual(projectileSpawnPoint.position, projectileSpawnPoint.forward);
                }
            }
            else
            {
                Debug.LogError("[HealerStaff] The instantiated energyBallPrefab is missing the HealerProjectile script on its root object!", energyBallPrefab);
            }

            if (shootParticles)
            {
                // Align the particles to the tip of the staff and fire
                shootParticles.transform.position = projectileSpawnPoint.position;
                shootParticles.Play();
            }
        }
    }

    private void ResetCharge()
    {
        currentCharge = 0f;
        isFullyCharged = false;
        isCharging = false;
        if (chargeParticles) chargeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (staffGlowObject) staffGlowObject.SetActive(false);
        audioSource.Stop();
    }
    
    private PlayerHealth playerHealth;
    
    private bool CanAct()
    {
        if (!playerHealth && NetworkManager.Instance)
        {
            playerHealth = NetworkManager.Instance.LocalPlayerHealth;
        }
        return playerHealth && playerHealth.IsAlive;
    }
}