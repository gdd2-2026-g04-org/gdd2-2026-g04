using System.Collections;
using UnityEngine;
using GameAssets.Health;

public class BossAI : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackInterval = 5f;
    [SerializeField] private int attackDamage = 10;

    [Header("Telegraph")]
    [Tooltip("Delay between animation start and actual damage")]
    [SerializeField] private float damageDelay = 0.6f;

    private float timer;
    private HealthSystemManager healthManager;
    private Animator animator;
    private bool gameOver = false;
    private bool isAttacking = false;   // Prevents overlapping attacks

    private void Start()
    {
        healthManager = FindFirstObjectByType<HealthSystemManager>();
        animator = GetComponent<Animator>();
        timer = attackInterval;
    }

    private void OnDestroy()
    {
        if (healthManager != null)
        {
            healthManager.OnEncounterVictory -= HandleGameOver;
            healthManager.OnPartyWipe -= HandleGameOver;
        }
    }

    private void Update()
    {
        if (gameOver || isAttacking) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            StartAttack();
            timer = attackInterval;
        }
    }

    private void StartAttack()
    {
        if (gameOver || isAttacking) return;

        isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger("Swipe");
        }

        StartCoroutine(ApplyDamageAfterDelay());
    }

    private IEnumerator ApplyDamageAfterDelay()
    {
        yield return new WaitForSeconds(damageDelay);

        if (!gameOver && healthManager != null)
        {
            healthManager.ApplyDamageToAllPlayers(attackDamage);
            Debug.Log($"[BossAI] Boss dealt {attackDamage} damage after delay.");
        }

        isAttacking = false;
    }

    private void HandleGameOver()
    {
        gameOver = true;
        StopAllCoroutines();
        Debug.Log("[BossAI] Game Over - Boss stopped attacking.");
    }
}