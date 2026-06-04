using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameAssets.Health
{
  public class HealthSystemManager : MonoBehaviour
  {
    [SerializeField] private BossHealth bossHealth;
    [SerializeField] private List<PlayerHealth> players = new();

    public BossHealth Boss => bossHealth;
    public int AlivePlayerCount { get { int n = 0; foreach (var p in players) if (p.IsAlive) n++; return n; } }

    public event Action OnEncounterVictory;
    public event Action OnPartyWipe;

    private bool encounterOver;

    private void OnEnable() { if (bossHealth != null) bossHealth.OnBossDefeated += HandleVictory; }
    private void OnDisable() { if (bossHealth != null) bossHealth.OnBossDefeated -= HandleVictory; }

    public void RegisterBoss(BossHealth boss)
    {
      if (bossHealth != null) bossHealth.OnBossDefeated -= HandleVictory;
      bossHealth = boss;
      bossHealth.OnBossDefeated += HandleVictory;
    }

    public void RegisterPlayer(PlayerHealth player)
    {
      if (!players.Contains(player))
      {
        players.Add(player);
        player.OnDeath += CheckPartyWipe;
      }
    }

    public void ApplyDamageToBoss(int damage)
    {
      if (bossHealth == null || !bossHealth.IsAlive) return;
      bossHealth.TakeDamage(damage);
    }

    public void ApplyDamageToAllPlayers(int damage)
    {
      foreach (PlayerHealth p in players)
        if (p.IsAlive) p.TakeDamage(damage);
    }

    public void ApplyDamageToPlayer(PlayerHealth player, int damage)
    {
      if (player == null || !player.IsAlive) return;
      player.TakeDamage(damage);
    }

    public void HealBoss(int amount)
    {
      if (bossHealth == null || !bossHealth.IsAlive) return;
      bossHealth.Heal(amount);
    }

    public void HealAllPlayers(int amount)
    {
      foreach (PlayerHealth p in players)
        if (p.IsAlive) p.Heal(amount);
    }

    public void HealPlayer(PlayerHealth player, int amount)
    {
      if (player == null || !player.IsAlive) return;
      player.Heal(amount);
    }

    private void CheckPartyWipe()
    {
      foreach (PlayerHealth p in players)
        if (p.IsAlive) return;
      HandlePartyWipe();
    }

    private void HandleVictory()
    {
      if (encounterOver) return;
      encounterOver = true;
      OnEncounterVictory?.Invoke();
    }

    private void HandlePartyWipe()
    {
      if (encounterOver) return;
      encounterOver = true;
      OnPartyWipe?.Invoke();
    }
  }
}
