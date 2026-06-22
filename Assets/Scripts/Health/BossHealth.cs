using System;
using UnityEngine;

namespace GameAssets.Health
{
  public class BossHealth : HealthComponent
  {
    [SerializeField] private BossData data;
    private Animator animator;

    public BossData Data => data;

    public event Action OnBossDefeated;

    protected override void Awake()
    {
      if (data != null) maxHP = data.maxHP;
      base.Awake();

      animator = GetComponent<Animator>();
      OnDeath += HandleDeath;
    }

    private void HandleDeath()
{
    if (animator != null)
    {
        animator.SetTrigger("Die");
    }

    BossAI bossAI = GetComponent<BossAI>();
    if (bossAI != null)
    {
        bossAI.enabled = false;
    }

    OnBossDefeated?.Invoke();
}


    private void Start()
    {
      FindFirstObjectByType<HealthSystemManager>()?.RegisterBoss(this);
    }
  }
}
