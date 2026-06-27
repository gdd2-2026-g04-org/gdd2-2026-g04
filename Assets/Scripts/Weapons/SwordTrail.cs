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
  [SerializeField] private float attackCooldown = 1.0f;

  private Vector3 lastPosition;
  private float currentSpeed;
  private float lastSoundTime;
  private float lastAttackTime;
  private bool wasSwinging;
  private bool wasTrailing;
  private float lastTrailTime;
  private float lastSwingSoundTime;

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
      bool canAttack = Time.time >= lastAttackTime + attackCooldown;
      bool isValidSwing = isSwinging && canAttack;

      // === TRAIL (uses its own timer) ===
      if (isValidSwing)
      {
          lastTrailTime = Time.time;           // ← Separate timer for trail
      }

      bool shouldTrail = Time.time <= lastTrailTime + trailDelay;
      trailRenderer.emitting = shouldTrail && canAttack;

      if (!shouldTrail && wasTrailing)
      {
          trailRenderer.emitting = false;
      }

      // === DAMAGE + COOLDOWN ===
      if (isSwinging && !wasSwinging)
      {
          if (turnManager != null && turnManager.IsPlayerTurn)
          {
              if (canAttack)
              {
                  int damage = weapon.damage + (playerHealth != null ? playerHealth.Damage : 0);

                  if (healthManager != null)
                  {
                      healthManager.ApplyDamageToBoss(damage);
                  }

                  Debug.Log($"[Sword] Player {playerIndex} swings for {damage} damage (IMMEDIATE)");
                  lastAttackTime = Time.time;
              }
              else
              {
                  Debug.Log($"[Sword] Player {playerIndex} attack on cooldown");
              }
          }
      }

      // === SOUND (plays together with trail) ===
      if (isValidSwing && Time.time > lastSwingSoundTime + weapon.soundCooldown)
      {
          if (audioSource != null && swingSound != null)
          {
              audioSource.PlayOneShot(swingSound);
              lastSwingSoundTime = Time.time;
          }
      }

      wasSwinging = isSwinging;
      wasTrailing = shouldTrail;
  }
}
}
