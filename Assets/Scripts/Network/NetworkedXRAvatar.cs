using System;
using Fusion;
using UnityEngine;

public class NetworkedXRAvatar : NetworkBehaviour
{
    [Header("Networked Avatar Parts")]
    [SerializeField] private Transform head;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    [SerializeField] private GameObject[] hideForLocalPlayer;

    [Header("Warrior Weapon Prefabs")]
    [SerializeField] private GameObject swordPrefab;
    [SerializeField] private GameObject shieldPrefab;

    // Networked properties that sync automatically to all players
    [Networked] public PlayerClass SelectedClass { get; set; }
    [Networked] public NetworkBool IsReady { get; set; }

    private XRReferences XRreferences;
    
    // Store local references to spawned weapon objects so we can clean them up if class changes
    private GameObject instantiatedSword;
    private GameObject instantiatedShield;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            XRreferences = XRReferences.Instance;

            foreach (var obj in hideForLocalPlayer)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
    }

    private void LateUpdate()
    {
        if (!Object.HasStateAuthority) return;
        
        if (!XRreferences) XRreferences = XRReferences.Instance;
        if (!XRreferences) return;
        
        CopyTransform(XRreferences.head, head);
        CopyTransform(XRreferences.leftHand, leftHand);
        CopyTransform(XRreferences.rightHand, rightHand);
    }

    private void CopyTransform(Transform src, Transform t)
    {
        if (!src || !t) return;
        t.SetPositionAndRotation(src.position, src.rotation);
    }

    // Fusion executes this whenever a networked property is updated
    public override void Render()
    {
        UpdateWeaponVisuals(SelectedClass);
    }

    private void UpdateWeaponVisuals(PlayerClass currentClass)
    {
        // Clean up previous weapons first if they exist
        if (currentClass != PlayerClass.Warrior)
        {
            if (instantiatedSword != null) Destroy(instantiatedSword);
            if (instantiatedShield != null) Destroy(instantiatedShield);
            return;
        }

        // If the class is Warrior and weapons are not yet spawned, spawn them
        if (currentClass == PlayerClass.Warrior)
        {
            if (instantiatedSword == null && rightHand != null && swordPrefab != null)
            {
                instantiatedSword = Instantiate(swordPrefab, rightHand);
                SetupEquippedWeapon(instantiatedSword);
            }

            if (instantiatedShield == null && leftHand != null && shieldPrefab != null)
            {
                instantiatedShield = Instantiate(shieldPrefab, leftHand);
                SetupEquippedWeapon(instantiatedShield);
            }
        }
    }

    private void SetupEquippedWeapon(GameObject weaponObj)
    {
        // 1. Reset local position to sit perfectly on the hand anchor
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;

        // 2. Disable physics and drop capabilities so they stay locked to the avatar's hands
        if (weaponObj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 3. Disable the XR Grab component so players can't drop them on the floor
        if (weaponObj.TryGetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>(out var grab))
        {
            grab.enabled = false;
        }
    }
}