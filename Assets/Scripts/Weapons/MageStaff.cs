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

    private HealthSystemManager healthManager;
    private MageMana mana;

    private enum GestureState { Idle, Winding, Primed }
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
        #if UNITY_EDITOR
        if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
        {
            FireEnergyBall();
            return;
        }
        #endif

        if (mana == null) mana = FindFirstObjectByType<MageMana>();

        if (healthManager == null || healthManager.Boss == null) return;

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

        bool triggerHeld = triggerAction.action != null && triggerAction.action.ReadValue<float>() > 0.5f;
        if (!triggerHeld)
        {
            ResetCharge();
            return;
        }

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
                    gestureState = GestureState.Primed;
                    if (chargeParticles && !chargeParticles.isPlaying) chargeParticles.Play();
                    if (staffGlowObject) staffGlowObject.SetActive(true);
                    Debug.Log("(MageStaff): Fully Charged! Release trigger to shoot.");
                }
                else if (forwardSpeed > 1f)
                {
                    gestureState = GestureState.Idle;
                }
                break;

            case GestureState.Primed:
                if (forwardSpeed >= thrustSpeed && Time.time >= lastFireTime + fireCooldown)
                {
                    FireEnergyBall();
                    gestureState = GestureState.Idle;
                    ResetCharge();
                }
                break;
        }
    }

    private void FireEnergyBall()
    {
        if (mana != null && !mana.TrySpend(manaPerShot))
            return;

        GameObject projectileObj = Instantiate(energyBallPrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        MageProjectile projectile = projectileObj.GetComponent<MageProjectile>();

        if (projectile)
        {
            projectile.Initialize(healthManager.Boss.transform, healthManager);
        }
        else
        {
            Debug.LogError("[MageStaff] The instantiated energyBallPrefab is missing the MageProjectile script on its root object!", energyBallPrefab);
        }

        if (shootParticles)
        {
            shootParticles.transform.position = projectileSpawnPoint.position;
            shootParticles.Play();
        }

        lastFireTime = Time.time;
    }

    private void ResetCharge()
    {
        gestureState = GestureState.Idle;
        if (chargeParticles) chargeParticles.Stop();
        if (staffGlowObject) staffGlowObject.SetActive(false);
    }
}