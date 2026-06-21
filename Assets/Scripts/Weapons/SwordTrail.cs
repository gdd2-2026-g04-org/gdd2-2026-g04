using System;
using GameAssets.Battle;
using GameAssets.Health;
using UnityEngine;

namespace GameAssets.Weapons
{
public class SwordTrail : MonoBehaviour
{
  [Header("Player")]
  [SerializeField] private int playerIndex = 0;
  [SerializeField] private PlayerHealth playerHealth;

  [Header("Weapon")]
  [SerializeField] private WeaponData weapon;

  [Header("Trail")]
  [SerializeField] private TrailRenderer trailRenderer;
  [SerializeField] private float trailDelay = 0.2f;

  [Header("Sound")]
  [SerializeField] private AudioSource audioSource;
  [SerializeField] private AudioClip swingSound;

  [Header("Combat Settings")]
  [SerializeField] private float attackCooldown = 1.5f;

  private Vector3 lastPosition;
  private float currentSpeed;
  private float lastSoundTime;
  private float lastAttackTime;
  private bool wasSwinging;
  private bool wasTrailing;

  private TurnManager turnManager;
  private HealthSystemManager healthManager;

  private void Awake()
  {
    if (trailRenderer != null)
    {
      trailRenderer.emitting = false;
      trailRenderer.Clear();
    }
  }

  private void Start()
  {
    lastPosition = transform.position;
    turnManager = TurnManager.Instance;
    healthManager = FindFirstObjectByType<HealthSystemManager>();

    if (playerHealth == null)
      playerHealth = GetComponentInParent<PlayerHealth>();
  }

  private void LateUpdate()
  {
    if (weapon == null || trailRenderer == null) return;

    currentSpeed = (transform.position - lastPosition).magnitude / Time.deltaTime;
    lastPosition = transform.position;

    bool isSwinging = currentSpeed > weapon.minSpeedForTrail;
    
    trailRenderer.emitting = isSwinging;

    if (isSwinging) lastSoundTime = Time.time;

    bool shouldTrail = Time.time <= lastSoundTime + trailDelay;
    trailRenderer.emitting = shouldTrail;

    if (!shouldTrail && wasTrailing)
    {
        trailRenderer.emitting = false;
    }

    // === SWING DETECTION + IMMEDIATE DAMAGE + COOLDOWN ===
    if (isSwinging && !wasSwinging)
    {
        // Only allow attack during player turn
        if (turnManager != null && turnManager.IsPlayerTurn)
        {
            // Check cooldown
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                int damage = weapon.damage + (playerHealth != null ? playerHealth.Damage : 0);

                // === IMMEDIATE DAMAGE ===
                if (healthManager != null)
                {
                    healthManager.ApplyDamageToBoss(damage);
                }

                Debug.Log($"[Sword] Player {playerIndex} swings for {damage} damage (IMMEDIATE)");

                lastAttackTime = Time.time; // Start cooldown
            }
            else
            {
                Debug.Log($"[Sword] Player {playerIndex} attack on cooldown");
            }
        }
    }

    if (isSwinging && Time.time > lastSoundTime + weapon.soundCooldown)
    {
      if (audioSource != null && swingSound != null)
      {
        audioSource.PlayOneShot(swingSound);
        lastSoundTime = Time.time;
      }
    }
    
    wasSwinging = isSwinging;
    wasTrailing = shouldTrail;
  }
}
}
