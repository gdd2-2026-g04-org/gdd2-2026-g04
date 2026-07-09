using System;
using GameAssets.Health;
using GameAssets.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BowState
{
    Idle,
    Drawing,
    Ready
}

public enum ShotResult
{
    Hit,
    ReleasedEarly,
    TimeOut,
    Miss
}

public class BowController : MonoBehaviour
{
    [Header("Input Source")]
    public BowInput input;

    [Header("Settings")]
    public float minDrawDistance = 0.5f;
    public float maxDrawDistance = 1.50f;
    [SerializeField] private WeaponData weapon;
    [SerializeField, Min(0f)] private float cooldown = 1f;
    
    [Header("Aim")]
    public LineRenderer aimLine;
    public float aimLineLength = 20f;

    [Header("Aim Stabilization")]
    [SerializeField, Min(0.001f)]
    private float aimSmoothTime = 0.04f;

    [SerializeField, Min(0f)]
    private float aimSmoothSpeed = 18f;

    private Vector3 smoothedAimOrigin;
    private Vector3 aimOriginVelocity;
    private Vector3 smoothedAimDirection;
    
    [Header("QTE")]
    public QTECircleController qteCircle;

    public Vector3 AimOrigin { get; private set; }
    public Vector3 AimDirection { get; private set; }
    public float Tension { get; private set; }
    public BowState State { get; private set;}
    
    private HealthSystemManager healthManager;
    private PlayerHealth playerHealth;
    
    private float lastShotTime = float.NegativeInfinity;

    private void Awake()
    {
        if (!aimLine) return;

        aimLine.useWorldSpace = true;
        aimLine.positionCount = 2;
        aimLine.enabled = false;
    }

    private void OnEnable()
    {
        ResetBow();
        ResolveReferences();

        if (XRReferences.Instance)
        {
            smoothedAimOrigin = XRReferences.Instance.leftHand.position;

            smoothedAimDirection = XRReferences.Instance.leftHand.forward;
        }
    }

    private void OnDisable()
    {
        ResetBow();
    }

    private void Update()
    {
        if (!input || !input.IsAvailable || !aimLine || !qteCircle) return;
        
        if (!healthManager || !playerHealth) ResolveReferences();

        aimLine.enabled = false;

        var leftHandPos = input.LeftHandPosition;
        var rightHandPos = input.RightHandPosition;
        var handDistance = Vector3.Distance(leftHandPos, rightHandPos);

        switch (State)
        {
            case BowState.Idle:
                UpdateIdle(handDistance);
                break;
            case BowState.Drawing:
                UpdateDrawing(handDistance);
                break;
            case BowState.Ready:
                UpdateReady(leftHandPos, rightHandPos, handDistance);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateIdle(float handDistance)
    {
        if (!input.DrawPressed) return;

        if (Time.time < lastShotTime + cooldown) return;

        if (handDistance > minDrawDistance) return;

        State = BowState.Drawing;
        Tension = 0f;
        Debug.Log("(BowController): Started drawing...");
    }

    private void UpdateDrawing(float handDistance)
    {
        if (!input.DrawPressed)
        {
            FinishShot(ShotResult.ReleasedEarly);
            return;
        }
        
        UpdateTension(handDistance);

        if (Tension < 1f) return;

        State = BowState.Ready;
        
        qteCircle.ShowCircle();
        
        Debug.Log("(BowController): Bow is fully drawn!");
    }

    private void UpdateReady(Vector3 leftHandPos, Vector3 rightHandPos, float handDistance)
    {
        aimLine.enabled = true;
        
        UpdateAimLine(leftHandPos, rightHandPos);

        if (qteCircle.TimeOut)
        {
            FinishShot(ShotResult.TimeOut);
            return;
        }

        if (!input.DrawPressed)
        {
            var result = qteCircle.IsAimInside ? ShotResult.Hit : ShotResult.Miss;
            
            FinishShot(result);
            return;
        }
        
        UpdateTension(handDistance);

        if (Tension < 1f)
        {
            qteCircle.HideCircle();
            State = BowState.Drawing;
            
            Debug.Log("(BowController): Cancelled shot!");
        }
    }

    private void UpdateTension(float handDistance)
    {
        Tension = Mathf.Clamp01(Mathf.InverseLerp(minDrawDistance, maxDrawDistance, handDistance));
    }

    private void UpdateAimLine(Vector3 leftHandPos, Vector3 rightHandPos)
    {
        var targetOrigin = leftHandPos;

        var targetDir = XRReferences.Instance.leftHand.forward.normalized;

        smoothedAimOrigin = Vector3.SmoothDamp(smoothedAimOrigin, targetOrigin, ref aimOriginVelocity, aimSmoothTime);

        var lerpFac = 1f - Mathf.Exp(-aimSmoothSpeed * Time.deltaTime);

        smoothedAimDirection = Vector3.Slerp(smoothedAimDirection, targetDir, lerpFac).normalized;

        AimOrigin = smoothedAimOrigin;
        AimDirection = smoothedAimDirection;
        
        aimLine.SetPosition(0, AimOrigin);
        
        aimLine.SetPosition(1, AimOrigin + AimDirection * aimLineLength);

        /*AimOrigin = leftHandPos;

        /*var dir = leftHandPos -  rightHandPos;

        if (dir.sqrMagnitude <= 0.0001f)
        {
            AimDirection = Vector3.zero;
            return;
        }

        AimDirection = dir.normalized;#1#
        AimDirection = XRReferences.Instance.leftHand.forward;

        aimLine.SetPosition(0, AimOrigin);

        aimLine.SetPosition(1, AimOrigin + AimDirection * aimLineLength);*/
    }

    private void FinishShot(ShotResult result)
    {
        switch (result)
        {
            case ShotResult.Hit:
                ApplySuccessfulShot();
                break;
            case ShotResult.ReleasedEarly:
                Debug.Log("(BowController): Released too early!");
                break;
            case ShotResult.TimeOut:
                Debug.Log("(BowController): Shot timed out!");
                break;
            case ShotResult.Miss:
                Debug.Log("(BowController): Missed!");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }

        lastShotTime = Time.time;
        
        ResetBow();
    }

    private void ApplySuccessfulShot()
    {
        ResolveReferences();

        if (!healthManager)
        {
            Debug.LogWarning("(BowController): Missing HealthSystemManager!");
            return;
        }

        var boss = healthManager.Boss;

        if (!boss || !boss.IsSpawned || !boss.IsAlive) return;

        var damage = weapon.damage + playerHealth.Damage;
        
        healthManager.ApplyDamageToBoss(damage);
        
        Debug.Log($"(Bow): Requested {damage} against {boss.name}");
    }

    private void ResolveReferences()
    {
        if (!healthManager) healthManager = FindFirstObjectByType<HealthSystemManager>();

        if (!playerHealth && NetworkManager.Instance) playerHealth = NetworkManager.Instance.LocalPlayerHealth;
    }

    private void ResetBow()
    {
        State = BowState.Idle;
        Tension = 0f;

        AimOrigin = Vector3.zero;
        AimDirection = Vector3.zero;

        if (aimLine) aimLine.enabled = false;
        
        if (qteCircle) qteCircle.HideCircle();
    }

}