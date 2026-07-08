using System;
using GameAssets.Health;
using UnityEngine;

namespace GameAssets.Weapons
{
public class SwordTrail : MonoBehaviour
{
  [Header("Weapon")]
  [SerializeField] private WeaponData weapon;

  [Header("Trail")] [SerializeField] private TrailRenderer trailRenderer;

  [SerializeField, Min(0f)]
  private float trailDelay = 0.2f;

  [Header("Combat")] [SerializeField, Min(0f)]
  private float attackCooldown = 1f;

  private PlayerHealth playerHealth;
  private HealthSystemManager healthManager;

  private Vector3 lastPosition;
  private float lastAttackTime = float.NegativeInfinity;
  private float lastTrailTime = float.NegativeInfinity;
  private float lastSwingSoundTime = float.NegativeInfinity;

  private bool wasSwinging;

  private void Awake()
  {
    if (!trailRenderer) return;

    trailRenderer.emitting = false;
    trailRenderer.Clear();
  }

  private void OnEnable()
  {
    lastPosition = transform.position;
    wasSwinging = false;

    if (trailRenderer)
    {
      trailRenderer.emitting = false;
      trailRenderer.Clear();
    }

    ResolveReferences();
  }

  private void OnDisable()
  {
    if (trailRenderer)
    {
      trailRenderer.emitting = false;
      trailRenderer.Clear();
    }

    wasSwinging = false;
  }

  private void LateUpdate()
  {
    if (!weapon || !trailRenderer) return;
    
    var deltaTime = Time.deltaTime;

    if (deltaTime <= 0f) return;

    var curSpeed = Vector3.Distance(transform.position, lastPosition) / deltaTime;

    lastPosition = transform.position;

    var isSwinging = curSpeed > weapon.minSpeedForTrail;

    bool attackReady = Time.time >= lastAttackTime + attackCooldown;
    
    UpdateTrail(isSwinging, attackReady);
    UpdateSound(isSwinging, attackReady);
    
    if (isSwinging && !wasSwinging && attackReady) TryAttackBoss();

    wasSwinging = isSwinging;
  }

  private void UpdateTrail(bool isSwinging, bool attackReady)
  {
    if (isSwinging && attackReady)
    {
      lastTrailTime = Time.time;
    }
    
    var shouldEmit = Time.time <= lastTrailTime + trailDelay;

    trailRenderer.emitting = shouldEmit;
  }

  private void UpdateSound(bool isSwinging, bool attackReady)
  {
    if (!isSwinging || !attackReady) return;

    if (Time.time < lastSwingSoundTime + weapon.soundCooldown) return;
    
    // # play sound here
    lastSwingSoundTime = Time.time;
  }

  private void TryAttackBoss()
  {
    if (!healthManager)
    {
      Debug.LogWarning("(SwordTrail): Health Manager is missing!");
      return;
    }

    if (!playerHealth)
    {
      Debug.LogWarning("(SwordTrail): PlayerHealth is missing!");
      return;
    }

    var boss = healthManager.Boss;

    if (!boss || !boss.IsSpawned || !boss.IsAlive) return;

    var damage = weapon.damage + playerHealth.Damage;
    
    healthManager.ApplyDamageToBoss(damage);

    lastAttackTime = Time.time;
    
    Debug.Log($"(Sword): Requested {damage} against {boss.name}");
  }

  private void ResolveReferences()
  {
    if (!healthManager)
    {
      healthManager = FindFirstObjectByType<HealthSystemManager>();
    }

    if (!playerHealth && NetworkManager.Instance)
    {
      playerHealth = NetworkManager.Instance.LocalPlayerHealth;
    }
  }
}
}
