using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Shield : MonoBehaviour
{
    [Header("Raised Settings")]
    [SerializeField] private float raiseThreshold = 1.2f;

    public bool isHeld = false;
    public bool isRaised { get; private set; } = false;

    private XRGrabInteractable grabInteractable;
    private Collider shieldCollider;
    private Rigidbody rb;
    private Transform playerRoot;
    private bool wasTrigger;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        shieldCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();

        grabInteractable.selectEntered.AddListener(_ => 
        {
            isHeld = true;
            SetShieldAsTrigger(true);           // ← Prevent damage
        });

        grabInteractable.selectExited.AddListener(_ => 
        {
            isHeld = false;
            isRaised = false;
            SetShieldAsTrigger(false);          // ← Restore normal collider
        });
    }

    private void Start()
    {
        var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null)
            playerRoot = xrOrigin.transform;

        if (shieldCollider != null)
            wasTrigger = shieldCollider.isTrigger;
    }

    private void Update()
    {
        if (!isHeld || playerRoot == null)
        {
            isRaised = false;
            return;
        }

        isRaised = transform.position.y > playerRoot.position.y + raiseThreshold;
    }

    private void SetShieldAsTrigger(bool makeTrigger)
    {
        if (shieldCollider != null)
        {
            shieldCollider.isTrigger = makeTrigger;
        }

        if (rb != null)
        {
            rb.isKinematic = makeTrigger;
        }
    }
}