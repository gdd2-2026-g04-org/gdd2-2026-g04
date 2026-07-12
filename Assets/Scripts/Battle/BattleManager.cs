using System;
using System.Collections;
using GameAssets.Health;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private HealthSystemManager healthManager;
    [SerializeField] private GameOverUI gameOverUI;

    [SerializeField] private float gameOverDelay = 3f;
    
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
        
        StartCoroutine(GameOverSequence(true));
    }

    private void HandlePartyWipe()
    {
        if (encounterOver) return;

        encounterOver = true;
        
        Debug.Log("(BattleManager): Party wiped.");
        
        StartCoroutine(GameOverSequence(false));
    }

    private IEnumerator GameOverSequence(bool victory)
    {
        gameOverUI?.ShowGameOver(victory);
        
        yield return new WaitForSeconds(gameOverDelay);
        
        gameOverUI?.ShowGameOverPanel();
    }
}
