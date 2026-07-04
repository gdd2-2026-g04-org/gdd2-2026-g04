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
    
    private XRReferences XRreferences;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        XRreferences = XRReferences.Instance;

        foreach (var obj in hideForLocalPlayer)
        {
            if (obj != null) obj.SetActive(false);
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
}
