using System;
using Fusion;
using UnityEngine;

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
          return data != null ? data.maxHP : base.GetStartingMaxHP();
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
