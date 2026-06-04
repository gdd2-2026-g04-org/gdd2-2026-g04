using System;
using UnityEngine;

namespace GameAssets.Health
{
  public class BossHealth : HealthComponent
  {
    [SerializeField] private BossData data;

    public BossData Data => data;

    public event Action OnBossDefeated;

    protected override void Awake()
    {
      if (data != null) maxHP = data.maxHP;
      base.Awake();
      OnDeath += () => OnBossDefeated?.Invoke();
    }

    private void Start()
    {
      FindFirstObjectByType<HealthSystemManager>()?.RegisterBoss(this);
    }
  }
}
