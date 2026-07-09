using UnityEngine;
using UnityEngine.InputSystem;

public class BowInput : MonoBehaviour
{
    // [Header("Mode")]
    // public bool keyboardDebug = true;

    [SerializeField] private InputActionReference drawAction;
    
    public Vector3 LeftHandPosition => XRReferences.Instance && XRReferences.Instance.leftHand ? XRReferences.Instance.leftHand.position : Vector3.zero;
    public Vector3 RightHandPosition => XRReferences.Instance && XRReferences.Instance.rightHand ? XRReferences.Instance.rightHand.position : Vector3.zero;

    public bool DrawPressed => drawAction && drawAction.action.IsPressed();
    
    public bool IsAvailable => XRReferences.Instance && XRReferences.Instance.leftHand && XRReferences.Instance.rightHand && drawAction;

    /*void Update()
    {
        if(keyboardDebug)
        HandleInput();
    }
    
    void HandleInput()
    {
        if (!keyboardDebug) return;

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
    }*/
}