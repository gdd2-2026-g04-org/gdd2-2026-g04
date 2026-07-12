using UnityEngine;
using UnityEngine.InputSystem;

public class MageBeer : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionProperty triggerAction;

    [Header("Drink Settings")]
    [SerializeField] private int manaPerDrink = 30;
    [SerializeField] private float drinkCooldown = 3f;
    [SerializeField] private float raiseThreshold = 0.3f;
    [SerializeField] private float holdDuration = 1f;

    [Header("Visuals")]
    [SerializeField] private ParticleSystem drinkParticles;

    private MageMana mana;
    private float lastDrinkTime = float.NegativeInfinity;
    private float holdTimer;

    private void OnEnable()
    {
        if (triggerAction.action != null) triggerAction.action.Enable();
        mana = FindFirstObjectByType<MageMana>();
    }

    private void OnDisable()
    {
        if (triggerAction.action != null) triggerAction.action.Disable();
        holdTimer = 0f;
    }

    private void Update()
    {
        if (TryHandleDebugDrink()) return;

        if (mana == null) mana = FindFirstObjectByType<MageMana>();

        if (XRReferences.Instance?.leftHand == null || XRReferences.Instance?.head == null) return;

        if (Time.time < lastDrinkTime + drinkCooldown) return;

        bool triggerHeld = triggerAction.action != null && triggerAction.action.ReadValue<float>() > 0.5f;
        bool handRaised = XRReferences.Instance.leftHand.position.y >
                          XRReferences.Instance.head.position.y + raiseThreshold;

        if (triggerHeld && handRaised)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdDuration)
            {
                TryDrink();
                holdTimer = 0f;
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    private void TryDrink()
    {
        if (Time.time < lastDrinkTime + drinkCooldown) return;

        if (mana == null) return;
        if (mana.CurrentMana >= mana.MaxMana) return;

        mana.Restore(manaPerDrink);
        lastDrinkTime = Time.time;

        if (drinkParticles) drinkParticles.Play();
    }

    private bool TryHandleDebugDrink()
    {
        #if UNITY_EDITOR
        if (Keyboard.current == null || !Keyboard.current.bKey.wasPressedThisFrame) return false;

        Debug.Log("[MageBeer] B key pressed - debug drink.");
        TryDrink();
        return true;
        #else
        return false;
        #endif
    }
}
