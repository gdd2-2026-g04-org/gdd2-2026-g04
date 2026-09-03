using System;
using System.Collections.Generic;
using GameAssets.Health;
using UnityEngine;
using UnityEngine.InputSystem;

public class HealerBook : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionProperty triggerAction;

    [Header("Visuals")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private ParticleSystem healSuccessParticles;
    [SerializeField] private Material healSuccessMaterial;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip circleSound;
    [SerializeField] private AudioClip healSound;
    
    [Header("Heal Balance")]
    [SerializeField] private int healAmount = 25;
    [SerializeField] private float healCooldown = 2f;

    [Header("Circle Recognition Settings")]
    [SerializeField] private float minPointDistance = 0.03f;
    [SerializeField] private float closureThreshold = 0.7f;
    [SerializeField] private float minRadius = 0.12f;
    [SerializeField] private float maxRadiusVariance = 0.45f;

    private List<Vector3> points = new List<Vector3>();
    private HealthSystemManager healthManager;
    private Material circleMat;
    private float lastHealTime = float.NegativeInfinity;
    private bool isDrawing;

    private void Awake()
    {
        circleMat = trailRenderer.material;
    }

    private void OnEnable()
    {
        healthManager = FindFirstObjectByType<HealthSystemManager>();
        
        if (trailRenderer)
        {
            trailRenderer.emitting = false;
            trailRenderer.Clear();
        }
    }

    private void OnDisable()
    {
        ResetDrawing();
    }

public bool ForceTriggerActive { get; set; }

    private void Update()
    {
        if (!CanAct())
        {
            if (isDrawing) ResetDrawing();
            return;
        }
        
        if (Time.time < lastHealTime + healCooldown)
        {
            if (isDrawing) ResetDrawing();
            return;
        }

        bool triggerHeld = ForceTriggerActive || (triggerAction.action != null && triggerAction.action.ReadValue<float>() > 0.5f);

        if (triggerHeld)
        {
            if (!isDrawing)
            {
                StartDrawing();
            }
            ContinueDrawing();
        }
        else
        {
            if (isDrawing)
            {
                ResetDrawing();
            }
        }
    }

    private void StartDrawing()
    {
        isDrawing = true;
        points.Clear();

        trailRenderer.material = circleMat;
        
        Vector3 startPos = trailRenderer ? trailRenderer.transform.position : transform.position;
        points.Add(startPos);

        if (trailRenderer)
        {
            trailRenderer.Clear();
            trailRenderer.emitting = true;
            audioSource.Play();
        }
    }

    private void ContinueDrawing()
    {
        Vector3 currentPos = trailRenderer ? trailRenderer.transform.position : transform.position;
        float dist = Vector3.Distance(currentPos, points[points.Count - 1]);

        if (dist > minPointDistance)
        {
            points.Add(currentPos);
            CheckForCircleGesture();
        }
    }

    private void ResetDrawing()
    {
        isDrawing = false;
        points.Clear();
        if (trailRenderer)
        {
            trailRenderer.emitting = false;
            audioSource.Stop();
        }
    }

    private void CheckForCircleGesture()
    {

        if (points.Count < 10) return;

        Vector3 center = Vector3.zero;
        foreach (var p in points)
        {
            center += p;
        }
        center /= points.Count;

        Vector3 normal = Vector3.forward;
        if (XRReferences.Instance != null && XRReferences.Instance.head != null)
        {
            normal = XRReferences.Instance.head.forward;
        }

        Vector3 u = Vector3.Cross(normal, Vector3.up).normalized;
        if (u.sqrMagnitude < 0.001f)
        {
            u = Vector3.Cross(normal, Vector3.right).normalized;
        }
        Vector3 v = Vector3.Cross(normal, u).normalized;

        List<Vector2> points2D = new List<Vector2>();
        float totalRadius = 0f;

        foreach (var p in points)
        {
            Vector3 offset = p - center;
            float x = Vector3.Dot(offset, u);
            float y = Vector3.Dot(offset, v);
            points2D.Add(new Vector2(x, y));
            totalRadius += Mathf.Sqrt(x * x + y * y);
        }

        float averageRadius = totalRadius / points2D.Count;

        if (averageRadius < minRadius) return;

        float firstLastDist = Vector3.Distance(points[0], points[points.Count - 1]);
        if (firstLastDist > averageRadius * closureThreshold) return;

        float totalPathLength = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            totalPathLength += Vector3.Distance(points[i], points[i - 1]);
        }
        if (totalPathLength < averageRadius * 4.5f) return;

        float totalVariance = 0f;
        foreach (var p2d in points2D)
        {
            float r = p2d.magnitude;
            totalVariance += Mathf.Abs(r - averageRadius);
        }
        float normalizedVariance = (totalVariance / points2D.Count) / averageRadius;

        if (normalizedVariance > maxRadiusVariance) return;

        bool q1 = false, q2 = false, q3 = false, q4 = false;
        foreach (var p2d in points2D)
        {
            if (p2d.x >= 0 && p2d.y >= 0) q1 = true;
            else if (p2d.x < 0 && p2d.y >= 0) q2 = true;
            else if (p2d.x < 0 && p2d.y < 0) q3 = true;
            else if (p2d.x >= 0 && p2d.y < 0) q4 = true;
        }

        if (q1 && q2 && q3 && q4)
        {
            TriggerPartyHeal();
        }
    }

    private void TriggerPartyHeal()
    {
        ResetDrawing();

        if (!CanAct()) return;
        
        if (!healthManager) healthManager = FindFirstObjectByType<HealthSystemManager>();
        
        healthManager.HealAllPlayers(healAmount);
        lastHealTime = Time.time;
        
        trailRenderer.material = healSuccessMaterial;
        AudioManager.PlaySoundAtSource(healSound, audioSource);
        
        if (healSuccessParticles)
        {
            healSuccessParticles.transform.position = transform.position;
            healSuccessParticles.Play();
        }
        
        Debug.Log($"(HealerBook): Restored {healAmount} HP to all party members.");
    }

    private PlayerHealth playerHealth;
    
    private bool CanAct()
    {
        if (!playerHealth && NetworkManager.Instance)
        {
            playerHealth = NetworkManager.Instance.LocalPlayerHealth;
        }
        return playerHealth && playerHealth.IsAlive;
    }
}