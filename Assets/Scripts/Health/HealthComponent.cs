using System;
using Fusion;
using UnityEngine;

namespace GameAssets.Health
{
  public abstract class HealthComponent : NetworkBehaviour
  {
    [SerializeField, Min(1)]
    protected int startingMaxHP = 100;
    
    [Networked]
    public int MaxHP { get; protected set; }
    
    [Networked, OnChangedRender(nameof(OnHealthChangedRender))]
    public int CurrentHP { get; protected set; }
    
    public bool IsAlive => CurrentHP > 0;
    
    public bool IsSpawned { get; private set; }
    
    public float NormalizedHP => MaxHP > 0 ? (float) CurrentHP / MaxHP : 0f;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    private bool deathOccurred;

    public override void Spawned()
    {
      IsSpawned = true;
      deathOccurred = false;

      if (Object.HasStateAuthority)
      {
        MaxHP = Mathf.Max(1, GetStartingMaxHP());
        CurrentHP = MaxHP;
      }
      
      OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
      IsSpawned = false;
    }

    protected virtual int GetStartingMaxHP()
    {
      return startingMaxHP;
    }

    protected bool ApplyDamage(int damage)
    {
      if (!Object.HasStateAuthority) return false;

      if (!IsAlive || damage <= 0) return false;

      CurrentHP = Mathf.Max(0, CurrentHP - damage);
      
      OnHealthChanged?.Invoke(CurrentHP, MaxHP);
      CheckDeath();
      
      return true;
    }

    protected bool ApplyHealing(int heal)
    {
      if (!Object.HasStateAuthority) return false;
      
      if (!IsAlive || heal <= 0) return false;

      var previousHP = CurrentHP;

      CurrentHP = Mathf.Min(MaxHP, CurrentHP + heal);
      
      if (CurrentHP == previousHP) return false;
      
      OnHealthChanged?.Invoke(CurrentHP, MaxHP);
      return true;
    }

    protected void SetMaxHP(int newMaxHP, bool refillHP)
    {
      if (!Object.HasStateAuthority) return;

      MaxHP = Mathf.Max(1, newMaxHP);
      
      CurrentHP = refillHP ? MaxHP : Mathf.Clamp(CurrentHP, 0, MaxHP);

      deathOccurred = CurrentHP <= 0;
      OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    public void ResetHealth()
    {
      if (!Object.HasStateAuthority) return;
      CurrentHP = MaxHP;
      deathOccurred = false;
      OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    private void OnHealthChangedRender()
    {
      OnHealthChanged?.Invoke(CurrentHP, MaxHP);
      CheckDeath();
    }
    
    private void CheckDeath()
    {
      if (CurrentHP > 0 || deathOccurred) return;

      deathOccurred = true;
      OnDeath?.Invoke();
    }
  }
}
