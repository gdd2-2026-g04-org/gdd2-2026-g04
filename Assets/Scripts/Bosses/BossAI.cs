using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAssets.Health;

public class BossAI : MonoBehaviour
{
    [Header("Swipe Attack")]
    [SerializeField] private float attackInterval = 5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float damageDelay = 0.6f;

    [Header("AOE Jump Earthquake")]
    [SerializeField] private int aoeDamage = 25;
    [SerializeField] private float aoeDamageDelay = 3.3f;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem bigChunkVFX;
    [SerializeField] private ParticleSystem smallChunkVFX;
    [SerializeField] private ParticleSystem dustVFX;

    private float timer;
    private HealthSystemManager healthManager;
    private BossHealth bossHealth;
    private Animator animator;
    private bool gameOver = false;
    private bool isAttacking = false;
    private readonly HashSet<float> triggeredThresholds = new HashSet<float>();

    private void Start()
    {
        healthManager = FindFirstObjectByType<HealthSystemManager>();
        animator = GetComponent<Animator>();
        bossHealth = GetComponent<BossHealth>();
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
            StartSwipeAttack();
            timer = attackInterval;
        }

        TryTriggerAOEAtHealthThresholds();
    }

    private void StartSwipeAttack()
    {
        if (gameOver || isAttacking) return;

        isAttacking = true;
        if (animator != null)
            animator.SetTrigger("Swipe");

        StartCoroutine(ApplySwipeDamageAfterDelay());
    }

    private IEnumerator ApplySwipeDamageAfterDelay()
    {
        yield return new WaitForSeconds(damageDelay);

        if (!gameOver && healthManager != null)
        {
            healthManager.ApplyDamageToAllPlayers(attackDamage);
            Debug.Log($"[BossAI] Swipe dealt {attackDamage} damage.");
        }

        isAttacking = false;
    }

    // ===================== AOE LOGIC =====================
    private void TryTriggerAOEAtHealthThresholds()
    {
        if (bossHealth == null || gameOver) return;

        float hpPercent = (float)bossHealth.CurrentHP / bossHealth.MaxHP;

        if (hpPercent <= 0.75f && !triggeredThresholds.Contains(0.75f))
            TriggerAOE(0.75f);
        else if (hpPercent <= 0.50f && !triggeredThresholds.Contains(0.50f))
            TriggerAOE(0.50f);
        else if (hpPercent <= 0.25f && !triggeredThresholds.Contains(0.25f))
            TriggerAOE(0.25f);
    }

    private void TriggerAOE(float threshold)
    {
        triggeredThresholds.Add(threshold);
        isAttacking = true;

        Debug.Log($"[BossAI] AOE triggered at {threshold * 100}% HP!");

        if (animator != null)
        {
            animator.Play("AOE", 0, 0f);
        }

        StartCoroutine(ApplyAOEDamageAfterDelay());
    }

    private IEnumerator ApplyAOEDamageAfterDelay()
    {
        yield return new WaitForSeconds(aoeDamageDelay);

        if (gameOver || healthManager == null)
        {
            isAttacking = false;
            yield break;
        }

        // === Play Particle Systems ===
        PlayParticleEffect(bigChunkVFX);
        PlayParticleEffect(smallChunkVFX);
        PlayParticleEffect(dustVFX);

        // Apply damage
        healthManager.ApplyDamageToAllPlayers(aoeDamage);
        Debug.Log($"AOE Earthquake dealt {aoeDamage} damage to all players.");

        isAttacking = false;
    }

    private void PlayParticleEffect(ParticleSystem ps)
    {
        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }
    }

    private void HandleGameOver()
    {
        gameOver = true;
        StopAllCoroutines();
        Debug.Log("[BossAI] Game Over - Boss stopped attacking.");
    }
}