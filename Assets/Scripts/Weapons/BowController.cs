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

    [Header("Aim")]
    public LineRenderer aimLine;
    public float aimLineLength = 20f;

    [Header("QTE")]
    public QTECircleController qteCircle;

    public Vector3 AimOrigin { get; private set; }
    public Vector3 AimDirection { get; private set; }

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
                    FinishShot(ShotResult.ReleasedEarly);
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

                    qteCircle.ShowCircle();
                }

            break;

            //====================================================
            // READY
            //====================================================

            case BowState.Ready:

                aimLine.enabled = true;
                UpdateAimLine(left, right);

                // Circle closed before shot
                if (qteCircle.TimeOut)
                {
                    FinishShot(ShotResult.TimeOut);
                    return;
                }

                // Button released to shoot
                if (!input.triggerPressed)
                {
                    if (qteCircle.IsAimInside)
                    {
                        FinishShot(ShotResult.Hit);
                    }
                    else
                    {
                        FinishShot(ShotResult.Miss);
                    }

                    return;
                }

                // if tension goes down again, return to DRAWING
                tension = Mathf.InverseLerp(minDrawDistance, maxDrawDistance, distance);
                tension = Mathf.Clamp01(tension);

                if (tension < 1f)
                {
                    Debug.Log("Back to DRAWING");

                    qteCircle.HideCircle();

                    bowState = BowState.Drawing;
                }

            break;
        }
    }

    private void UpdateAimLine(Vector3 left, Vector3 right)
    {
        AimOrigin = left;
        AimDirection = (left - right).normalized;

        aimLine.SetPosition(0, AimOrigin);
        aimLine.SetPosition(1, AimOrigin + AimDirection * aimLineLength);
    }

    private void FinishShot(ShotResult result)
    {
        switch (result)
        {
            case ShotResult.Hit:
                Debug.Log("HIT"); //Damage Calcs here
            break;

            case ShotResult.ReleasedEarly:
                Debug.Log("FAILED: Released too early"); //skip turn
            break;

            case ShotResult.TimeOut:
                Debug.Log("FAILED: Time out"); //skip turn
            break;

            case ShotResult.Miss:
                Debug.Log("FAILED: Missed circle"); //skip turn
            break;
        }

        bowState = BowState.Idle;
        tension = 0f;

        qteCircle.HideCircle();
    }

}