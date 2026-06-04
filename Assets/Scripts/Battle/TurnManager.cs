using System;
using System.Collections;
using System.Collections.Generic;
using GameAssets.Health;
using UnityEngine;

namespace GameAssets.Battle
{
  public class TurnManager : MonoBehaviour
  {
    [SerializeField] private HealthSystemManager healthManager;

    [Header("Timing")]
    [SerializeField] private float breakAfterAllSwings = 3.0f;
    [SerializeField] private float breakAfterBoss = 3.0f;

    [Header("Boss")]
    [SerializeField] private int bossAttackDamage = 5;

    public bool IsPlayerTurn { get; private set; } = true;

    public event Action OnPlayerTurnStart;
    public event Action OnBossTurnStart;

    private readonly Dictionary<int, int> pendingDamage = new();
    private bool resolving;

    private void Start()
    {
      if (healthManager == null)
        healthManager = FindFirstObjectByType<HealthSystemManager>();

      healthManager.OnEncounterVictory += () => Debug.Log("[Turn] VICTORY! Boss defeated.");
      healthManager.OnPartyWipe += () => Debug.Log("[Turn] GAME OVER. Party wiped.");

      StartCoroutine(StartEncounter());
    }

    private IEnumerator StartEncounter()
    {
      IsPlayerTurn = false;
      Debug.Log("[Turn] Encounter starting...");
      yield return new WaitForSeconds(2f);
      IsPlayerTurn = true;
      OnPlayerTurnStart?.Invoke();
      Debug.Log($"[Turn] Encounter started. Waiting for {healthManager.AlivePlayerCount} player(s) to act.");
    }

    public void OnPlayerSwing(int playerIndex, int damage)
    {
      if (!IsPlayerTurn)
      {
        Debug.Log("[Turn] Swing ignored — not player's turn.");
        return;
      }
      if (resolving) return;

      if (pendingDamage.ContainsKey(playerIndex))
      {
        Debug.Log($"[Turn] Player {playerIndex} already acted this turn.");
        return;
      }

      pendingDamage[playerIndex] = damage;
      Debug.Log($"[Turn] Player {playerIndex} swings for {damage} damage. ({pendingDamage.Count}/{healthManager.AlivePlayerCount} acted)");

      if (pendingDamage.Count >= healthManager.AlivePlayerCount)
        StartCoroutine(ResolveTurn());
    }

    private IEnumerator ResolveTurn()
    {
      resolving = true;
      IsPlayerTurn = false;

      yield return new WaitForSeconds(breakAfterAllSwings);

      int totalDamage = 0;
      foreach (int dmg in pendingDamage.Values) totalDamage += dmg;
      healthManager.ApplyDamageToBoss(totalDamage);
      Debug.Log($"[Turn] Boss takes {totalDamage} total damage. Boss HP: {healthManager.Boss?.CurrentHP} / {healthManager.Boss?.MaxHP}");

      pendingDamage.Clear();

      if (healthManager.Boss == null || !healthManager.Boss.IsAlive)
      {
        Debug.Log("[Turn] Boss is dead!");
        resolving = false;
        yield break;
      }

      OnBossTurnStart?.Invoke();
      Debug.Log("[Turn] Boss turn — incoming attack...");
      yield return new WaitForSeconds(breakAfterBoss);

      healthManager.ApplyDamageToAllPlayers(bossAttackDamage);
      Debug.Log($"[Turn] Boss attacks for {bossAttackDamage} damage.");

      resolving = false;
      IsPlayerTurn = true;
      OnPlayerTurnStart?.Invoke();
      Debug.Log($"[Turn] Player's turn. Waiting for {healthManager.AlivePlayerCount} player(s) to act.");
    }
  }
}
