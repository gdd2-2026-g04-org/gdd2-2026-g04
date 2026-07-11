using GameAssets.Health;
using UnityEngine;

public class MageProjectile : MonoBehaviour
{
    [SerializeField] private int damage = 15;
    [SerializeField] private float lifetime = 5f;

    [Header("Throw Settings")]
    [SerializeField] private float flightTime = 1.2f;
    [SerializeField] private float gravity = 18f;
    [SerializeField] private float hitDistanceThreshold = 0.8f;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Throw Visual")]
    [SerializeField] private Vector3 tumbleSpeed = new Vector3(180f, 90f, 270f);

    private HealthSystemManager healthSystem;
    private Transform target;
    private Vector3 currentVelocity;
    private bool hasHit;

    public void Initialize(Transform targetBoss, HealthSystemManager manager)
    {
        target = targetBoss;
        healthSystem = manager;

        // Compute ballistic launch velocity to reach target in flightTime under gravity
        Vector3 targetPosition = target.position + targetOffset;
        Vector3 displacement = targetPosition - transform.position;
        Vector3 flatDisplacement = new Vector3(displacement.x, 0f, displacement.z);

        float vy = (displacement.y + 0.5f * gravity * flightTime * flightTime) / flightTime;
        float horizontalSpeed = flatDisplacement.magnitude / flightTime;
        Vector3 vFlat = flatDisplacement.normalized * horizontalSpeed;

        currentVelocity = new Vector3(vFlat.x, vy, vFlat.z);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (hasHit) return;

        // Apply gravity
        currentVelocity.y -= gravity * Time.deltaTime;

        // Tumble spin (visual only)
        transform.Rotate(tumbleSpeed * Time.deltaTime, Space.Self);

        // Move
        transform.position += currentVelocity * Time.deltaTime;

        // Hit check against boss
        if (target != null)
        {
            Vector3 targetPosition = target.position + targetOffset;
            if (Vector3.Distance(transform.position, targetPosition) <= hitDistanceThreshold)
                TriggerHit();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        BossHealth boss = other.GetComponent<BossHealth>() ?? other.GetComponentInParent<BossHealth>();
        if (boss != null)
            TriggerHit();
    }

    private void TriggerHit()
    {
        if (hasHit) return;
        hasHit = true;

        if (healthSystem)
        {
            healthSystem.ApplyDamageToBoss(damage);
            Debug.Log($"[MageProjectile] Hit the Boss and dealt {damage} damage.");
        }

        Destroy(gameObject);
    }
}