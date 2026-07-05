using UnityEngine;
using UnityEngine.InputSystem;

public enum BowState
{
    Idle,
    Drawing,
    Ready
}

public class BowController : MonoBehaviour
{
    [Header("Input Source")]
    public BowInput input;

    [Header("Settings")]
    public float minDrawDistance = 0.5f;
    public float maxDrawDistance = 1.50f;

    [Header("Aim")]
    public LineRenderer aimLine;
    public float aimLineLength = 20f;

    private BowState bowState = BowState.Idle;

    private float tension = 0f;

    void Update()
    {
        aimLine.enabled = false;

        Vector3 left = input.LeftHandPosition;
        Vector3 right = input.RightHandPosition;

        float distance = Vector3.Distance(left, right);

        if(Keyboard.current.xKey.isPressed)
            Debug.Log("DISTANCE: " + distance);

        switch (bowState)
        {
            //====================================================
            // IDLE
            //====================================================

            case BowState.Idle:

                // button not pressed
                if (!input.triggerPressed)
                    return;

                // hands too far appart
                if (distance > minDrawDistance)
                {
                    Debug.Log("Hands are too far apart.");
                    return;
                }

                // hands close enough to start the draw
                bowState = BowState.Drawing;
                Debug.Log("START DRAW");

            break;

            //====================================================
            // DRAWING
            //====================================================

            case BowState.Drawing:

                // Bow not fully drawed
                if (!input.triggerPressed)
                {
                    Debug.Log("SHOT FAILED");

                    bowState = BowState.Idle;
                    tension = 0f;

                    return;
                }

                
                tension = Mathf.InverseLerp(minDrawDistance, maxDrawDistance, distance);
                tension = Mathf.Clamp01(tension);

                Debug.Log("Current tension: " + tension);

                //Bow fully drawed
                if (tension >= 1f)
                {
                    bowState = BowState.Ready;
                    Debug.Log("BOW READY");
                }

            break;

            //====================================================
            // READY
            //====================================================

            case BowState.Ready:

                aimLine.enabled = true;
                UpdateAimLine(left, right);

                // Button released to shoot
                if (!input.triggerPressed)
                {
                    Debug.Log("SHOT FIRED");

                    bowState = BowState.Idle;
                    tension = 0f;

                    aimLine.enabled = false;

                    return;
                }

                // if tension goes down again, return to DRAWING
                tension = Mathf.InverseLerp(minDrawDistance, maxDrawDistance, distance);
                tension = Mathf.Clamp01(tension);

                if (tension < 1f)
                {
                    Debug.Log("Back to DRAWING");

                    bowState = BowState.Drawing;
                }

            break;
        }
    }

    private void UpdateAimLine(Vector3 left, Vector3 right)
    {
        Vector3 aimDirection = (left - right).normalized;

        aimLine.SetPosition(0, left);
        aimLine.SetPosition(1, left + aimDirection * aimLineLength);
    }

}