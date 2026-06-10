using System;
using UnityEngine;

namespace GameAssets.Health
{
  public abstract class HealthComponent : MonoBehaviour
  {
    [SerializeField] protected int maxHP = 100;

    public int MaxHP => maxHP;
    public int CurrentHP { get; protected set; }
    public bool IsAlive => CurrentHP > 0;
    public float NormalisedHP => maxHP > 0 ? (float)CurrentHP / maxHP : 0f;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    protected virtual void Awake() => CurrentHP = maxHP;

public virtual void TakeDamage(int damage)
{
    PlayerHealth player = this as PlayerHealth;
    if (player != null)
    {
        Shield activeShield = FindFirstObjectByType<Shield>();
        if (activeShield != null && activeShield.isHeld)
        {
            Debug.Log("Shield blocked the damage!");
            return;
        }
    }

    if (!IsAlive || damage <= 0) return;
    CurrentHP = Mathf.Max(0, CurrentHP - damage);
    OnHealthChanged?.Invoke(CurrentHP, maxHP);
    if (CurrentHP <= 0) OnDeath?.Invoke();
}

    public virtual void Heal(int amount)
    {
      if (!IsAlive || amount <= 0) return;
      CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
      OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }

    public virtual void ResetHealth()
    {
      CurrentHP = maxHP;
      OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }
  }
}
