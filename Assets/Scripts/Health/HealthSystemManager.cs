using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameAssets.Health
{
  public class HealthSystemManager : MonoBehaviour
  {
    [SerializeField] private BossHealth bossHealth;

    private readonly List<PlayerHealth> players = new();

    public BossHealth Boss => bossHealth;

    public int AlivePlayerCount
    {
      get
      {
        return players.Count(player => player is not null && player.IsAlive);
      }
    }

    public event Action OnEncounterVictory;
    public event Action OnPartyWipe;

    private bool encounterOver;

    private bool HasSceneAuthority =>
      bossHealth != null && bossHealth.Runner != null && bossHealth.Runner.IsSceneAuthority;

    private void OnEnable()
    {
      PlayerHealth.PlayerSpawned += RegisterPlayer;
      PlayerHealth.PlayerDespawned += UnregisterPlayer;
      
      var existingPlayers =
      FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);

      foreach (var player in existingPlayers) RegisterPlayer(player);

      if (bossHealth == null) bossHealth = FindFirstObjectByType<BossHealth>();

      RegisterBoss(bossHealth);
    }

    private void OnDisable()
    {
      PlayerHealth.PlayerSpawned -= RegisterPlayer;
      PlayerHealth.PlayerDespawned -= UnregisterPlayer;

      if (bossHealth)
      {
        bossHealth.OnBossDefeated -= HandleVictory;
      }

      foreach (var player in players)
      {
        if (player != null) player.OnDeath -= CheckPartyWipe;
      }
      
      players.Clear();
    }

    public void RegisterBoss(BossHealth boss)
    {
      if (bossHealth != null)
      {
        bossHealth.OnBossDefeated -= HandleVictory;
      }

      bossHealth = boss;

      if (bossHealth != null)
      {
        bossHealth.OnBossDefeated += HandleVictory;
      }
    }

    public void RegisterPlayer(PlayerHealth player)
    {
      if (!player || players.Contains(player)) return;
      players.Add(player);
      player.OnDeath += CheckPartyWipe;
      
      Debug.Log($"Registered player health: {player.name}");
    }

    public void UnregisterPlayer(PlayerHealth player)
    {
      if (!player) return;
      
      player.OnDeath -= CheckPartyWipe;
      players.Remove(player);
      Debug.Log($"Unregistered player health: {player.name}");
    }

    public void ApplyDamageToBoss(int damage)
    {
      if (encounterOver || !bossHealth || !bossHealth.IsAlive) return;
      
      bossHealth.RequestDamage(damage);
    }

    public void ApplyDamageToAllPlayers(int damage)
    {
      if (!HasSceneAuthority || encounterOver) return;

      foreach (var player in players)
      {
        if (player && player.IsAlive) player.RequestDamage(damage);
      }
    }

    public void ApplyDamageToPlayer(PlayerHealth player, int damage)
    {
      if (!HasSceneAuthority || encounterOver || !player || !player.IsAlive) return;

      player.RequestDamage(damage);
    }

    public void HealBoss(int heal)
    {
      if (encounterOver || !bossHealth || !bossHealth.IsAlive) return;
      
      bossHealth.RequestHeal(heal);
    }

    public void HealAllPlayers(int heal)
    {
      if (!HasSceneAuthority || encounterOver) return;

      foreach (var player in players.Where(player => player && player.IsAlive))
      {
        player.RequestHeal(heal);
      }
    }

    public void HealPlayer(PlayerHealth player, int heal)
    {
      if (encounterOver || !player || !player.IsAlive) return;
      
      player.RequestHeal(heal);
    }

    private void CheckPartyWipe()
    {
      if (encounterOver) return;

      foreach (var player in players)
      {
        if (player != null && player.IsAlive) return;
      }
      
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
