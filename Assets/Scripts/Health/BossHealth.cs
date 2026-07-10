using System;
using Fusion;
using UnityEngine;
using System.Linq;

namespace GameAssets.Health
{
  public class BossHealth : HealthComponent
  {
    [SerializeField] private BossData data;

    private Animator animator;
    private BossAI bossAI;

    public BossData Data => data;

    public event Action OnBossDefeated;

    protected override int GetStartingMaxHP()
    {
        if (data == null)
            return base.GetStartingMaxHP();

        int playerCount = 1;

        if (Runner != null && Runner.ActivePlayers != null)
        {
            playerCount = Mathf.Max(1, Runner.ActivePlayers.Count());
        }

        int scaledHP = Mathf.RoundToInt(data.maxHP * (1f + (playerCount - 1) * 1f));

        Debug.Log($"(BossHealth) Scaling HP for {playerCount} player(s). Final HP: {scaledHP}");
        return scaledHP;
    }

    public override void Spawned()
    {
        animator = GetComponent<Animator>();
        bossAI = GetComponent<BossAI>();

        OnDeath += HandleDeath;
        
        base.Spawned();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        OnDeath -= HandleDeath;
        base.Despawned(runner, hasState);
    }

    public void RequestDamage(int damage)
    {
        if (damage <= 0) return;

        if (Object.HasStateAuthority)
        {
            ApplyDamage(damage);
            return;
        }

        RPC_RequestDamage(damage);
    }

    public void RequestHeal(int heal)
    {
        if (heal <= 0) return;

        if (Object.HasStateAuthority)
        {
            ApplyHealing(heal);
            return;
        }

        RPC_RequestHeal(heal);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestDamage(int damage)
    {
        ApplyDamage(damage);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestHeal(int heal)
    {
        ApplyHealing(heal);
    }

    private void HandleDeath()
    {
        if (animator != null) animator.SetTrigger("Die");

        if (bossAI != null) bossAI.enabled = false;
        
        OnBossDefeated?.Invoke();
    }
  }
}
