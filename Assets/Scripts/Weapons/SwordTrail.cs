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

  private Vector3 lastPosition;
  private float currentSpeed;
  private float lastSoundTime;
  private bool wasSwinging;
  private bool wasTrailing;
  private TurnManager turnManager;

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

    if (isSwinging && !wasSwinging)
    {
      var damage = weapon.damage + (playerHealth != null ? playerHealth.Damage : 0);
      Debug.Log($"[Sword] Player {playerIndex} swings — weapon: {weapon.weaponName}, total damage: {damage}");
      turnManager?.OnPlayerSwing(playerIndex, damage);
    }
    
    trailRenderer.emitting = shouldTrail;

    if (!shouldTrail && wasTrailing)
    {
      trailRenderer.emitting = false;
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
