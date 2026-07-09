using UnityEngine;
using UnityEngine.InputSystem;

public class BowInput : MonoBehaviour
{
    [Header("Mode")]
    public bool debugMode = true;

    [Header("VR References")]
    public Transform vrLeft;
    public Transform vrRight;

    [Header("Debug Hands")]
    public Transform debugLeft;
    public Transform debugRight;

    public Vector3 LeftHandPosition { get; private set; }
    public Vector3 RightHandPosition { get; private set; }

    public bool triggerPressed { get; private set; }

    void Update()
    {
        HandlePosition();
        HandleInput();
    }

    void HandlePosition()
    {
        if (debugMode)
        {
            LeftHandPosition = debugLeft.position;
            RightHandPosition = debugRight.position;
        }
        else
        {
            LeftHandPosition = vrLeft.position;
            RightHandPosition = vrRight.position;
        }
    }

    void HandleInput()
    {
        if (!debugMode) return;

        // LEFT HAND (WASD)
        float speed = 1.5f * Time.deltaTime;

        if (Keyboard.current.wKey.isPressed)
            debugLeft.position += Vector3.right * speed;

        if (Keyboard.current.sKey.isPressed)
            debugLeft.position += Vector3.left * speed;

        if (Keyboard.current.aKey.isPressed)
            debugLeft.position += Vector3.forward * speed;

        if (Keyboard.current.dKey.isPressed)
            debugLeft.position += Vector3.back * speed;

        if (Keyboard.current.eKey.isPressed)
            debugLeft.position += Vector3.up * speed;

        if (Keyboard.current.qKey.isPressed)
            debugLeft.position += Vector3.down * speed;

        // RIGHT HAND (FLECHAS)
        if (Keyboard.current.upArrowKey.isPressed)
            debugRight.position += Vector3.right * speed;

        if (Keyboard.current.downArrowKey.isPressed)
            debugRight.position += Vector3.left * speed;

        if (Keyboard.current.leftArrowKey.isPressed)
            debugRight.position += Vector3.forward * speed;

        if (Keyboard.current.rightArrowKey.isPressed)
            debugRight.position += Vector3.back * speed;

        // TRIGGER BUTTON (SPACEBAR)
        triggerPressed = Keyboard.current.spaceKey.isPressed;
    }
}