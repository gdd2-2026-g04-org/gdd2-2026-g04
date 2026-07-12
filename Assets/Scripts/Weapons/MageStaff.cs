using GameAssets.Health;
using UnityEngine;
using UnityEngine.InputSystem;

public class MageStaff : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionProperty triggerAction;

    [Header("Charge Settings")]
    [SerializeField] private float windUpDistance = 0.2f;
    [SerializeField] private float thrustSpeed = 2.0f;
    [SerializeField] private float fireCooldown = 1.2f;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject energyBallPrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Visual Feedback")]
    [SerializeField] private ParticleSystem chargeParticles;
    [SerializeField] private ParticleSystem shootParticles;
    [SerializeField] private GameObject staffGlowObject;

    [Header("Mana")]
    [SerializeField] private int manaPerShot = 15;

    [Header("Overcharge")]
    [SerializeField] private MageQTECircleController qteCircle;
    [SerializeField] private GameObject overchargePrefab;
    [SerializeField] private int manaPerOvercharge = 30;

    private HealthSystemManager healthManager;
    private MageMana mana;

    private enum GestureState { Idle, Winding, Primed, Overcharged }

    public Vector3 AimOrigin { get; private set; }
    public Vector3 AimDirection { get; private set; }
    private GestureState gestureState = GestureState.Idle;
    private Vector3 lastHandPos;
    private Vector3 windUpAnchor;
    private float lastFireTime = float.NegativeInfinity;

    private void OnEnable()
    {
        if (triggerAction.action != null) triggerAction.action.Enable();
        healthManager = FindFirstObjectByType<HealthSystemManager>();
        mana = FindFirstObjectByType<MageMana>();
        ResetCharge();
        if (XRReferences.Instance?.rightHand != null)
            lastHandPos = XRReferences.Instance.rightHand.position;
    }

    private void OnDisable()
    {
        if (triggerAction.action != null) triggerAction.action.Disable();
        ResetCharge();
    }

    private void Update()
    {
        if (TryHandleDebugFire()) return;

        ResolveReferences();
        UpdateAim();

        if (healthManager == null || healthManager.Boss == null)
        {
            #if UNITY_EDITOR
            if (gestureState == GestureState.Idle) return;
            #else
            return;
            #endif
        }

        bool triggerHeld = triggerAction.action != null && triggerAction.action.ReadValue<float>() > 0.5f;
        #if UNITY_EDITOR
        if (gestureState == GestureState.Primed || gestureState == GestureState.Overcharged) triggerHeld = true;
        #endif

        if (!triggerHeld)
        {
            if (gestureState == GestureState.Primed && Time.time >= lastFireTime + fireCooldown)
            {
                float power = qteCircle ? qteCircle.Power : 0f;
                if (qteCircle) qteCircle.HideCircle();
                FireEnergyBall(power);
                ResetCharge();
            }
            else if (gestureState == GestureState.Overcharged && Time.time >= lastFireTime + fireCooldown)
            {
                FireOvercharge();
                ResetCharge();
            }
            else
            {
                ResetCharge();
            }
            return;
        }

        if (gestureState == GestureState.Idle)
        {
            if (mana != null && !mana.HasMana(manaPerShot))
            {
                Debug.Log("(MageStaff): Not enough mana to charge!");
                return;
            }
            EnterPrimedState();
        }

        if (gestureState == GestureState.Primed && qteCircle && qteCircle.TimeOut)
        {
            gestureState = GestureState.Overcharged;
            Debug.Log("(MageStaff): Overcharged!");
        }

        /* ===== MOVEMENT LOGIC DISABLED - Button-based charging only =====
        Vector3 currentPos = XRReferences.Instance.rightHand.position;

        float dt = Time.deltaTime;
        if (dt <= 0f) { lastHandPos = currentPos; return; }

        Vector3 velocity = (currentPos - lastHandPos) / dt;
        lastHandPos = currentPos;

        Vector3 forward = XRReferences.Instance.head.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) return;
        forward.Normalize();

        float forwardSpeed = Vector3.Dot(velocity, forward);

        switch (gestureState)
        {
            case GestureState.Idle:
                if (forwardSpeed < -0.3f)
                {
                    gestureState = GestureState.Winding;
                    windUpAnchor = currentPos;
                }
                break;

            case GestureState.Winding:
                float backDist = Vector3.Dot(windUpAnchor - currentPos, forward);
                if (backDist >= windUpDistance)
                {
                    EnterPrimedState();
                    Debug.Log("(MageStaff): Fully Charged! Release trigger to shoot.");
                }
                else if (forwardSpeed > 1f)
                {
                    gestureState = GestureState.Idle;
                }
                break;

            case GestureState.Primed:
                if (qteCircle && qteCircle.TimeOut)
                {
                    gestureState = GestureState.Overcharged;
                    Debug.Log("(MageStaff): Overcharged! Throw it!");
                    break;
                }
                if (forwardSpeed >= thrustSpeed && Time.time >= lastFireTime + fireCooldown)
                {
                    float power = qteCircle ? qteCircle.Power : 0f;
                    if (qteCircle) qteCircle.HideCircle();
                    FireEnergyBall(power);
                    gestureState = GestureState.Idle;
                    ResetCharge();
                }
                break;

            case GestureState.Overcharged:
                if (forwardSpeed >= thrustSpeed && Time.time >= lastFireTime + fireCooldown)
                {
                    FireOvercharge();
                    gestureState = GestureState.Idle;
                    ResetCharge();
                }
                break;
        }
        ===== END DISABLED MOVEMENT LOGIC ===== */
    }

    private void FireEnergyBall(float power = 0f)
    {
        if (healthManager == null || healthManager.Boss == null)
        {
            Debug.LogWarning("[MageStaff] Can't fire — no boss in scene.");
            return;
        }

        if (mana != null && !mana.TrySpend(manaPerShot))
        {
            Debug.LogWarning("[MageStaff] Not enough mana.");
            return;
        }

        GameObject projectileObj = Instantiate(energyBallPrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        MageProjectile projectile = projectileObj.GetComponent<MageProjectile>();

        if (projectile)
        {
            projectile.Initialize(healthManager.Boss.transform, healthManager, power);
        }
        else
        {
            Debug.LogError("[MageStaff] energyBallPrefab is missing MageProjectile on root!", energyBallPrefab);
        }

        if (shootParticles)
        {
            shootParticles.transform.position = projectileSpawnPoint.position;
            shootParticles.Play();
        }

        lastFireTime = Time.time;
    }

    private void FireOvercharge()
    {
        if (overchargePrefab == null)
        {
            Debug.LogWarning("[MageStaff] No overcharge prefab assigned.");
            return;
        }
        if (healthManager == null || healthManager.Boss == null)
        {
            Debug.LogWarning("[MageStaff] Can't overcharge fire — no boss in scene.");
            return;
        }
        if (mana != null && !mana.TrySpend(manaPerOvercharge))
        {
            Debug.LogWarning("[MageStaff] Not enough mana for overcharge.");
            return;
        }

        GameObject obj = Instantiate(overchargePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        MageProjectile projectile = obj.GetComponent<MageProjectile>();
        if (projectile)
            projectile.Initialize(healthManager.Boss.transform, healthManager);
        else
            Debug.LogError("[MageStaff] overchargePrefab is missing MageProjectile!", overchargePrefab);

        lastFireTime = Time.time;
    }

    private void ResolveReferences()
    {
        if (healthManager == null) healthManager = FindFirstObjectByType<HealthSystemManager>();
        if (mana == null) mana = FindFirstObjectByType<MageMana>();
    }

    private void UpdateAim()
    {
        if (XRReferences.Instance?.rightHand == null || XRReferences.Instance?.head == null) return;

        AimOrigin = XRReferences.Instance.rightHand.position;
        AimDirection = XRReferences.Instance.head.forward;
    }

    private void EnterPrimedState()
    {
        gestureState = GestureState.Primed;

        if (chargeParticles && !chargeParticles.isPlaying) chargeParticles.Play();
        if (staffGlowObject) staffGlowObject.SetActive(true);
        if (qteCircle) qteCircle.ShowCircle();
    }

    private bool TryHandleDebugFire()
    {
        #if UNITY_EDITOR
        if (Keyboard.current == null || !Keyboard.current.cKey.wasPressedThisFrame) return false;

        ResolveReferences();

        if (gestureState == GestureState.Idle || gestureState == GestureState.Winding)
        {
            EnterPrimedState();
            Debug.Log("(MageStaff) [DEBUG]: Jumped to Primed.");
        }
        else if (gestureState == GestureState.Primed)
        {
            float power = qteCircle ? qteCircle.Power : 0f;
            if (qteCircle) qteCircle.HideCircle();
            FireEnergyBall(power);
            ResetCharge();
        }
        else if (gestureState == GestureState.Overcharged)
        {
            FireOvercharge();
            ResetCharge();
        }

        return true;
        #else
        return false;
        #endif
    }

    private void ResetCharge()
    {
        gestureState = GestureState.Idle;
        if (chargeParticles) chargeParticles.Stop();
        if (staffGlowObject) staffGlowObject.SetActive(false);
        if (qteCircle) qteCircle.HideCircle();
    }
}