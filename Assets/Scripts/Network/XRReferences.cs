using System;
using UnityEngine;

public class XRReferences : MonoBehaviour
{
    public static XRReferences Instance { get; private set; }

    [Header("XR References")]
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    private void Awake()
    {
        Instance = this;
    }
}