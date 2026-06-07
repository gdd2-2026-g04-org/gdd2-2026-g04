using UnityEngine;


public class Shield : MonoBehaviour
{
    // We will reference this from your Health script
    public bool isHeld = false;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        // Listen for when the player grabs or releases the shield
        grabInteractable.selectEntered.AddListener((x) => isHeld = true);
        grabInteractable.selectExited.AddListener((x) => isHeld = false);
    }
}