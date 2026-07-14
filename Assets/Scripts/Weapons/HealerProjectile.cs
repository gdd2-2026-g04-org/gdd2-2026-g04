using GameAssets.Health;
using UnityEngine;

public class HealerProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 15;
    [SerializeField] private float lifetime = 5f;

    [Header("Homing Settings")]
    [Tooltip("Offset to target the chest of the boss instead of their feet on the floor.")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.2f, 0f);
    
    [Tooltip("How fast the projectile curves towards the boss (degrees per second).")]
    [SerializeField] private float turnSpeed = 180f; 

    [Tooltip("How close the ball needs to get to the chest to count as a hit.")]
    [SerializeField] private float hitDistanceThreshold = 0.6f;

    private Transform target;
    private HealthSystemManager healthSystem;
    private Vector3 currentVelocity;
    private bool hasHit;
    private bool dealDamage;

    private void Start()
    {
        currentVelocity = transform.forward * speed;
    }

    public void Initialize(Transform targetBoss, HealthSystemManager manager, bool dealDamage)
    {
        target = targetBoss;
        healthSystem = manager;
        this.dealDamage = dealDamage;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (hasHit) return;

        if (target == null)
        {
            transform.position += currentVelocity * Time.deltaTime;
            return;
        }

        Vector3 targetPosition = target.position + targetOffset;
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;

        if (turnSpeed > 0)
        {
            currentVelocity = Vector3.RotateTowards(
                currentVelocity, 
                directionToTarget * speed, 
                turnSpeed * Mathf.Deg2Rad * Time.deltaTime, 
                0f
            );
        }
        else
        {
            currentVelocity = directionToTarget * speed;
        }

        transform.position += currentVelocity * Time.deltaTime;

        if (currentVelocity != Vector3.zero)
        {
            transform.forward = currentVelocity.normalized;
        }

        float distanceToBoss = Vector3.Distance(transform.position, targetPosition);
        if (distanceToBoss <= hitDistanceThreshold)
        {
            TriggerHit();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        BossHealth boss = other.GetComponent<BossHealth>() ?? other.GetComponentInParent<BossHealth>();
        if (boss != null)
        {
            TriggerHit();
        }
    }

    private void TriggerHit()
    {
        if (hasHit) return;
        hasHit = true;

        if (dealDamage && healthSystem)
        {
            healthSystem.ApplyDamageToBoss(damage);
            Debug.Log($"[HealerProjectile] Successfully hit the Boss and dealt {damage} damage.");
        }

        Destroy(gameObject);
    }
}