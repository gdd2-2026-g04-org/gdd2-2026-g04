using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using GameAssets.Health;

public class BossAI : NetworkBehaviour
{
    [Header("Swipe Attack")]
    [SerializeField, Min(0.1f)] private float attackInterval = 5f;
    [SerializeField, Min(0)] private int attackDamage = 10;

    [Header("AOE Jump Earthquake")]
    [SerializeField, Min(0)] private int aoeDamage = 25;


    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem bigChunkVFX;
    [SerializeField] private ParticleSystem smallChunkVFX;
    [SerializeField] private ParticleSystem dustVFX;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swipeSound;
    [SerializeField] private AudioClip aoeImpactSound;
    [SerializeField] private AudioClip deathSound;

    private readonly HashSet<int> triggeredThresholds = new();

    private HealthSystemManager healthManager;
    private BossHealth bossHealth;
    private Animator animator;

    private float attackTimer;
    
    [Networked] public NetworkBool IsAttacking { get; private set; }
    [Networked] public NetworkBool EncounterOver { get; private set; }

    public override void Spawned()
    {
        animator = GetComponent<Animator>();
        bossHealth = GetComponent<BossHealth>();
        
        healthManager = FindFirstObjectByType<HealthSystemManager>();

        attackTimer = attackInterval;

        if (Object.HasStateAuthority)
        {
            IsAttacking = false;
            EncounterOver = false;
            triggeredThresholds.Clear();
        }
        
        SubscribeToHealthManager();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        UnsubscribeFromHealthManager();
    }

    private void OnDestroy()
    {
        UnsubscribeFromHealthManager();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (EncounterOver || IsAttacking) return;
        
        TryTriggerAOEAtHealthThresholds();

        if (IsAttacking) return;
        
        attackTimer -= Runner.DeltaTime;

        if (attackTimer <= 0f)
        {
            StartSwipeAttack();
        }
    }
    
    private void StartSwipeAttack()
    {
        if (!Object.HasStateAuthority || EncounterOver || IsAttacking) return;

        IsAttacking = true;
        attackTimer = attackInterval;
        RPC_PlaySwipeAnimation();
    }

    public void ApplySwipeDamageAndEffects()
    {
        if (!Object.HasStateAuthority) return;

        if (EncounterOver || healthManager == null)
        {
            IsAttacking = false;
            return;
        }

        healthManager.ApplyDamageToAllPlayers(attackDamage);
        Debug.Log($"(BossAI): Swipe dealt {attackDamage} damage.");
        IsAttacking = false;
    }

    public void PlayDeathSound()
    {
        AudioManager.PlaySoundAtSource(deathSound, audioSource);
    }

    private void TryTriggerAOEAtHealthThresholds()
    {
        if (!Object.HasStateAuthority || !bossHealth || EncounterOver || !bossHealth.IsAlive ||
            bossHealth.MaxHP <= 0) return;

        var hpPercent = bossHealth.NormalizedHP;

        if (hpPercent <= 0.75f && !triggeredThresholds.Contains(75))
        {
            TriggerAOE(75); 
        } else if (hpPercent <= 0.5f && !triggeredThresholds.Contains(50))
        {
            TriggerAOE(50);
        } else if (hpPercent <= 0.25f && !triggeredThresholds.Contains(25))
        {
            TriggerAOE(25);
        }
    }

    private void TriggerAOE(int thresholdPercent)
    {
        if (!Object.HasStateAuthority || EncounterOver || IsAttacking) return;

        triggeredThresholds.Add(thresholdPercent);
        IsAttacking = true;
        
        Debug.Log($"(BossAI): AOE triggered at {thresholdPercent}% HP!");
        
        RPC_PlayAOEAnimation();
    }

    public void ApplyAOEDamageAndEffects()
    {
        if (!Object.HasStateAuthority) return;

        if (EncounterOver || healthManager == null)
        {
            IsAttacking = false;
            return;
        }

        // Play particles on all clients
        RPC_PlayAOEImpactEffects();

        healthManager.ApplyDamageToAllPlayers(aoeDamage);
        Debug.Log($"(BossAI): AOE dealt {aoeDamage} damage to all players!");

        IsAttacking = false;
        attackTimer = attackInterval;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlaySwipeAnimation()
    {
        if (animator) animator.SetTrigger("Swipe");

        if (audioSource && swipeSound)
        AudioManager.PlaySoundAtSource(swipeSound, audioSource);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayAOEAnimation()
    {
        if (animator) animator.SetTrigger("AOE");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayAOEImpactEffects()
    {
        PlayParticleEffect(bigChunkVFX);
        PlayParticleEffect(smallChunkVFX);
        PlayParticleEffect(dustVFX);

        if (audioSource && aoeImpactSound)
        AudioManager.PlaySoundAtSource(aoeImpactSound, audioSource);
    }

    private void PlayParticleEffect(ParticleSystem ps)
    {
        if (ps == null) return;
        
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        ps.Play();
    }

    private void SubscribeToHealthManager()
    {
        if (healthManager == null) return;

        healthManager.OnEncounterVictory -= HandleGameOver;
        healthManager.OnPartyWipe -= HandleGameOver;
        
        healthManager.OnEncounterVictory += HandleGameOver;
        healthManager.OnPartyWipe += HandleGameOver;
    }

    private void UnsubscribeFromHealthManager()
    {
        if (healthManager == null) return;
        healthManager.OnEncounterVictory -= HandleGameOver;
        healthManager.OnPartyWipe -= HandleGameOver;
    }

    private void HandleGameOver()
    {
        if (!Object.HasStateAuthority || EncounterOver) return;

        EncounterOver = true;
        IsAttacking = false;
        
        RPC_StopBossPresentation();
        
        Debug.Log("(BossAI): Boss has stopped attacking. Encounter ended.");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_StopBossPresentation()
    {
        StopAllCoroutines();
        
        if (animator) animator.ResetTrigger("Swipe");
        
        StopParticleEffect(bigChunkVFX);
        StopParticleEffect(smallChunkVFX);
        StopParticleEffect(dustVFX);
    }
    
    private static void StopParticleEffect(ParticleSystem effect)
    {
        if (effect == null) return;
        
        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}