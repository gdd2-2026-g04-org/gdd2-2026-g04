using System;
using GameAssets.Health;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private HealthSystemManager healthManager;
    [SerializeField] private GameOverUI gameOverUI;

    private bool encounterOver;

    private void Start()
    {
        if (!healthManager) healthManager = FindFirstObjectByType<HealthSystemManager>();
        healthManager.OnEncounterVictory += HandleVictory;
        healthManager.OnPartyWipe += HandlePartyWipe;
    }

    private void OnDestroy()
    {
        if (!healthManager) return;

        healthManager.OnEncounterVictory -= HandleVictory;
        healthManager.OnPartyWipe -= HandlePartyWipe;
    }

    private void HandleVictory()
    {
        if (encounterOver) return;

        encounterOver = true;
        
        Debug.Log("(BattleManager): Victory!");
        
        gameOverUI?.ShowGameOverScreen();
    }

    private void HandlePartyWipe()
    {
        if (encounterOver) return;

        encounterOver = true;
        
        Debug.Log("(BattleManager): Party wiped.");
        
        gameOverUI?.ShowGameOverScreen();
    }
}
