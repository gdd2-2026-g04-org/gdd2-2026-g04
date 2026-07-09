using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class XRGestureSimulator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode legacySimulationKey = KeyCode.U;
    [SerializeField] private float circleRadius = 0.22f; // Adjusted slightly for standard casting size
    [SerializeField] private float drawDuration = 1.0f;

    private bool isSimulating = false;

    private void Update()
    {
        bool uKeyPressed = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            uKeyPressed = Keyboard.current.uKey.wasPressedThisFrame;
        }
#endif

        if (!uKeyPressed)
        {
            try
            {
                uKeyPressed = Input.GetKeyDown(legacySimulationKey);
            }
            catch
            {
                // Legacy system disabled
            }
        }

        if (uKeyPressed && !isSimulating)
        {
            StartCoroutine(SimulateCircleRoutine());
        }
    }

    private IEnumerator SimulateCircleRoutine()
    {
        isSimulating = true;

        HealerBook healerBook = FindFirstObjectByType<HealerBook>();
        if (healerBook == null)
        {
            Debug.LogError("XRGestureSimulator: HealerBook not found.");
            isSimulating = false;
            yield break;
        }

        if (XRReferences.Instance == null || XRReferences.Instance.leftHand == null)
        {
            Debug.LogError("XRGestureSimulator: Left Hand reference missing.");
            isSimulating = false;
            yield break;
        }

        Transform leftHand = XRReferences.Instance.leftHand;
        Vector3 originalPosition = leftHand.position;
        Quaternion originalRotation = leftHand.rotation;

        var controllers = leftHand.GetComponentsInChildren<MonoBehaviour>();
        var disabledComponents = new List<MonoBehaviour>();

        foreach (var comp in controllers)
        {
            string typeName = comp.GetType().Name;
            if (typeName.Contains("Controller") || typeName.Contains("TrackedPose") || typeName.Contains("Driver"))
            {
                if (comp.enabled)
                {
                    comp.enabled = false;
                    disabledComponents.Add(comp);
                }
            }
        }

        Vector3 u = Vector3.right;
        Vector3 v = Vector3.forward;

        Vector3 startOffset = u * circleRadius;
        leftHand.position = originalPosition + startOffset;

        TrailRenderer trail = healerBook.GetComponentInChildren<TrailRenderer>();
        if (trail != null)
        {
            trail.Clear();
        }

        yield return new WaitForSeconds(0.05f); 

        healerBook.ForceTriggerActive = true;
        yield return null;

        float elapsed = 0f;
        while (elapsed < drawDuration)
        {
            float angle = (elapsed / drawDuration) * Mathf.PI * 2f;
            Vector3 offset = (u * Mathf.Cos(angle) + v * Mathf.Sin(angle)) * circleRadius;

            leftHand.position = originalPosition + offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        leftHand.position = originalPosition + startOffset;
        yield return new WaitForSeconds(0.05f); 

        healerBook.ForceTriggerActive = false;
        yield return null;

        leftHand.position = originalPosition;
        leftHand.rotation = originalRotation;

        foreach (var comp in disabledComponents)
        {
            if (comp != null) comp.enabled = true;
        }

        isSimulating = false;
        Debug.Log("XRGestureSimulator: Horizontal circle completed successfully.");
    }
}