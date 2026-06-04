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

  [Header("Sound")]
  [SerializeField] private AudioSource audioSource;
  [SerializeField] private AudioClip swingSound;

  private Vector3 lastPosition;
  private float currentSpeed;
  private float lastSoundTime;
  private bool wasSwinging;
  private TurnManager turnManager;

  private void Start()
  {
    lastPosition = transform.position;
    turnManager = FindFirstObjectByType<TurnManager>();

    if (playerHealth == null)
      playerHealth = GetComponentInParent<PlayerHealth>();
  }

  private void Update()
  {
    if (weapon == null) return;

    currentSpeed = (transform.position - lastPosition).magnitude / Time.deltaTime;
    lastPosition = transform.position;

    bool isSwinging = currentSpeed > weapon.minSpeedForTrail;

    if (trailRenderer != null)
      trailRenderer.emitting = isSwinging;

    if (isSwinging && Time.time > lastSoundTime + weapon.soundCooldown)
    {
      if (audioSource != null && swingSound != null)
      {
        audioSource.PlayOneShot(swingSound);
        lastSoundTime = Time.time;
      }
    }

    if (isSwinging && !wasSwinging)
    {
      int damage = weapon.damage + (playerHealth != null ? playerHealth.Damage : 0);
      Debug.Log($"[Sword] Player {playerIndex} swings — weapon: {weapon.weaponName}, total damage: {damage}");
      turnManager?.OnPlayerSwing(playerIndex, damage);
    }

    wasSwinging = isSwinging;
  }
}
}
